﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class DropIndexStatement: DropStatement {
		private readonly IndexName indexName;
		private readonly List<IndexOption> indexOptions;
		private readonly TableName tableName;

		[Rule("<DropIndexStatement> ::= DROP INDEX <IndexName> ON <TableName> <IndexOptionGroup>", ConstructorParameterMapping = new[] {2, 4, 5})]
		public DropIndexStatement(IndexName indexName, TableName tableName, Optional<Sequence<IndexOption>> indexOptions) {
			if (indexName == null) {
				throw new ArgumentNullException("indexName");
			}
			if (tableName == null) {
				throw new ArgumentNullException("tableName");
			}
			this.indexName = indexName;
			this.tableName = tableName;
			this.indexOptions = indexOptions.ToList();
		}

		public IndexName IndexName {
			get {
				return indexName;
			}
		}

		public List<IndexOption> IndexOptions {
			get {
				return indexOptions;
			}
		}

		public TableName TableName {
			get {
				return tableName;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("DROP INDEX ");
			writer.WriteScript(indexName);
			writer.Write(" ON ");
			writer.WriteScript(tableName);
			writer.WriteIndexOptions(indexOptions);
		}
	}
}