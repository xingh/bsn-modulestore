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
//  
using System;
using System.Reflection;

namespace bsn.ModuleStore.Mapper.Deserialization {
	internal class CachedMemberConverter: MemberConverter {
		private readonly MemberConverter identityMember;

		public CachedMemberConverter(Type type, int memberIndex, DateTimeKind dateTimeKind): base(type, memberIndex) {
			foreach (FieldInfo field in SqlDeserializerTypeMapping.GetAllFields(type)) {
				SqlColumnAttribute columnAttribute = SqlColumnAttribute.GetColumnAttribute(field, false);
				if ((columnAttribute != null) && columnAttribute.Identity) {
					identityMember = Get(field.FieldType, memberIndex, dateTimeKind);
					break;
				}
			}
			if (identityMember == null) {
				throw new InvalidOperationException(String.Format("The type {0} cannot be retrieved from the cache because it lacks an identity column", type.FullName));
			}
		}

		public override object Process(SqlDeserializer.DeserializerContext context, int column) {
			object identity = identityMember.Process(context, column);
			if (identity != null) {
				InstanceOrigin instanceOrigin;
				object result = context.GetInstance(Type, identity, out instanceOrigin);
				if (instanceOrigin == InstanceOrigin.New) {
					context.ForgetInstance(Type, identity);
					throw new InvalidOperationException("The instance {0} of the type {1} wasn't in the cache");
				}
				return result;
			}
			return null;
		}
	}
}