using System;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class DropProcedureStatement: DropStatement {
		private readonly ProcedureName procedureName;

		[Rule("<DropProcedureStatement> ::= DROP PROCEDURE <ProcedureName>", ConstructorParameterMapping = new[] {2})]
		public DropProcedureStatement(ProcedureName procedureName) {
			if (procedureName == null) {
				throw new ArgumentNullException("procedureName");
			}
			this.procedureName = procedureName;
		}

		public ProcedureName ProcedureName {
			get {
				return procedureName;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("DROP PROCEDURE ");
			writer.WriteScript(procedureName);
		}
	}
}