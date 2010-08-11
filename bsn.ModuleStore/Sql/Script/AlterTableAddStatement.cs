using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	public class AlterTableAddStatement: AlterTableStatement {
		private readonly TableCheck check;
		private readonly List<TableDefinition> definitions;

		[Rule("<AlterTableStatement> ::= ALTER TABLE <TableName> <TableCheck> ADD <TableDefinitionList>", ConstructorParameterMapping = new[] {2, 3, 5})]
		public AlterTableAddStatement(TableName tableName, TableCheckToken check, Sequence<TableDefinition> definitions): base(tableName) {
			Debug.Assert(check != null);
			Debug.Assert(definitions != null);
			this.check = check.TableCheck;
			this.definitions = definitions.ToList();
		}

		public override void ApplyTo(CreateTableStatement createTable) {
			throw new NotImplementedException();
		}

		public override void WriteTo(TextWriter writer) {
			base.WriteTo(writer);
			writer.WriteValue(check, null, " ");
			writer.Write("ADD ");
			writer.WriteSequence(definitions, null, ", ", null);
		}
	}
}