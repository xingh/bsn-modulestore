﻿using System;
using System.Linq;

namespace bsn.ModuleStore.Sql.Script.Tokens {
	public abstract class ForXmlToken: SqlToken {
		public abstract ForXmlKind Kind {
			get;
		}
	}
}