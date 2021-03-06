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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace bsn.ModuleStore.Mapper.Serialization {
	public interface ISerializationTypeMapping {
		IDictionary<string, SqlColumnInfo> Columns {
			get;
		}

		ReadOnlyCollection<IMemberConverter> Converters {
			get;
		}

		SqlDbType DbType {
			get;
		}

		bool HasNestedSerializers {
			get;
		}

		bool IsNativeType {
			get;
		}

		int MemberCount {
			get;
		}

		object GetMember(object instance, int index);

		void PopulateInstanceMembers(object result, object[] buffer);
	}
}
