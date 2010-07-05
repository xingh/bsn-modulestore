using bsn.GoldParser.Semantic;

namespace bsn.ModuleStore.Sql.Script {
	public class DeallocateStatement: SqlCursorStatement {
		[Rule("<DeallocateStatement> ::= DEALLOCATE <GlobalOrLocalCursor>")]
		public DeallocateStatement(CursorName cursorName): base(cursorName) {}

		public override void WriteTo(System.IO.TextWriter writer) {
			writer.Write("DEALLOCATE ");
			base.WriteTo(writer);
		}
	}
}