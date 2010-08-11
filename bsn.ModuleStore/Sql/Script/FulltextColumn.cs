using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class FulltextColumn: SqlToken, IScriptable {
		private readonly ColumnName columnName;
		private readonly LanguageLcid language;
		private readonly Qualified<TypeName> typeColumn;

		[Rule("<FulltextColumn> ::= <ColumnName> <FulltextColumnType> <OptionalLanguage>")]
		public FulltextColumn(ColumnName columnName, Optional<Qualified<TypeName>> typeColumn, Optional<LanguageLcid> language) {
			if (columnName == null) {
				throw new ArgumentNullException("columnName");
			}
			this.columnName = columnName;
			this.typeColumn = typeColumn;
			this.language = language;
		}

		public ColumnName ColumnName {
			get {
				return columnName;
			}
		}

		public LanguageLcid Language {
			get {
				return language;
			}
		}

		public Qualified<TypeName> TypeColumn {
			get {
				return typeColumn;
			}
		}

		public void WriteTo(TextWriter writer) {
			writer.WriteScript(columnName);
			writer.WriteScript(typeColumn, " TYPE COLUMN ", null);
			writer.WriteScript(language, " ", null);
		}
	}
}