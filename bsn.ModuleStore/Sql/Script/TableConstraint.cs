namespace bsn.ModuleStore.Sql.Script {
	public abstract class TableConstraint: TableDefinition {
		private readonly ConstraintName constraintName;

		protected TableConstraint(ConstraintName constraintName) {
			this.constraintName = constraintName;
		}

		public ConstraintName ConstraintName {
			get {
				return constraintName;
			}
		}
	}
}