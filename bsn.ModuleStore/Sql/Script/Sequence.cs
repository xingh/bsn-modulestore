using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public class Sequence<T>: SqlToken, IEnumerable<T> where T: SqlToken {
		private readonly T item;
		private readonly Sequence<T> next;

		[Rule("<CursorOptionList> ::=", typeof(Identifier))]
		public Sequence(): this(null, null) {}

		[Rule("<ColumnNameList> ::= <ColumnName>", typeof(ColumnName))]
		[Rule("<StatementList> ::= <StatementGroup>", typeof(SqlStatement))]
		[Rule("<StatementList> ::= <StatementGroup> <Terminator>", typeof(SqlStatement), AllowTruncationForConstructor = true)]
		[Rule("<OpenxmlColumnList> ::= <OpenxmlColumn>", typeof(OpenxmlColumn))]
		[Rule("<DeclareItemList> ::= <DeclareItem>", typeof(VariableDeclaration))]
		[Rule("<SetValueList> ::= <SetValue>", typeof(SqlToken))]
		[Rule("<FulltextColumnList> ::= <FulltextColumn>", typeof(FulltextColumn))]
		[Rule("<FunctionParameterList> ::= <FunctionParameter>", typeof(FunctionParameter))]
		[Rule("<ProcedureParameterList> ::= <ProcedureParameter>", typeof(ProcedureParameter))]
		[Rule("<ExecuteParameterList> ::= <ExecuteParameter>", typeof(ExecuteParameter))]
		[Rule("<TableDefinitionList> ::= <TableDefinition>", typeof(TableDefinition))]
		public Sequence(T item): this(item, null) {}

		[Rule("<ColumnNameList> ::= <ColumnName> ',' <ColumnNameList>", typeof(ColumnName), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<StatementList> ::= <StatementGroup> <Terminator> <StatementList>", typeof(SqlStatement), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<CursorOptionList> ::= Id <CursorOptionList>", typeof(Identifier))]
		[Rule("<OpenxmlColumnList> ::= <OpenxmlColumn> ',' <OpenxmlColumnList>", typeof(OpenxmlColumn), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<DeclareItemList> ::= <DeclareItem> ',' <DeclareItemList>", typeof(VariableDeclaration), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<SetValueList> ::= <SetValue> <SetValueList>", typeof(SqlToken))]
		[Rule("<FulltextColumnList> ::= <FulltextColumn> ',' <FulltextColumnList>", typeof(FulltextColumn), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<FunctionParameterList> ::= <FunctionParameter> ',' <FunctionParameterList>", typeof(FunctionParameter), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<ProcedureParameterList> ::= <ProcedureParameter> ',' <ProcedureParameterList>", typeof(ProcedureParameter), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<ExecuteParameterList> ::= <ExecuteParameter> ',' <ExecuteParameterList>", typeof(ExecuteParameter), ConstructorParameterMapping = new[] {0, 2})]
		[Rule("<TableDefinitionList> ::= <TableDefinition> ',' <TableDefinitionList>", typeof(TableDefinition), ConstructorParameterMapping = new[] {0, 2})]
		public Sequence(T item, Sequence<T> next) {
			this.next = next;
			this.item = item;
		}

		public T Item {
			get {
				return item;
			}
		}

		public Sequence<T> Next {
			get {
				return next;
			}
		}

		public override void WriteTo(TextWriter writer) {
			throw new NotSupportedException();
		}

		public IEnumerator<T> GetEnumerator() {
			for (Sequence<T> sequence = this; sequence != null; sequence = sequence.Next) {
				if (sequence.Item != null) {
					yield return sequence.Item;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}