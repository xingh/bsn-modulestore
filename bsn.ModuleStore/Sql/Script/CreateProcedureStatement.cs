using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class CreateProcedureStatement: CreateStatement {
		private readonly StatementBlock body;
		private readonly bool forReplication;
		private readonly List<ProcedureParameter> parameters;
		private readonly ProcedureName procedureName;
		private readonly bool recompile;

		[Rule("<CreateProcedureStatement> ::= CREATE PROCEDURE <ProcedureName> <ProcedureParameterGroup> <ProcedureOptionGroup> <ProcedureFor> AS <StatementBlock>", ConstructorParameterMapping = new[] {2, 3, 4, 5, 7})]
		public CreateProcedureStatement(ProcedureName procedureName, Optional<Sequence<ProcedureParameter>> parameters, Optional<WithRecompileToken> recompile, Optional<ForReplicationToken> forReplication, StatementBlock body) {
			if (procedureName == null) {
				throw new ArgumentNullException("procedureName");
			}
			if (body == null) {
				throw new ArgumentNullException("body");
			}
			this.procedureName = procedureName;
			this.parameters = parameters.ToList();
			this.recompile = recompile.HasValue();
			this.forReplication = forReplication.HasValue();
			this.body = body;
		}

		public StatementBlock Body {
			get {
				return body;
			}
		}

		public bool ForReplication {
			get {
				return forReplication;
			}
		}

		public List<ProcedureParameter> Parameters {
			get {
				return parameters;
			}
		}

		public ProcedureName ProcedureName {
			get {
				return procedureName;
			}
		}

		public bool Recompile {
			get {
				return recompile;
			}
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("CREATE PROCEDURE ");
			writer.WriteScript(procedureName);
			writer.WriteSequence(parameters, " ", ",", null);
			if (recompile) {
				writer.Write(" WITH RECOMPILE");
			}
			if (forReplication) {
				writer.Write(" FOR REPLICATION");
			}
			writer.WriteLine(" AS");
			writer.WriteScript(body);
		}
	}
}