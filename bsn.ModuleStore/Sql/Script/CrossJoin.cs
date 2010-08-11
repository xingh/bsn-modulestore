﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class CrossJoin: Join {
		[Rule("<Join> ::= CROSS JOIN <SourceRowset>", ConstructorParameterMapping = new[] {2})]
		public CrossJoin(SourceRowset joinRowset): base(joinRowset) {}

		public override JoinKind Kind {
			get {
				return JoinKind.Cross;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("CROSS ");
			base.WriteTo(writer);
		}
	}
}