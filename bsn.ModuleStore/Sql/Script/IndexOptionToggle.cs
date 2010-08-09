﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class IndexOptionToggle: IndexOption {
		private readonly bool value;

		[Rule("<IndexOption> ::= Id '=' <Toggle>", ConstructorParameterMapping=new[] { 0, 2 })]
		public IndexOptionToggle(Identifier key, ToggleToken value)
				: base(key) {
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			this.value = value.On;
		}

		public bool Value {
			get {
				return value;
			}
		}

		public override void WriteTo(TextWriter writer) {
			base.WriteTo(writer);
			writer.WriteToggle(value, null, null);
		}
	}
}