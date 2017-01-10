using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypedDataLayer.DatabaseAbstraction.Databases;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseAbstraction {
	public static class DatabaseOps {
		internal static Database CreateDatabase( DatabaseInfo databaseInfo, List<string> oracleTableSpaces ) {
			if( databaseInfo == null )
				return new NoDatabase();
			if( databaseInfo is SqlServerInfo )
				return new SqlServer( (SqlServerInfo)databaseInfo );
			if( databaseInfo is MySqlInfo )
				return new MySql( (MySqlInfo)databaseInfo );
			if( databaseInfo is OracleInfo )
				return new Oracle( (OracleInfo)databaseInfo );

			throw new ApplicationException( $"{databaseInfo} is a {databaseInfo.GetType().Name} which is an unkonwn database information object type." );
		}

		/// <summary>
		/// Gets the tables in the specified database, ordered by name.
		/// </summary>
		public static IEnumerable<string> GetDatabaseTables( Database database ) => database.GetTables().OrderBy( i => i );

		/// <summary>
		/// Returns null if no database script exists on the hard drive.
		/// </summary>
		public static int? GetNumberOfLinesInDatabaseScript( string databaseUpdateFilePath ) {
			if( !File.Exists( databaseUpdateFilePath ) )
				return null;

			var lines = 0;
			using( var reader = new StreamReader( File.OpenRead( databaseUpdateFilePath ) ) ) {
				while( reader.ReadLine() != null )
					lines++;
			}
			return lines;
		}
	}
}