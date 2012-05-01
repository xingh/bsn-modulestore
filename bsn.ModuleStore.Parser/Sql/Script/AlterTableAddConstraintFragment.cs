// bsn ModuleStore database versioning
// -----------------------------------
// 
// Copyright 2010 by Ars�ne von Wyss - avw@gmx.ch
// 
// Development has been supported by Sirius Technologies AG, Basel
// 
// Source:
// 
// https://bsn-modulestore.googlecode.com/hg/
// 
// License:
// 
// The library is distributed under the GNU Lesser General Public License:
// http://www.gnu.org/licenses/lgpl.html
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using bsn.ModuleStore.Sql.Script.Tokens;

namespace bsn.ModuleStore.Sql.Script {
	internal class AlterTableAddConstraintFragment: StatementFragment<CreateTableStatement> {
		private readonly TableConstraint constraint;

		public AlterTableAddConstraintFragment(CreateTableStatement owner, TableConstraint constraint): base(owner) {
			this.constraint = constraint;
		}

		public TableConstraint Constraint {
			get {
				return constraint;
			}
		}

		public override ObjectCategory ObjectCategory {
			get {
				return ObjectCategory.Constraint;
			}
		}

		public override string ObjectName {
			get {
				return constraint.ConstraintName.Value;
			}
		}

		public override IInstallStatement CreateDropStatement() {
			return new AlterTableDropConstraintStatement(Owner.TableName, constraint.ConstraintName);
		}

		public override void WriteTo(SqlWriter writer) {
			new AlterTableAddStatement(Owner.TableName, new TableWithCheckToken(), new Sequence<TableDefinition>(constraint)).WriteTo(writer);
		}
	}
}