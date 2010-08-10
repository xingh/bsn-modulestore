﻿using System;

using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script.Tokens {
	[Terminal("DELETE")]
	public sealed class DeleteToken: DmlOperationToken {
		public override DmlOperation Operation {
			get {
				return DmlOperation.Delete;
			}
		}
	}
}