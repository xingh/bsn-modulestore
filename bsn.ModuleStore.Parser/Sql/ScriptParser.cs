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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using bsn.ModuleStore.Sql.Script;

namespace bsn.ModuleStore.Sql {
	public static class ScriptParser {
		// functions: http://technet.microsoft.com/en-us/library/ms174318(SQL.90).aspx
		private static readonly Dictionary<string, string> builtinFunctions = LoadFromFile("SqlFunctions.txt");
		// reserved word list: http://msdn.microsoft.com/en-us/library/aa238507(v=SQL.80).aspx
		private static readonly Dictionary<string, string> reservedWords = LoadFromFile("SqlKeywords.txt");
		private static readonly object sync = new object();
		private static CompiledGrammar compiledGrammar;
		private static SemanticTypeActions<SqlToken> semanticActions;

		internal static string GetBuiltinFunctionName(string name) {
			return builtinFunctions[name];
		}

		internal static CompiledGrammar GetGrammar() {
			lock (sync) {
				if (compiledGrammar == null) {
					compiledGrammar = CompiledGrammar.Load(typeof(ScriptParser), "ModuleStoreSQL.egt");
				}
				return compiledGrammar;
			}
		}

		public static SemanticTypeActions<SqlToken> GetSemanticActions() {
			lock (sync) {
				if (semanticActions == null) {
					semanticActions = new SemanticTypeActions<SqlToken>(GetGrammar());
					semanticActions.Initialize();
				}
				return semanticActions;
			}
		}

		public static bool IsBuiltinFunctionName(string functionName) {
			return builtinFunctions.ContainsKey(functionName);
		}

		public static bool IsReservedWord(string name) {
			return reservedWords.ContainsKey(name);
		}

		private static Dictionary<string, string> LoadFromFile(string filename) {
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			using (Stream stream = typeof(ScriptParser).Assembly.GetManifestResourceStream(typeof(ScriptParser), filename)) {
				Debug.Assert(stream != null);
				using (TextReader reader = new StreamReader(stream, true)) {
					for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
						line = line.Trim();
						if (line.Length > 0) {
							try {
								result.Add(line, line);
							} catch (ArgumentException) {
								Debug.WriteLine(string.Format("Duplicate entry {0} in file {1}", line, filename));
							}
						}
					}
				}
			}
			return result;
		}

		public static IEnumerable<Statement> Parse(string sql) {
			using (StringReader reader = new StringReader(sql)) {
				return Parse(reader);
			}
		}

		public static IEnumerable<Statement> Parse(TextReader sql) {
			ParseMessage parseMessage = ParseMessage.None;
			SqlSemanticProcessor processor = new SqlSemanticProcessor(sql, GetSemanticActions());
			try {
				parseMessage = processor.ParseAll();
			} catch (Exception ex) {
				throw new ParseException(ex.Message, parseMessage, ((IToken)processor.CurrentToken).Position, ex);
			}
			if (parseMessage != ParseMessage.Accept) {
				throw new ParseException("The supplied SQL could not be parsed", parseMessage, ((IToken)processor.CurrentToken).Position);
			}
			return (IEnumerable<Statement>)processor.CurrentToken;
		}

		public static Qualified<SchemaName, ObjectName> ParseObjectName(string objectName) {
			return Parse(NameOnlyStatement.Key+' '+objectName+";").OfType<NameOnlyStatement>().Single().ObjectName;
		}

		internal static bool TryGetBuiltinFunctionName(ref string name) {
			string result;
			if (builtinFunctions.TryGetValue(name, out result)) {
				name = result;
				return true;
			}
			return false;
		}

		internal static bool TryGetReservedWord(ref string word) {
			string result;
			if (reservedWords.TryGetValue(word, out result)) {
				word = result;
				return true;
			}
			return false;
		}
	}
}
