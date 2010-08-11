﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class ExpressionConvertFunction: ExpressionFunction {
		private readonly TypeName typeName;
		private readonly Expression valueExpression;
		private readonly IntegerLiteral style;

		[Rule("<ExpressionFunction> ::= CONVERT '(' <TypeName> ',' <Expression> ')'", ConstructorParameterMapping=new[] { 2, 4 })]
		public ExpressionConvertFunction(TypeName typeName, Expression valueExpression): this(typeName, valueExpression, null) {}

		[Rule("<ExpressionFunction> ::= CONVERT '(' <TypeName> ',' <Expression> ',' IntegerLiteral ')'", ConstructorParameterMapping=new[] { 2, 4, 6 })]
		public ExpressionConvertFunction(TypeName typeName, Expression valueExpression, IntegerLiteral style)
				: base() {
			if (typeName == null) {
				throw new ArgumentNullException("typeName");
			}
			if (valueExpression == null) {
				throw new ArgumentNullException("valueExpression");
			}
			this.typeName = typeName;
			this.valueExpression = valueExpression;
			this.style = style;
		}

		public TypeName TypeName {
			get {
				return typeName;
			}
		}
		public Expression ValueExpression {
			get {
				return valueExpression;
			}
		}
		public IntegerLiteral Style {
			get {
				return style;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("CONVERT(");
			writer.WriteScript(typeName);
			writer.Write(", ");
			writer.WriteScript(valueExpression);
			writer.WriteScript(style, ", ", null);
			writer.Write(')');
		}
	}
}