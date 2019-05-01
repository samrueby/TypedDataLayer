using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;

namespace CommandRunner.DatabaseAbstraction {
	public interface IDatabase {

		/// <summary>
		/// The specified script is expected to either be the empty string or end with the line terminator string.
		/// </summary>
		void ExecuteSqlScriptInTransaction( string script );

		// Line marker retrieval and modification
		int GetLineMarker();
		void UpdateLineMarker( int value );

		// Other
		List<Table> GetTables();

		List<string> GetProcedures();
		List<ProcedureParameter> GetProcedureParameters( string procedure );

		/// <summary>
		/// Executes the given method inside a DBConnection for this Database.
		/// </summary>
		void ExecuteDbMethod( Action<DBConnection> method );
	}
}