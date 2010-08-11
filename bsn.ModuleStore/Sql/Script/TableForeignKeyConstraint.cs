﻿using System;
using System.Collections.Generic;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class TableForeignKeyConstraint: TableConstraint {
		private readonly List<ColumnName> columnNames;
		private readonly List<ForeignKeyAction> keyActions;
		private readonly List<ColumnName> refColumnNames;
		private readonly TableName refTableName;

		[Rule("<TableConstraint> ::= FOREIGN KEY '(' <ColumnNameList> ')' REFERENCES <TableName> <ColumnNameGroup> <ForeignKeyActionList>", ConstructorParameterMapping = new[] {3, 6, 7, 8})]
		public TableForeignKeyConstraint(Sequence<ColumnName> columnNames, TableName refTableName, Optional<Sequence<ColumnName>> refColumnNames, Sequence<ForeignKeyAction> keyActions): this(null, columnNames, refTableName, refColumnNames, keyActions) {}

		[Rule("<TableConstraint> ::= CONSTRAINT <ConstraintName> FOREIGN KEY '(' <ColumnNameList> ')' REFERENCES <TableName> <ColumnNameGroup> <ForeignKeyActionList>", ConstructorParameterMapping = new[] {1, 5, 8, 9, 10})]
		public TableForeignKeyConstraint(ConstraintName constraintName, Sequence<ColumnName> columnNames, TableName refTableName, Optional<Sequence<ColumnName>> refColumnNames, Sequence<ForeignKeyAction> keyActions): base(constraintName) {
			if (refTableName == null) {
				throw new ArgumentNullException("refTableName");
			}
			this.columnNames = columnNames.ToList();
			this.refTableName = refTableName;
			this.refColumnNames = refColumnNames.ToList();
			this.keyActions = keyActions.ToList();
		}

		public List<ColumnName> ColumnNames {
			get {
				return columnNames;
			}
		}

		public List<ForeignKeyAction> KeyActions {
			get {
				return keyActions;
			}
		}

		public List<ColumnName> RefColumnNames {
			get {
				return refColumnNames;
			}
		}

		public TableName RefTableName {
			get {
				return refTableName;
			}
		}

		public override void WriteTo(TextWriter writer) {
			base.WriteTo(writer);
			writer.Write("FOREIGN KEY (");
			writer.WriteSequence(columnNames, null, ", ", null);
			writer.Write(") REFERENCES ");
			writer.WriteScript(refTableName);
			if (refColumnNames.Count > 0) {
				writer.Write(" (");
				writer.WriteSequence(refColumnNames, null, ", ", null);
				writer.Write(')');
			}
			writer.WriteSequence(keyActions, " ", null, null);
		}
	}
}