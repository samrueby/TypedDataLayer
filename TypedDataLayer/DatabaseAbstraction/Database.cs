using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;

namespace TypedDataLayer.DatabaseAbstraction {
	public interface Database {
		// Other
		List<string> GetTables();

		List<string> GetProcedures();
		List<ProcedureParameter> GetProcedureParameters( string procedure );

		/// <summary>
		/// Executes the given method inside a DBConnection for this Database.
		/// </summary>
		void ExecuteDbMethod( Action<DBConnection> method );
	}
}