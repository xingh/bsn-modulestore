using System;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class BreakStatement: Statement {
		[Rule("<BreakStatement> ::= BREAK", AllowTruncationForConstructor = true)]
		public BreakStatement() {}

		public override void WriteTo(SqlWriter writer) {
			writer.Write("BREAK");
		}
	}
}