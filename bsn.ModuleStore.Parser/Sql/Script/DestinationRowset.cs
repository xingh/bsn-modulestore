﻿// bsn ModuleStore database versioning
// -----------------------------------
// 
// Copyright 2010 by Arsène von Wyss - avw@gmx.ch
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

using System;
using System.Diagnostics;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public abstract class DestinationRowset: SqlScriptableToken {}

	public sealed class DestinationRowset<T>: DestinationRowset where T: SqlScriptableToken {
		private readonly T name;
		private readonly TableHintGroup tableHints;

		[Rule("<DestinationRowset> ::= <VariableName>", typeof(VariableName))]
		public DestinationRowset(T name): this(name, null) {}

		[Rule("<DestinationRowset> ::= <TableNameQualified> <TableHintGroup>", typeof(Qualified<SchemaName, TableName>))]
		public DestinationRowset(T name, TableHintGroup tableHints) {
			Debug.Assert(name != null);
			this.name = name;
			this.tableHints = tableHints;
		}

		public T Name {
			get {
				return name;
			}
		}

		public TableHintGroup TableHints {
			get {
				return tableHints;
			}
		}

		public override void WriteTo(SqlWriter writer) {
			writer.WriteScript(name, WhitespacePadding.None);
			writer.WriteScript(tableHints, WhitespacePadding.SpaceBefore);
		}
	}
}
