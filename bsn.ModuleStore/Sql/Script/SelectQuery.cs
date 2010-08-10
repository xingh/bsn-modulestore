﻿using System;
using System.Collections.Generic;
using System.IO;

using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	public class SelectQuery: SqlToken, IScriptable {
		private readonly List<ColumnItem> columnItems;
		private readonly DestinationRowset intoClause;
		private readonly bool? restriction;
		private readonly TopExpression top;
		private readonly UnionClause unionClause;

		[Rule("<SelectQuery> ::= SELECT <Restriction> <TopLegacy> <ColumnItemList> <IntoClause> <UnionClause>", ConstructorParameterMapping = new[] {1, 2, 3, 4, 5})]
		public SelectQuery(TopExpression top, DuplicateRestrictionToken restriction, Sequence<ColumnItem> columnItems, Optional<DestinationRowset> intoClause, UnionClause unionClause) {
			this.top = top;
			this.intoClause = intoClause;
			this.unionClause = unionClause;
			this.columnItems = columnItems.ToList();
			this.restriction = restriction.Distinct;
		}

		public List<ColumnItem> ColumnItems {
			get {
				return columnItems;
			}
		}

		public DestinationRowset IntoClause {
			get {
				return intoClause;
			}
		}

		public bool? Restriction {
			get {
				return restriction;
			}
		}

		public TopExpression Top {
			get {
				return top;
			}
		}

		public UnionClause UnionClause {
			get {
				return unionClause;
			}
		}

		protected virtual void WriteToInternal(TextWriter writer) {}

		public void WriteTo(TextWriter writer) {
			writer.Write("SELECT ");
			writer.WriteDuplicateRestriction(restriction, null, " ");
			writer.WriteScript(top, null, " ");
			writer.WriteSequence(columnItems, null, ", ", null);
			writer.WriteScript(intoClause, " INTO ", null);
			WriteToInternal(writer);
			writer.WriteScript(unionClause, Environment.NewLine, null);
		}
	}
}