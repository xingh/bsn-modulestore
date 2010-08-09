﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bsn.ModuleStore.Sql.Script {
	public abstract class CreateIndexStatement: SqlCreateStatement {
		private readonly IndexName indexName;
		private readonly List<IndexOption> indexOptions;
		private readonly TableName tableName;

		protected CreateIndexStatement(IndexName indexName, TableName tableName, Optional<Sequence<IndexOption>> indexOptions) {
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

		protected void WriteOptions(TextWriter writer) {
			if (indexOptions.Count > 0) {
				writer.Write("WITH (");
				writer.WriteSequence(indexOptions, null, ", ", null);
				writer.Write(')');
			}
		}
	}
}