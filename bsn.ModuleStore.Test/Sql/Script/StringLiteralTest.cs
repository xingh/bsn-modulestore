﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace bsn.ModuleStore.Sql.Script {
	[TestFixture]
	public class StringLiteralTest: AssertionHelper {
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ParseIsUnicodeNull() {
			StringLiteral.ParseIsUnicode(null);
		}

		[Test]
		public void ParseIsUnicodeFalse() {
			Expect(!StringLiteral.ParseIsUnicode("'Plain ol'' ASCII'"));
		}

		[Test]
		public void ParseIsUnicodeTrueUpper() {
			Expect(StringLiteral.ParseIsUnicode("N'This isn''t ANSI! äöü'"));
		}

		[Test]
		public void ParseIsUnicodeTrueLower() {
			Expect(StringLiteral.ParseIsUnicode("n'This isn''t ANSI! äöü'"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ParseValueNull() {
			StringLiteral.ParseIsUnicode(null);
		}

		[Test]
		public void ParseValueAscii() {
			Expect(StringLiteral.ParseValue("'Plain ol'' ASCII'"), EqualTo("Plain ol' ASCII"));
		}

		[Test]
		public void ParseValueEscapeStartAscii() {
			Expect(StringLiteral.ParseValue("'''Start'"), EqualTo("'Start"));
		}

		[Test]
		public void ParseValueEscapeStartUnicode() {
			Expect(StringLiteral.ParseValue("N'''Start'"), EqualTo("'Start"));
		}

		[Test]
		public void ParseValueEscapeEnd() {
			Expect(StringLiteral.ParseValue("'End'''"), EqualTo("End'"));
		}

		[Test]
		public void ParseValueEscapeOnlyAscii() {
			Expect(StringLiteral.ParseValue("''''"), EqualTo("'"));
		}

		[Test]
		public void ParseValueEscapeOnlyUnicode() {
			Expect(StringLiteral.ParseValue("N''''"), EqualTo("'"));
		}

		[Test]
		public void ParseValueEmptyAscii() {
			Expect(StringLiteral.ParseValue("''"), EqualTo(""));
		}

		[Test]
		public void ParseValueEmptyUnicode() {
			Expect(StringLiteral.ParseValue("N''"), EqualTo(""));
		}

		[Test]
		public void ParseValueUnicode() {
			Expect(StringLiteral.ParseValue("N'This isn''t ANSI! äöü'"), EqualTo("This isn't ANSI! äöü"));
		}

		[Test]
		public void ParseValueSimple() {
			Expect(StringLiteral.ParseValue("'A string'"), EqualTo("A string"));
		}
	}
}