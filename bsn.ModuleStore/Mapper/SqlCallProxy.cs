﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Xml;
using System.Xml.XPath;

namespace bsn.ModuleStore.Mapper {
	/// <summary>
	/// The DbCallProxy class allows the creation of transparent proxies to call stored procedures with typed and named parameters, and return results in different ways.
	/// <br/><br/>
	/// The interface can declare any number of methods, which are then used to call stored procedures on the database.
	/// <br/><br/>
	/// See also <see cref="SqlProcedureAttribute"/> and <see cref="SqlArgAttribute"/> for explicit bindings on methods.
	/// <br/><br/>
	/// The result is returned as follows:<br/>
	/// - When no return value is expected (void), the call is made as non-query call (<see cref="DbCommand.ExecuteNonQuery"/>).<br/>
	/// - When the return value is a <see cref="IDataReader"/>, a DataReader is opened and returned (<see cref="DbCommand.ExecuteReader(CommandBehavior)"/>). Note that this return type cannot be combined with OUTPUT arguments.<br/>
	/// - When the return value is an interface implementing <see cref="ITypedDataReader"/>, a typed data reader is initialized. Note that this return type cannot be combined with OUTPUT arguments.<br/>
	/// - When the return value is a <see cref="XPathNavigator"/>, a XPathNavigator will be returned to navigator over the rows.<br/>
	/// - When the return value is an <see cref="int"/> and the <see cref="SqlReturnValue"/> is <see cref="SqlReturnValue.Auto"/> or <see cref="SqlReturnValue.ReturnValue"/>, the SP result is returned.
	/// - When the return value is another primitive (or <see cref="int"/> with DbReturnValue.<see cref="SqlReturnValue.Scalar"/> set), a scalar call is made (<see cref="DbCommand.ExecuteScalar"/>).
	/// </summary>
	/// <seealso cref="SqlProcedureAttribute"/>
	/// <seealso cref="SqlReturnValue"/>
	/// <seealso cref="SqlArgAttribute"/>
	/// <seealso cref="CreateConnection"/>
	public class SqlCallProxy: RealProxy {
		/// <summary>
		/// Create a new proxy to be used for stored procedure calls, which can be called through the interface specified by <typeparamref name="I"/>.
		/// </summary>
		/// <typeparam name="I">The interface declaring the calls. This interface must implement <see cref="IDisposable"/></typeparam>
		/// <param name="createConnection">A delegate used to allocate new connections. This delegate is called for every call done on the interface.</param>
		/// <returns>An instance of the type requested by <typeparamref name="I"/>.</returns>
		public static I Create<I>(Func<SqlConnection> createConnection, string schemaName) where I: IDisposable {
			return (I)(new SqlCallProxy(createConnection, schemaName, typeof(I))).GetTransparentProxy();
		}

		private static object[] GetOutArgValues(KeyValuePair<SqlParameter, Type>[] dbParams) {
			if (dbParams == null) {
				return null;
			}
			object[] result = new object[dbParams.Length];
			for (int i = 0; i < result.Length; i++) {
				SqlParameter param = dbParams[i].Key;
				Type valueType = dbParams[i].Value;
				if (param != null) {
					if ((valueType != null) && ((param.Value == null) || (param.Value == DBNull.Value))) {
					    result[i] = Activator.CreateInstance(valueType);
					} else {
						result[i] = param.Value;
					}
				}
			}
			return result;
		}

		private readonly SqlCallInfo callInfo;
		private readonly Func<SqlConnection> createConnection;
		private readonly string schemaName;

		private SqlCallProxy(Func<SqlConnection> createConnection, string schemaName, Type interfaceToProxy)
			: base(interfaceToProxy) {
			if (createConnection == null) {
				throw new ArgumentNullException("createConnection");
			}
			this.createConnection = createConnection;
			this.schemaName = schemaName;
			callInfo = SqlCallInfo.Get(interfaceToProxy);
		}

