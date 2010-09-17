using System;
using System.Globalization;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	[Terminal("HexLiteral")]
	public sealed class IntegerHexLiteral: IntegerLiteral {
		public IntegerHexLiteral(string value): base(int.Parse(value.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo)) {}

		public override void WriteTo(SqlWriter writer) {
			WriteCommentsTo(writer);
			writer.Write(string.Format(CultureInfo.InvariantCulture, "0x{0:X}", Value));
		}
	}
}