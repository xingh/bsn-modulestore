using System;
using System.Diagnostics;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class OpenxmlColumn: SqlScriptableToken {
		private readonly ColumnName columnName;
		private readonly StringLiteral columnPattern;
		private readonly TypeName columnType;

		[Rule("<OpenxmlColumn> ::= <ColumnName> <TypeName>")]
		public OpenxmlColumn(ColumnName columnName, TypeName columnType): this(columnName, columnType, null) {}

		[Rule("<OpenxmlColumn> ::= <ColumnName> <TypeName> <StringLiteral>")]
		public OpenxmlColumn(ColumnName columnName, TypeName columnType, StringLiteral columnPattern) {
			Debug.Assert(columnName != null);
			Debug.Assert(columnType != null);
			this.columnName = columnName;
			this.columnType = columnType;
			this.columnPattern = columnPattern;
		}

		public ColumnName ColumnName {
			get {
				return columnName;
			}
		}

		public StringLiteral ColumnPattern {
			get {
				return columnPattern;
			}
		}

		public TypeName ColumnType {
			get {
				return columnType;
			}
		}

		public override void WriteTo(SqlWriter writer) {
			writer.WriteScript(columnName, WhitespacePadding.None);
			writer.WriteScript(columnType, WhitespacePadding.SpaceBefore);
			writer.WriteScript(columnPattern, WhitespacePadding.SpaceBefore);
		}
	}
}