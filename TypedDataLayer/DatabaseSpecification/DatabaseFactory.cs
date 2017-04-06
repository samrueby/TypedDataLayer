using System;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseSpecification {
	/// <summary>
	/// Factory methods for <see cref="DatabaseInfo"/>s.
	/// </summary>
	public static class DatabaseFactory {
		/// <summary>
		/// Creates a <see cref="DatabaseInfo"/>
		/// </summary>
		public static DatabaseInfo CreateDatabaseInfo( SupportedDatabaseType type, string connectionString ) {
			switch( type ) {
				case SupportedDatabaseType.SqlServer:
					return new SqlServerInfo( connectionString );
				case SupportedDatabaseType.MySql:
					return new MySqlInfo( connectionString );
				case SupportedDatabaseType.Oracle:
					return new OracleInfo( connectionString, false );
			}

			throw new ApplicationException( $"{type} is an unknown database type." );
		}
	}
}