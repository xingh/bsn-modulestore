﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public class UpdateStatement: Statement {
		private readonly List<CommonTableExpression> ctes;
		private readonly DestinationRowset destinationRowset;
		private readonly FromClause fromClause;
		private readonly OutputClause outputClause;
		private readonly TopExpression topExpression;
		private readonly List<UpdateItem> updateItems;
		private readonly Predicate whereClause;

		[Rule("<UpdateStatement> ::= <CTEGroup> UPDATE <OptionalTop> <DestinationRowset> SET <UpdateItemList> <OutputClause> <OptionalFromClause> <WhereClause> <QueryHint>", ConstructorParameterMapping = new[] {0, 2, 3, 5, 6, 7, 8})]
		public UpdateStatement(Optional<Sequence<CommonTableExpression>> ctes, TopExpression topExpression, DestinationRowset destinationRowset, Sequence<UpdateItem> updateItems, OutputClause outputClause, Optional<FromClause> fromClause, Optional<Predicate> whereClause) {
			Debug.Assert(destinationRowset != null);
			this.ctes = ctes.ToList();
			this.topExpression = topExpression;
			this.destinationRowset = destinationRowset;
			this.updateItems = updateItems.ToList();
			this.outputClause = outputClause;
			this.fromClause = fromClause;
			this.whereClause = whereClause;
		}

		public List<CommonTableExpression> Ctes {
			get {
				return ctes;
			}
		}

		public DestinationRowset DestinationRowset {
			get {
				return destinationRowset;
			}
		}

		public FromClause FromClause {
			get {
				return fromClause;
			}
		}

		public OutputClause OutputClause {
			get {
				return outputClause;
			}
		}

		public TopExpression TopExpression {
			get {
				return topExpression;
			}
		}

		public List<UpdateItem> UpdateItems {
			get {
				return updateItems;
			}
		}

		public Predicate WhereClause {
			get {
				return whereClause;
			}
		}

		public override void WriteTo(SqlWriter writer) {
			writer.WriteCommonTableExpressions(ctes);
			writer.Write("UPDATE ");
			writer.WriteScript(topExpression, null, " ");
			writer.WriteScript(destinationRowset);
			writer.Write(" SET ");
			writer.WriteSequence(updateItems, null, ", ", null);
			writer.WriteScript(outputClause, " ", null);
			writer.WriteScript(fromClause, " ", null);
			writer.WriteScript(whereClause, " ", null);
		}
	}
}