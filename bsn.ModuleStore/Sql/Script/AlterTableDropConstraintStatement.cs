using System;
using System.IO;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public sealed class AlterTableDropConstraintStatement: AlterTableStatement {
		private readonly ConstraintName constraintName;

		[Rule("<AlterTableStatement> ::= ALTER TABLE <TableName> DROP <ConstraintName>", ConstructorParameterMapping = new[] {2, 4})]
		[Rule("<AlterTableStatement> ::= ALTER TABLE <TableName> DROP CONSTRAINT <ConstraintName>", ConstructorParameterMapping = new[] {2, 5})]
		public AlterTableDropConstraintStatement(TableName tableName, ConstraintName constraintName): base(tableName) {
			if (constraintName == null) {
				throw new ArgumentNullException("constraintName");
			}
			this.constraintName = constraintName;
		}

		public ConstraintName ConstraintName {
			get {
				return constraintName;
			}
		}

		public override void ApplyTo(CreateTableStatement createTable) {
			throw new NotImplementedException();
		}

		public override void WriteTo(TextWriter writer) {
			base.WriteTo(writer);
			writer.Write("DROP CONSTRAINT ");
			writer.WriteScript(constraintName);
		}
	}
}