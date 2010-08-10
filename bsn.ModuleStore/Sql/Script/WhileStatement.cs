using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class WhileStatement: Statement {
		private readonly Expression expression;
		private readonly Statement statement;

		[Rule("<WhileStatement> ::= WHILE <Expression> <StatementGroup>", ConstructorParameterMapping = new[] {1, 2})]
		public WhileStatement(Expression expression, Statement statement) {
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			if (statement == null) {
				throw new ArgumentNullException("statement");
			}
			this.expression = expression;
			this.statement = statement;
		}

		public Expression Expression {
			get {
				return expression;
			}
		}

		public Statement Statement {
			get {
				return statement;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("WHILE ");
			writer.WriteScript(expression);
			writer.Write(' ');
			writer.WriteScript(statement);
		}
	}
}