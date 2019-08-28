using System;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about a MySQL database.
	/// </summary>
	public class MySqlInfo: DatabaseInfo {
		private static DbProviderFactory factoryField;
		private static DbProviderFactory factory => factoryField ?? ( factoryField = DbProviderFactories.GetFactory( "MySql.Data.MySqlClient" ) );


		/// <summary>
		/// Creates a new MySQL information object.
		/// </summary>
		public MySqlInfo( string connectionString ) {
			if( connectionString.IsNullOrWhiteSpace() )
				throw new ApplicationException( "Connection string was not found in configuration." );
			ConnectionString = connectionString;
		}


		string DatabaseInfo.ParameterPrefix => "@";
		string DatabaseInfo.LastAutoIncrementValueExpression => "LAST_INSERT_ID()";
		string DatabaseInfo.QueryCacheHint => "SQL_CACHE";


		public string ConnectionString { get; }

		DbConnection DatabaseInfo.CreateConnection() {
			var connection = factory.CreateConnection();
			connection.ConnectionString = ConnectionString;
			return connection;
		}

		// NOTE SJR: Stop ignoring commandTimeout
		DbCommand DatabaseInfo.CreateCommand( int? commandTimeout ) => new ProfiledDbCommand( factory.CreateCommand(), null, MiniProfiler.Current );

		DbParameter DatabaseInfo.CreateParameter() => factory.CreateParameter();

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType ) =>
			Enum.GetName( factory.GetType().Assembly.GetType( "MySql.Data.MySqlClient.MySqlDbType" ), databaseSpecificType );

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) {
			var mySqlDbTypeProperty = parameter.GetType().GetProperty( "MySqlDbType" );
			mySqlDbTypeProperty.SetValue( parameter, Enum.Parse( factory.GetType().Assembly.GetType( "MySql.Data.MySqlClient.MySqlDbType" ), dbTypeString ), null );
		}
	}
}