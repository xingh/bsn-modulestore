﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class CreateXmlIndexStatement: CreateIndexStatement {
		private readonly ColumnName columnName;
		private readonly IndexUsing indexUsing;
		private readonly bool primary;

		[Rule("<CreateIndexStatement> ::= CREATE <IndexPrimary> XML_INDEX <IndexName> ON <TableName> '(' <ColumnName> ')' <IndexUsing> <IndexOptionGroup>", ConstructorParameterMapping = new[] {1, 3, 5, 7, 9, 10})]
		public CreateXmlIndexStatement(Optional<PrimaryToken> primary, IndexName indexName, TableName tableName, ColumnName columnName, IndexUsing indexUsing, Optional<Sequence<IndexOption>> indexOptions): base(indexName, tableName, indexOptions) {
			this.primary = primary.HasValue();
			this.columnName = columnName;
			this.indexUsing = indexUsing;
		}

		public ColumnName ColumnName {
			get {
				return columnName;
			}
		}

		public IndexUsing IndexUsing {
			get {
				return indexUsing;
			}
		}

		public bool Primary {
			get {
				return primary;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("CREATE ");
			writer.WritePrimary(primary, null, " ");
			writer.Write("XML INDEX ");
			writer.WriteScript(IndexName);
			writer.Write(" ON ");
			writer.WriteScript(TableName);
			writer.Write(" (");
			writer.WriteScript(columnName);
			writer.Write(") ");
			writer.WriteScript(indexUsing);
			WriteOptions(writer);
		}
	}
}