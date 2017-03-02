using System;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseSpecification {
	public static class DatabaseFactory {
		public static DatabaseInfo CreateDatabaseInfo( DatabaseConfiguration database ) {
			if( database is SqlServerDatabase ) {
				var sqlServerDatabase = (SqlServerDatabase)database;
				return new SqlServerInfo( sqlServerDatabase.ConnectionString );
			}

			if( database is MySqlDatabase ) {
				var mySqlDatabase = database as MySqlDatabase;
				return new MySqlInfo( mySqlDatabase.ConnectionString );
			}

			if( database is OracleDatabase ) {
				var oracleDatabase = database as OracleDatabase;
				return new OracleInfo( oracleDatabase.ConnectionString, oracleDatabase.SupportsLinguisticIndexes );
			}

			throw new ApplicationException( $"{nameof( database )} is a {database.GetType().Name} which is an unknown database type." );
		}
	}
}