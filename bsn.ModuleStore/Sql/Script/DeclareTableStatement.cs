using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class DeclareTableStatement: DeclareStatement {
		private readonly List<TableDefinition> tableDefinitions;

		[Rule("<DeclareStatement> ::= DECLARE <VariableName> <OptionalAs> TABLE <TableDefinitionGroup>", ConstructorParameterMapping = new[] {1, 4})]
		public DeclareTableStatement(VariableName name, Sequence<TableDefinition> tableDefinitions): base(name) {
			this.tableDefinitions = tableDefinitions.ToList();
		}

		public override void WriteTo(TextWriter writer) {
			writer.Write("DECLARE ");
			writer.WriteScript(VariableName);
			writer.WriteLine(" TABLE (");
			writer.WriteSequence(tableDefinitions, "\t", ",", Environment.NewLine);
			writer.WriteLine(")");
		}
	}
}