		/// <summary>
		/// Handle a method invocation. This is called by the proxy and should not be called explicitly.
		/// </summary>
		public override IMessage Invoke(IMessage msg) {
			IMethodCallMessage mcm = (IMethodCallMessage)msg;
			try {
				if (mcm.MethodName == "Dispose") {
					// release connections etc.
					return new ReturnMessage(null, null, 0, mcm.LogicalCallContext, mcm);
				}
				SqlConnection connection = createConnection();
				SqlDataReader reader = null;
				try {
					if (connection.State != ConnectionState.Open) {
						connection.Open();
					}
					SqlParameter returnParameter;
					KeyValuePair<SqlParameter, Type>[] outParameters;
					SqlDeserializer.TypeInfo returnTypeInfo;
					SqlProcedureAttribute procInfo;
					IList<IDisposable> disposeList = new List<IDisposable>(0);
					XmlNameTable xmlNameTable;
					using (SqlCommand command = callInfo.CreateCommand(mcm, connection, schemaName, out returnParameter, out outParameters, out returnTypeInfo, out procInfo, out xmlNameTable, disposeList)) {
						try {
							Type returnType = ((MethodInfo)mcm.MethodBase).ReturnType;
							object returnValue;
							if (returnType == typeof(void)) {
								command.ExecuteNonQuery();
								returnValue = null;
							} else {
								if ((returnType == typeof(XPathNavigator)) || (returnType == typeof(XPathDocument))) {
									using (XmlReader xmlReader = command.ExecuteXmlReader()) {
										XPathDocument xmlDocument = new XPathDocument(xmlReader);
										if (returnType == typeof(XPathNavigator)) {
											returnValue = xmlDocument.CreateNavigator();
										} else {
											returnValue = xmlDocument;
										}
									}
								} else if (returnType == typeof(XmlReader)) {
									returnValue = new XmlReaderCloseConnection(command.ExecuteXmlReader(), connection);
									connection = null;
								} else {
									bool isTypedDataReader = typeof(ITypedDataReader).IsAssignableFrom(returnType);
									if (isTypedDataReader || typeof(IDataReader).IsAssignableFrom(returnType)) {
										Debug.Assert(returnParameter == null);
										if (outParameters.Length > 0) {
											throw new NotSupportedException(
													"Out arguments cannot be combined with a DataReader return value, because DB output values are only returned after the reader is closed. See remarks section of http://msdn.microsoft.com/en-us/library/system.data.common.dbparameter.direction.aspx");
										}
										reader = command.ExecuteReader(CommandBehavior.CloseConnection);
										connection = null;
										if (isTypedDataReader) {
											try {
												returnValue = new DataReaderProxy(reader, returnType).GetTransparentProxy();
											} catch {
												reader.Dispose();
												throw;
											}
										} else {
											returnValue = reader;
										}
									} else if (returnParameter == null) {
										reader = command.ExecuteReader(CommandBehavior.SingleResult); // no using() or try...finally required, since the "reader" is already handled below
										bool hasData = reader.Read();
										if (!hasData) {
											if (procInfo.DeserializeReturnNullOnEmptyReader) {
												returnValue = null;
											} else if (returnTypeInfo.Type.IsArray) {
												returnValue = Array.CreateInstance(returnTypeInfo.InstanceType, 0);
											} else if (returnTypeInfo.IsCollection) {
												returnValue = Activator.CreateInstance(returnTypeInfo.ListType, 0); // creates an List<InstanceType> with capacity 0
											} else {
												throw new InvalidOperationException("The stored procedure did not return any result, but a result was required for object deserialization");
											}
										} else {
											if (returnTypeInfo.SimpleConverter != null) {
												SqlDeserializer.DeserializerContext context = new SqlDeserializer.DeserializerContext(reader, xmlNameTable);
												if (returnTypeInfo.IsCollection) {
													IList list = returnTypeInfo.CreateList();
													for (int row = procInfo.DeserializeRowLimit; row > 0; row--) {
														list.Add(returnTypeInfo.SimpleConverter.Process(context, 0));
														if (!reader.Read()) {
															break;
														}
													}
													returnValue = returnTypeInfo.FinalizeList(list);
												} else {
													returnValue = returnTypeInfo.SimpleConverter.Process(context, 0);
												}
											} else {
												returnValue = new SqlDeserializer(reader, returnType).DeserializeInternal(procInfo.DeserializeRowLimit, true, procInfo.DeserializeCallConstructor, xmlNameTable);
											}
										}
										reader.Dispose();
										reader = null;
									} else {
										command.ExecuteNonQuery();
										returnValue = returnParameter.Value;
										if (returnType != typeof(int)) {
											returnValue = Convert.ChangeType(returnValue, returnType);
										}
									}
								}
							}
							return new ReturnMessage(returnValue, GetOutArgValues(outParameters), outParameters.Length, mcm.LogicalCallContext, mcm);
						} finally {
							foreach (IDisposable disposable in disposeList) {
								disposable.Dispose();
							}
						}
					}
				} catch {
					if (reader != null) {
						reader.Dispose();
					}
					throw;
				} finally {
					if (connection != null) {
						connection.Dispose();
					}
				}
			} catch (Exception ex) {
				return new ReturnMessage(ex, mcm);
			}
		}
	}
}