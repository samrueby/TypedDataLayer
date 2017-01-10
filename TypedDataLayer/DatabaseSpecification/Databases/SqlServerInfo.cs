using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about a SQL Server database.
	/// </summary>
	public class SqlServerInfo: DatabaseInfo {
		/// <summary>
		/// Creates a new SQL Server information object. Specify the empty string for the secondary database name if this represents the primary database. Pass null
		/// for the server to represent the local machine. Specify null for the login name and password if SQL Server Authentication is not being used. Pass null
		/// for fullTextCatalog to represent no full text catalog.
		/// </summary>
		public SqlServerInfo(
			string secondaryDatabaseName, string server, string loginName, string password, string database, bool supportsConnectionPooling, string fullTextCatalog ) {
			this.secondaryDatabaseName = secondaryDatabaseName;
			Server = server;
			LoginName = loginName;
			Password = password;
			Database = database;
			SupportsConnectionPooling = supportsConnectionPooling;
			FullTextCatalog = fullTextCatalog;
		}

		private readonly string secondaryDatabaseName;
		string DatabaseInfo.SecondaryDatabaseName => secondaryDatabaseName;

		string DatabaseInfo.ParameterPrefix => "@";
		string DatabaseInfo.LastAutoIncrementValueExpression => "@@IDENTITY";

		string DatabaseInfo.QueryCacheHint => "";

		/// <summary>
		/// Gets the server. Returns null to represent the local machine.
		/// </summary>
		public string Server { get; }

		/// <summary>
		/// Gets the SQL Server Authentication login name. Returns null if SQL Server Authentication is not being used.
		/// </summary>
		public string LoginName { get; }

		/// <summary>
		/// Gets the SQL Server Authentication password. Returns null if SQL Server Authentication is not being used.
		/// </summary>
		public string Password { get; }

		/// <summary>
		/// Gets the database.
		/// </summary>
		public string Database { get; }

		/// <summary>
		/// Gets whether the database supports connection pooling.
		/// </summary>
		public bool SupportsConnectionPooling { get; }

		/// <summary>
		/// Gets the full text catalog name, if it exists.  Otherwise, returns null.
		/// </summary>
		public string FullTextCatalog { get; }

		DbConnection DatabaseInfo.CreateConnection( string connectionString ) => new SqlConnection( connectionString );

		DbCommand DatabaseInfo.CreateCommand() => new ProfiledDbCommand( new SqlCommand { CommandTimeout = 15 }, null, MiniProfiler.Current );

		DbParameter DatabaseInfo.CreateParameter() => new SqlParameter();

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType ) => ( (SqlDbType)databaseSpecificType ).ToString();

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) => ( (SqlParameter)parameter ).SqlDbType = dbTypeString.ToEnum<SqlDbType>();
	}
}