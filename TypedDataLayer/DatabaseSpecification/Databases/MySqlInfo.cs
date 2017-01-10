using System;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about a MySQL database.
	/// </summary>
	public class MySqlInfo: DatabaseInfo {
		private static DbProviderFactory factoryField;
		private static DbProviderFactory factory => factoryField ?? ( factoryField = DbProviderFactories.GetFactory( "MySql.Data.MySqlClient" ) );

		private readonly string secondaryDatabaseName;

		/// <summary>
		/// Creates a new MySQL information object. Specify the empty string for the secondary database name if this represents the primary database.
		/// </summary>
		public MySqlInfo( string secondaryDatabaseName, string database, bool supportsConnectionPooling ) {
			this.secondaryDatabaseName = secondaryDatabaseName;
			Database = database;
			SupportsConnectionPooling = supportsConnectionPooling;
		}

		string DatabaseInfo.SecondaryDatabaseName => secondaryDatabaseName;

		string DatabaseInfo.ParameterPrefix => "@";
		string DatabaseInfo.LastAutoIncrementValueExpression => "LAST_INSERT_ID()";
		string DatabaseInfo.QueryCacheHint => "SQL_CACHE";

		/// <summary>
		/// Gets the database.
		/// </summary>
		public string Database { get; }

		/// <summary>
		/// Gets whether the database supports connection pooling.
		/// </summary>
		public bool SupportsConnectionPooling { get; }

		DbConnection DatabaseInfo.CreateConnection( string connectionString ) {
			var connection = factory.CreateConnection();
			connection.ConnectionString = connectionString;
			return connection;
		}

		DbCommand DatabaseInfo.CreateCommand() => new ProfiledDbCommand( factory.CreateCommand(), null, MiniProfiler.Current );

		DbParameter DatabaseInfo.CreateParameter() => factory.CreateParameter();

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType )
			=> Enum.GetName( factory.GetType().Assembly.GetType( "MySql.Data.MySqlClient.MySqlDbType" ), databaseSpecificType );

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) {
			var mySqlDbTypeProperty = parameter.GetType().GetProperty( "MySqlDbType" );
			mySqlDbTypeProperty.SetValue( parameter, Enum.Parse( factory.GetType().Assembly.GetType( "MySql.Data.MySqlClient.MySqlDbType" ), dbTypeString ), null );
		}
	}
}