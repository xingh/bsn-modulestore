﻿#if DEBUG

using System;
using System.Data;
using System.Data.SqlClient;

namespace bsn.ModuleStore.Mapper.InterfaceMetadata {
	internal class ConnectionProvider: IConnectionProvider {
		public ConnectionProvider(string connectionString) {
			ConnectionString = connectionString;
		}

		public string ConnectionString {
			get;
			set;
		}

		public SqlConnection GetConnection() {
			return new SqlConnection(ConnectionString);
		}

		public SqlTransaction GetTransaction() {
			return null;
		}

		public IsolationLevel DefaultIsolationLevel {
			get {
				return IsolationLevel.Unspecified;
			}
		}

		public string SchemaName {
			get {
				return null;
			}
		}
	}
}

#endif
