using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;

namespace CommandRunner.DatabaseAbstraction.Databases {
	internal class NoDatabase: Database {
		public void ExecuteSqlScriptInTransaction( string script ) {
			throw new NotSupportedException();
		}

		public int GetLineMarker() {
			throw new NotSupportedException();
		}

		public void UpdateLineMarker( int value ) {
			throw new NotSupportedException();
		}

		List<string> Database.GetTables() {
			throw new NotSupportedException();
		}

		List<string> Database.GetProcedures() {
			throw new NotSupportedException();
		}

		List<ProcedureParameter> Database.GetProcedureParameters( string procedure ) {
			throw new NotSupportedException();
		}

		void Database.ExecuteDbMethod( Action<DBConnection> method ) {
			throw new NotSupportedException();
		}
	}
}