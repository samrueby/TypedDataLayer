using System;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about an Oracle database.
	/// </summary>
	public class OracleInfo: DatabaseInfo {
		private static DbProviderFactory factoryField;
		private static DbProviderFactory factory => factoryField ?? ( factoryField = DbProviderFactories.GetFactory( "Oracle.DataAccess.Client" ) );


		/// <summary>
		/// Creates a new Oracle database information object.
		/// </summary>
		public OracleInfo( string connectionString, bool supportsLinguisticIndexes ) {
			if( connectionString.IsNullOrWhiteSpace() )
				throw new ApplicationException( "Connection string was not found in configuration." );

			ConnectionString = connectionString;
			SupportsLinguisticIndexes = supportsLinguisticIndexes;
		}

		string DatabaseInfo.ParameterPrefix => ":";

		string DatabaseInfo.LastAutoIncrementValueExpression => "";

		string DatabaseInfo.QueryCacheHint => "/*+ RESULT_CACHE */";

		public string ConnectionString { get; }


		/// <summary>
		/// Gets whether the database supports linguistic indexes, which impacts whether or not it can enable case-insensitive comparisons.
		/// </summary>
		public bool SupportsLinguisticIndexes { get; }

		DbConnection DatabaseInfo.CreateConnection() {
			var connection = factory.CreateConnection();
			connection.ConnectionString = ConnectionString;
			return connection;
		}

		DbCommand DatabaseInfo.CreateCommand( int? commandTimeout ) {
			// NOTE SJR: Stop ignoring commandTimeout

			var c = factory.CreateCommand();

			// This property would be important if we screwed up the order of parameter adding later on.
			var bindByNameProperty = c.GetType().GetProperty( "BindByName" );
			bindByNameProperty.SetValue( c, true );

			// Tell the data reader to retrieve LOB data along with the rest of the row rather than making a separate request when GetValue is called.
			// Unfortunately, as of 17 July 2014 there is an Oracle bug that prevents us from setting the property to -1. See
			// http://stackoverflow.com/q/9006773/35349, https://community.oracle.com/thread/3548124, and Oracle bugs 14279177 and 17869834.
			var initialLobFetchSizeProperty = c.GetType().GetProperty( "InitialLOBFetchSize" );
			//initialLobFetchSizeProperty.SetValue( c, -1 );
			initialLobFetchSizeProperty.SetValue( c, 1024 );

			return new ProfiledDbCommand( c, null, MiniProfiler.Current );
		}

		DbParameter DatabaseInfo.CreateParameter() => factory.CreateParameter();

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType )
			=> Enum.GetName( factory.GetType().Assembly.GetType( "Oracle.DataAccess.Client.OracleDbType" ), databaseSpecificType );

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) {
			var oracleDbTypeProperty = parameter.GetType().GetProperty( "OracleDbType" );
			oracleDbTypeProperty.SetValue( parameter, Enum.Parse( factory.GetType().Assembly.GetType( "Oracle.DataAccess.Client.OracleDbType" ), dbTypeString ), null );
		}
	}
}