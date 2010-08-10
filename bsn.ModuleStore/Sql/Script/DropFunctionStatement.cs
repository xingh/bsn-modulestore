using System;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class DropFunctionStatement: DropStatement {
		private readonly FunctionName functionName;

		[Rule("<DropFunctionStatement> ::= DROP FUNCTION <FunctionName>", ConstructorParameterMapping = new[] {2})]
		public DropFunctionStatement(FunctionName functionName) {
			if (functionName == null) {
				throw new ArgumentNullException("functionName");
			}
			this.functionName = functionName;
		}

		public FunctionName FunctionName {
			get {
				return functionName;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("DROP FUNCTION ");
			writer.WriteScript(functionName);
		}
	}
}