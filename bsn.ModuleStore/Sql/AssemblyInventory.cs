﻿// bsn ModuleStore database versioning
// -----------------------------------
// 
// Copyright 2010 by Arsène von Wyss - avw@gmx.ch
// 
// Development has been supported by Sirius Technologies AG, Basel
// 
// Source:
// 
// https://bsn-modulestore.googlecode.com/hg/
// 
// License:
// 
// The library is distributed under the GNU Lesser General Public License:
// http://www.gnu.org/licenses/lgpl.html
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Common.Logging;

using bsn.ModuleStore.Sql.Script;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql {
	public class AssemblyInventory: InstallableInventory {
		private static readonly Dictionary<Assembly, AssemblyInventory> cachedInventories = new Dictionary<Assembly, AssemblyInventory>();
		private static readonly ILog log = LogManager.GetLogger<AssemblyInventory>();

		public static AssemblyInventory Get(Assembly assembly) {
			if (assembly == null) {
				throw new ArgumentNullException("assembly");
			}
			lock (cachedInventories) {
				AssemblyInventory result;
				if (!cachedInventories.TryGetValue(assembly, out result)) {
					result = new AssemblyInventory(new AssemblyHandle(assembly));
					cachedInventories.Add(assembly, result);
				}
				return result;
			}
		}

		private static IEnumerable<IInstallStatement> HandleDependendObjects(IInstallStatement statement, DatabaseInventory inventory, ICollection<string> droppedObjects) {
			DependencyDisablingAlterStatement dependencyAltering = statement as DependencyDisablingAlterStatement;
			if (dependencyAltering != null) {
				ICollection<IAlterableCreateStatement> dependencyObjects = dependencyAltering.GetDependencyObjects(inventory, droppedObjects);
				foreach (IAlterableCreateStatement dependencyObject in dependencyObjects) {
					yield return dependencyObject.CreateDropStatement();
				}
				yield return statement;
				foreach (IAlterableCreateStatement dependencyObject in dependencyObjects) {
					yield return dependencyObject;
				}
			} else {
				yield return statement;
			}
		}

		private readonly IAssemblyHandle assembly;
		private readonly ReadOnlyCollection<KeyValuePair<SqlAssemblyAttribute, string>> attributes;
		private readonly List<SqlExceptionMappingAttribute> exceptionMappings = new List<SqlExceptionMappingAttribute>();
		private readonly HashSet<string> processedManifestStreamKeys = new HashSet<string>(StringComparer.Ordinal);
		private readonly int requiredEngineVersion = 9;
		private readonly SortedList<int, IScriptableStatement[]> updateStatements = new SortedList<int, IScriptableStatement[]>();
		private readonly int updateVersion;

		public AssemblyInventory(IAssemblyHandle assembly) {
			this.assembly = assembly;
			attributes = assembly.GetCustomAttributes<SqlAssemblyAttribute>().ToList().AsReadOnly();
			foreach (KeyValuePair<SqlAssemblyAttribute, string> attribute in attributes) {
				SqlRequiredVersionAttribute requiredVersionAttribute = attribute.Key as SqlRequiredVersionAttribute;
				if (requiredVersionAttribute != null) {
					if (requiredVersionAttribute.RequiredEngineVersion > requiredEngineVersion) {
						requiredEngineVersion = requiredVersionAttribute.RequiredEngineVersion;
					}
				} else {
					SqlSetupScriptAttributeBase setupScriptAttribute = attribute.Key as SqlSetupScriptAttributeBase;
					if (setupScriptAttribute != null) {
						string manifestStreamKey;
						using (TextReader reader = OpenText(setupScriptAttribute, attribute.Value, out manifestStreamKey)) {
							if (processedManifestStreamKeys.Add(manifestStreamKey)) {
								try {
									ProcessSingleScript(reader, AddAdditionalSetupStatement);
								} catch (ParseException ex) {
									ex.FileName = setupScriptAttribute.ManifestResourceName;
									throw;
								}
							}
						}
					} else {
						SqlUpdateScriptAttribute updateScriptAttribute = attribute.Key as SqlUpdateScriptAttribute;
						if (updateScriptAttribute != null) {
							if (updateScriptAttribute.Version < 1) {
								string message = string.Format("Update script versions must be at least 1, but {0} was specified (script: {1})", updateScriptAttribute.Version, updateScriptAttribute.ManifestResourceName);
								log.Error(message);
								throw new InvalidOperationException(message);
							}
							using (TextReader reader = OpenText(updateScriptAttribute, attribute.Value)) {
								updateStatements.Add(updateScriptAttribute.Version, ScriptParser.Parse(reader).ToArray());
								updateVersion = Math.Max(updateVersion, updateScriptAttribute.Version);
							}
						} else {
							SqlExceptionMappingAttribute exceptionMappingAttribute = attribute.Key as SqlExceptionMappingAttribute;
							if (exceptionMappingAttribute != null) {
								exceptionMappings.Add(exceptionMappingAttribute);
							} else {
								log.WarnFormat("Unrecognized assembly SQL attribute {0}", attribute.Key.GetType());
							}
						}
					}
				}
			}
			int expectedVersion = 1;
			foreach (KeyValuePair<int, IScriptableStatement[]> update in updateStatements) {
				Debug.Assert(update.Key == expectedVersion);
				StatementSetSchemaOverride(update.Value);
				expectedVersion = update.Key+1;
			}
			AdditionalSetupStatementSetSchemaOverride();
			exceptionMappings.Sort((x, y) => x.ComputeSpecificity()-y.ComputeSpecificity());
		}

		public AssemblyName AssemblyName {
			get {
				return assembly.AssemblyName;
			}
		}

		public ReadOnlyCollection<KeyValuePair<SqlAssemblyAttribute, string>> Attributes {
			get {
				return attributes;
			}
		}

		public List<SqlExceptionMappingAttribute> ExceptionMappings {
			get {
				return exceptionMappings;
			}
		}

		public int RequiredEngineVersion {
			get {
				return requiredEngineVersion;
			}
		}

		public SortedList<int, IScriptableStatement[]> UpdateStatements {
			get {
				return updateStatements;
			}
		}

		public int UpdateVersion {
			get {
				return updateVersion;
			}
		}

		public IEnumerable<string> GenerateUpdateSql(DatabaseInventory inventory, int currentVersion) {
			if (inventory == null) {
				throw new ArgumentNullException("inventory");
			}
			SetQualification(inventory.SchemaName);
			inventory.SetQualification(inventory.SchemaName);
			try {
				DependencyResolver resolver = new DependencyResolver();
				List<IInstallStatement> alterUsingUpdateScript = new List<IInstallStatement>();
				Dictionary<string, IScriptableStatement> dropStatements = new Dictionary<string, IScriptableStatement>(StringComparer.OrdinalIgnoreCase);
				HashSet<string> newObjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				HashSet<string> refreshObjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				refreshObjectNames.UnionWith(inventory.Objects.OfType<CreateViewStatement>().Where(v => !(v.ViewOption is OptionSchemabindingToken)).Select(v => v.ObjectName));
				// refreshObjectNames.UnionWith(inventory.Objects.OfType<CreateFunctionStatement>().Where(f => !(f.Option is OptionSchemabindingToken)).Select(f => f.ObjectName));
				foreach (KeyValuePair<IAlterableCreateStatement, InventoryObjectDifference> pair in Compare(inventory, this, inventory.TargetEngine)) {
					switch (pair.Value) {
					case InventoryObjectDifference.None:
						resolver.AddExistingObject(pair.Key.ObjectName);
						break;
					case InventoryObjectDifference.Different:
						if (pair.Key.AlterUsingUpdateScript) {
							alterUsingUpdateScript.Add(pair.Key);
						} else {
							AlterTableAddConstraintFragment alterConstraint = pair.Key as AlterTableAddConstraintFragment;
							if (alterConstraint != null) {
								dropStatements.Add(pair.Key.ObjectName, pair.Key.CreateDropStatement());
								resolver.Add(pair.Key);
							} else {
								if (pair.Key.DisableUsagesForUpdate) {
									Debug.Assert(!(pair.Key is CreateTableFragment)); // we must not wrap those - but they shouldn't return true for this flag
									resolver.Add(new DependencyDisablingAlterStatement(pair.Key.CreateAlterStatement()));
								} else {
									resolver.Add(pair.Key.CreateAlterStatement());
								}
							}
							refreshObjectNames.Remove(pair.Key.ObjectName);
						}
						break;
					case InventoryObjectDifference.SourceOnly:
						dropStatements.Add(pair.Key.ObjectName, pair.Key.CreateDropStatement());
						refreshObjectNames.Remove(pair.Key.ObjectName);
						break;
					case InventoryObjectDifference.TargetOnly:
						resolver.Add(pair.Key);
						if (pair.Key.IsPartOfSchemaDefinition) {
							newObjectNames.Add(pair.Key.ObjectName);
						}
						break;
					}
				}
				StringBuilder builder = new StringBuilder(4096);
				// first drop table constraints and indices (if any)
				foreach (AlterTableDropConstraintStatement dropStatement in dropStatements.Values.OfType<AlterTableDropConstraintStatement>()) {
					yield return WriteStatement(dropStatement, builder, inventory.TargetEngine);
				}
				foreach (DropIndexStatement dropStatement in dropStatements.Values.OfType<DropIndexStatement>()) {
					yield return WriteStatement(dropStatement, builder, inventory.TargetEngine);
				}
				// now perform all possible actions which do not rely on tables which are altered
				HashSet<string> createdTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (IInstallStatement statement in resolver.GetInOrder(false)) {
					CreateTableFragment createTable = statement as CreateTableFragment;
					if (createTable != null) {
						yield return WriteStatement(createTable.Owner.CreateStatementFragments(CreateFragmentMode.CreateOnExistingSchema).OfType<CreateTableFragment>().Single(), builder, inventory.TargetEngine);
						createdTables.Add(createTable.ObjectName);
					} else if (!statement.IsTableUniqueConstraintOfTables(createdTables)) {
						foreach (IInstallStatement innerStatement in HandleDependendObjects(statement, inventory, dropStatements.Keys)) {
							yield return WriteStatement(innerStatement, builder, inventory.TargetEngine);
						}
					}
				}
				// then perform updates (if any)
				HashSet<string> droppedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<int, IScriptableStatement[]> update in updateStatements.Where(u => u.Key > currentVersion)) {
					foreach (IScriptableStatement statement in update.Value) {
						DropTableStatement dropTable = statement as DropTableStatement;
						if (dropTable != null) {
							droppedTables.Add(dropTable.ObjectName);
						}
						yield return WriteStatement(statement, builder, inventory.TargetEngine);
					}
				}
				// now that the update scripts have updated the tables, mark the tables in the dependency resolver
				foreach (IInstallStatement createTableStatement in alterUsingUpdateScript) {
					resolver.AddExistingObject(createTableStatement.ObjectName);
				}
				// refresh the views and functions
				foreach (string objectName in refreshObjectNames) {
					yield return string.Format("EXEC [sp_refreshsqlmodule] '[{0}].[{1}]'", inventory.SchemaName, objectName);
				}
				// try to perform the remaining actions
				foreach (IInstallStatement statement in resolver.GetInOrder(true).Where(statement => !(statement.IsTableUniqueConstraintOfTables(createdTables) || statement.DependsOnTables(droppedTables)))) {
					foreach (IInstallStatement innerStatement in HandleDependendObjects(statement, inventory, dropStatements.Keys)) {
						yield return WriteStatement(innerStatement, builder, inventory.TargetEngine);
					}
				}
				// execute insert statements for table setup data
				if (AdditionalSetupStatements.Any()) {
					bool disabledChecks = false;
					foreach (IScriptableStatement statement in AdditionalSetupStatements) {
						Qualified<SchemaName, TableName> name = null;
						InsertStatement insertStatement = statement as InsertStatement;
						if (insertStatement != null) {
							DestinationRowset<Qualified<SchemaName, TableName>> targetTable = insertStatement.DestinationRowset as DestinationRowset<Qualified<SchemaName, TableName>>;
							if (targetTable != null) {
								name = targetTable.Name;
							}
						} else {
							SetIdentityInsertStatement setIdentityInsertStatement = statement as SetIdentityInsertStatement;
							if (setIdentityInsertStatement != null) {
								name = setIdentityInsertStatement.TableName;
							}
						}
						if ((name != null) && name.IsQualified && string.Equals(name.Qualification.Value, inventory.SchemaName, StringComparison.OrdinalIgnoreCase) && newObjectNames.Contains(name.Name.Value)) {
							if (!disabledChecks) {
								foreach (CreateTableStatement table in Objects.OfType<CreateTableStatement>()) {
									yield return WriteStatement(new AlterTableNocheckConstraintStatement(table.TableName, new TableCheckToken()), builder, inventory.TargetEngine);
								}
								disabledChecks = true;
							}
							yield return WriteStatement(statement, builder, inventory.TargetEngine);
						}
					}
					if (disabledChecks) {
						foreach (CreateTableStatement table in Objects.OfType<CreateTableStatement>()) {
							yield return WriteStatement(new AlterTableCheckConstraintStatement(table.TableName, new TableWithCheckToken()), builder, inventory.TargetEngine);
						}
					}
				}
				// finally drop objects which are no longer used
				foreach (IScriptableStatement dropStatement in dropStatements.Values.Where(s => !(s is AlterTableDropConstraintStatement || s is DropTableStatement || s is DropIndexStatement || s.DependsOnTables(droppedTables)))) {
					yield return WriteStatement(dropStatement, builder, inventory.TargetEngine);
				}
				// refresh the SPs
				foreach (string objectName in Objects.OfType<CreateProcedureStatement>().Where(sp => !(sp.Option is OptionSchemabindingToken)).Select(sp => sp.ObjectName)) {
					yield return string.Format("EXEC [sp_refreshsqlmodule] '[{0}].[{1}]'", inventory.SchemaName, objectName);
				}
			} finally {
				UnsetQualification();
				inventory.UnsetQualification();
			}
		}

		internal void AssertEngineVersion(int engineVersion) {
			if (engineVersion < RequiredEngineVersion) {
				string message = string.Format("The assembly {0} requires a database engine version {1}, but the database engine version is {2}", assembly.AssemblyName.FullName, requiredEngineVersion, engineVersion);
				log.ErrorFormat(message);
				throw new InvalidOperationException(message);
			}
		}

		private Stream OpenStream(SqlManifestResourceAttribute attribute, string optionalPrefix, out string manifestStreamKey) {
			if (attribute.ManifestResourceType == null) {
				manifestStreamKey = attribute.ManifestResourceName;
			} else {
				manifestStreamKey = attribute.ManifestResourceType.Namespace+Type.Delimiter+attribute.ManifestResourceName;
			}
			Stream result = assembly.GetManifestResourceStream(null, manifestStreamKey);
			if ((result == null) && (attribute.ManifestResourceType == null) && (!string.IsNullOrEmpty(optionalPrefix))) {
				manifestStreamKey = optionalPrefix+Type.Delimiter+attribute.ManifestResourceName;
				result = assembly.GetManifestResourceStream(null, manifestStreamKey);
			}
			if (result == null) {
				log.ErrorFormat("The embedded SQL file {0} was not found", attribute.ManifestResourceName);
				throw new FileNotFoundException("The embedded SQL file was not found", attribute.ManifestResourceName);
			}
			return result;
		}

		private TextReader OpenText(SqlManifestResourceAttribute attribute, string optionalPrefix, out string manifestStreamKey) {
			return new StreamReader(OpenStream(attribute, optionalPrefix, out manifestStreamKey), true);
		}

		private TextReader OpenText(SqlManifestResourceAttribute attribute, string optionalPrefix) {
			string key;
			return OpenText(attribute, optionalPrefix, out key);
		}
	}
}
