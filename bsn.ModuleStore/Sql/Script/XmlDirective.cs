﻿using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class XmlDirective: SqlToken, IScriptable {
		private readonly StringLiteral elementName;
		private readonly Identifier key;
		private readonly Identifier value;

		[Rule("<XmlDirective> ::= Id Id")]
		public XmlDirective(Identifier key, Identifier value): this(key) {
			this.value = value;
		}

		[Rule("<XmlDirective> ::= Id <OptionalElementName>")]
		public XmlDirective(Identifier key, Optional<StringLiteral> elementName): this(key) {
			this.elementName = elementName;
		}

		private XmlDirective(Identifier key) {
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			this.key = key;
		}

		public void WriteTo(TextWriter writer) {
			writer.WriteScript(key);
			writer.WriteScript(value, " ", null);
			writer.WriteScript(elementName, " ", null);
		}
	}
}