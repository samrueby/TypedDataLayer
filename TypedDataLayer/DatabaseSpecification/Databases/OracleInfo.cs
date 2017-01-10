using System;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace TypedDataLayer.DatabaseSpecification.Databases {
	/// <summary>
	/// Contains information about an Oracle database.
	/// </summary>
	public class OracleInfo: DatabaseInfo {
		private static DbProviderFactory factoryField;
		private static DbProviderFactory factory => factoryField ?? ( factoryField = DbProviderFactories.GetFactory( "Oracle.DataAccess.Client" ) );

		private readonly string secondaryDatabaseName;

		/// <summary>
		/// Creates a new Oracle database information object. Specify the empty string for the secondary database name if this represents the primary database.
		/// </summary>
		public OracleInfo(
			string secondaryDatabaseName, string dataSource, string userAndSchema, string password, bool supportsConnectionPooling, bool supportsLinguisticIndexes ) {
			this.secondaryDatabaseName = secondaryDatabaseName;
			DataSource = dataSource;
			UserAndSchema = userAndSchema;
			Password = password;
			SupportsConnectionPooling = supportsConnectionPooling;
			SupportsLinguisticIndexes = supportsLinguisticIndexes;
		}

		string DatabaseInfo.SecondaryDatabaseName => secondaryDatabaseName;

		string DatabaseInfo.ParameterPrefix => ":";

		string DatabaseInfo.LastAutoIncrementValueExpression => "";

		string DatabaseInfo.QueryCacheHint => "/*+ RESULT_CACHE */";

		/// <summary>
		/// Gets the data source.
		/// </summary>
		public string DataSource { get; }

		/// <summary>
		/// Gets the user/schema.
		/// </summary>
		public string UserAndSchema { get; }

		/// <summary>
		/// Gets the password.
		/// </summary>
		public string Password { get; }

		/// <summary>
		/// Gets whether the database supports connection pooling.
		/// </summary>
		public bool SupportsConnectionPooling { get; }

		/// <summary>
		/// Gets whether the database supports linguistic indexes, which impacts whether or not it can enable case-insensitive comparisons.
		/// </summary>
		public bool SupportsLinguisticIndexes { get; }

		DbConnection DatabaseInfo.CreateConnection( string connectionString ) {
			var connection = factory.CreateConnection();
			connection.ConnectionString = connectionString;
			return connection;
		}

		DbCommand DatabaseInfo.CreateCommand() {
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

		string DatabaseInfo.GetDbTypeString( object databaseSpecificType ) => Enum.GetName( factory.GetType().Assembly.GetType( "Oracle.DataAccess.Client.OracleDbType" ), databaseSpecificType );

		void DatabaseInfo.SetParameterType( DbParameter parameter, string dbTypeString ) {
			var oracleDbTypeProperty = parameter.GetType().GetProperty( "OracleDbType" );
			oracleDbTypeProperty.SetValue( parameter, Enum.Parse( factory.GetType().Assembly.GetType( "Oracle.DataAccess.Client.OracleDbType" ), dbTypeString ), null );
		}
	}
}