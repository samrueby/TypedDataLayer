using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;

namespace TypedDataLayer.DatabaseAbstraction.Databases {
	internal class NoDatabase: Database {
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