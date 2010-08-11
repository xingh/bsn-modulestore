﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class ExpressionSimpleCase: ExpressionCase<Expression> {
		private readonly Expression inputExpression;

		[Rule("<ExpressionCase> ::= CASE <Expression> <CaseWhenExpressionList> END", ConstructorParameterMapping = new[] {1, 2})]
		public ExpressionSimpleCase(Expression inputExpression, Sequence<CaseWhen<Expression>> whenItems): this(inputExpression, whenItems, null) {}

		[Rule("<ExpressionCase> ::= CASE <Expression> <CaseWhenExpressionList> ELSE <Expression> END", ConstructorParameterMapping = new[] {1, 2, 4})]
		public ExpressionSimpleCase(Expression inputExpression, Sequence<CaseWhen<Expression>> whenItems, Expression elseExpression): base(whenItems, elseExpression) {
			if (inputExpression == null) {
				throw new ArgumentNullException("inputExpression");
			}
			this.inputExpression = inputExpression;
		}

		public Expression InputExpression {
			get {
				return inputExpression;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("CASE ");
			writer.WriteScript(inputExpression);
			base.WriteTo(writer);
		}
	}
}