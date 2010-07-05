using System;
using System.Globalization;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	[Terminal("HexLiteral")]
	public class IntegerHexLiteral: IntegerLiteral {
		public IntegerHexLiteral(string value): base(int.Parse(value.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo)) {}
	}
}