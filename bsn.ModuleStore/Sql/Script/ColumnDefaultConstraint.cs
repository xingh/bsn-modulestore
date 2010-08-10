﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public class ColumnDefaultConstraint: ColumnConstraint {
		private readonly Expression defaultValue;

		[Rule("<ColumnConstraint> ::= DEFAULT <NumberLiteral>", ConstructorParameterMapping = new[] {1})]
		[Rule("<ColumnConstraint> ::= DEFAULT <StringLiteral>", ConstructorParameterMapping = new[] {1})]
		[Rule("<ColumnConstraint> ::= DEFAULT <NullLiteral>", ConstructorParameterMapping = new[] {1})]
		public ColumnDefaultConstraint(Expression defaultValue) {
			if (defaultValue == null) {
				throw new ArgumentNullException("defaultValue");
			}
			this.defaultValue = defaultValue;
		}

		public Expression DefaultValue {
			get {
				return defaultValue;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("DEFAULT ");
			writer.WriteScript(defaultValue);
		}
	}
}