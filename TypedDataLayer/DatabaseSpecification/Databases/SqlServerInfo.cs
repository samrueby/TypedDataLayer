using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about a SQL Server database.
	/// </summary>
	public class SqlServerInfo: DatabaseInfo {
		/// <summary>
		/// Creates a new SQL Server information object. 
		/// </summary>
		public SqlServerInfo( string connectionString ) {
			if( connectionString.IsNullOrWhiteSpace() )
				throw new ApplicationException( "Connection string was not found in configuration." );
			ConnectionString = connectionString;
		}


		string DatabaseInfo.ParameterPrefix => "@";
		string DatabaseInfo.LastAutoIncrementValueExpression => "@@IDENTITY";
		string DatabaseInfo.QueryCacheHint => "";

		/// <summary>
		/// The connection string used to connect to the server.
		/// </summary>
		public string ConnectionString { get; }

		DbConnection DatabaseInfo.CreateConnection() => new SqlConnection( ConnectionString );

		DbCommand DatabaseInfo.CreateCommand( int? commandTimeout ) => new ProfiledDbCommand( new SqlCommand { CommandTimeout = commandTimeout ?? 30 }, null, MiniProfiler.Current );

		DbParameter DatabaseInfo.CreateParameter() => new SqlParameter();

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType ) => ( (SqlDbType)databaseSpecificType ).ToString();

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) => ( (SqlParameter)parameter ).SqlDbType = dbTypeString.ToEnum<SqlDbType>();
	}
}