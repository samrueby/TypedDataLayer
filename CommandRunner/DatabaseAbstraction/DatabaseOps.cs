using System;
using System.Collections.Generic;
using System.Linq;
using CommandRunner.DatabaseAbstraction.Databases;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace CommandRunner.DatabaseAbstraction {
	public static class DatabaseOps {
		internal static IDatabase CreateDatabase( DatabaseInfo databaseInfo ) {
			if( databaseInfo == null )
				return new NoDatabase();
			if( databaseInfo is SqlServerInfo )
				return new SqlServer( (SqlServerInfo)databaseInfo );
			if( databaseInfo is MySqlInfo )
				return new Databases.MySql( (MySqlInfo)databaseInfo );
			if( databaseInfo is OracleInfo )
				return new Oracle( (OracleInfo)databaseInfo );

			throw new ApplicationException( $"{databaseInfo} is a {databaseInfo.GetType().Name} which is an unknown database information object type." );
		}

		/// <summary>
		/// Gets the tables in the specified database, ordered by name.
		/// </summary>
		public static IEnumerable<Table> GetDatabaseTables( IDatabase database ) => database.GetTables().OrderBy( i => i.Schema ).ThenBy( i => i.Name );
	}
}