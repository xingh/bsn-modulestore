using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public class SetVariableStatement: SqlStatement {
		private readonly Expression expression;
		private readonly VariableName variable;

		[Rule("<SetVariableStatement> ::= SET <VariableName> '=' <Expression>", ConstructorParameterMapping = new[] {1, 3})]
		public SetVariableStatement(VariableName variable, Expression expression) {
			if (variable == null) {
				throw new ArgumentNullException("variable");
			}
			if (expression == null) {
				throw new ArgumentNullException("expression");
			}
			this.variable = variable;
			this.expression = expression;
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("SET ");
			variable.WriteTo(writer);
			writer.Write("=");
			expression.WriteTo(writer);
		}
	}
}