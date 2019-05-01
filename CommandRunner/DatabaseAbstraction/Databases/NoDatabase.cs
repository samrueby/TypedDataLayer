using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;

namespace CommandRunner.DatabaseAbstraction.Databases {
	internal class NoDatabase: IDatabase {
		public void ExecuteSqlScriptInTransaction( string script ) {
			throw new NotSupportedException();
		}

		public int GetLineMarker() {
			throw new NotSupportedException();
		}

		public void UpdateLineMarker( int value ) {
			throw new NotSupportedException();
		}

		List<Table> IDatabase.GetTables() {
			throw new NotSupportedException();
		}

		List<string> IDatabase.GetProcedures() {
			throw new NotSupportedException();
		}

		List<ProcedureParameter> IDatabase.GetProcedureParameters( string procedure ) {
			throw new NotSupportedException();
		}

		void IDatabase.ExecuteDbMethod( Action<DBConnection> method ) {
			throw new NotSupportedException();
		}
	}
}