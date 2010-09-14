using System;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class LabelName: SqlQuotedName {
		[Rule("<LabelName> ::= Id")]
		public LabelName(Identifier identifier): base(identifier.Value) {}
	}
}