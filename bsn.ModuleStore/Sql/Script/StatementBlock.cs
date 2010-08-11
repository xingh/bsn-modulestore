using System;
using System.Collections.Generic;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class StatementBlock: Statement {
		private readonly List<Statement> statements;

		[Rule("<StatementBlock> ::= BEGIN <StatementList> END", ConstructorParameterMapping = new[] {1})]
		public StatementBlock(Sequence<Statement> statements) {
			this.statements = statements.ToList();
			if (this.statements.Count == 1) {
				StatementBlock innerBlock = this.statements[0] as StatementBlock;
				if (innerBlock != null) {
					this.statements = innerBlock.statements;
				}
			}
		}

		public List<Statement> Statements {
			get {
				return statements;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.WriteLine("BEGIN");
			writer.WriteSequence(statements, "\t", null, ";"+Environment.NewLine);
			writer.Write("END");
		}
	}
}