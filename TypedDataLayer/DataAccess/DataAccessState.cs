using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using TypedDataLayer.Collections;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess {
	/// <summary>
	/// Controls how database connections are retrieved and initialized within a call stack.
	/// </summary>
	public class DataAccessState {
		private readonly Action<DBConnection> connectionInitializer;
		private readonly SupportedDatabaseType databaseType;
		private readonly string connectionString;


		/// <summary>
		/// This should only be used for two purposes. First, to create objects that will be returned by the
		/// mainDataAccessStateGetter argument of
		/// GlobalInitializationOps.InitStatics. Second, to create supplemental data-access state objects, which you may need if
		/// you want to communicate with a
		/// database outside of the main transaction.
		/// </summary>
		/// <param name="databaseConnectionInitializer">
		/// A method that is called every time a database connection is requested. Can be used to initialize the
		/// connection.
		/// </param>
		public DataAccessState( Action<DBConnection> databaseConnectionInitializer = null ): this(
			readSupportedDatabaseTypeFromConfiguration(),
			readConnectionStringFromConfiguration(),
			databaseConnectionInitializer ) { }

		private static string readConnectionStringFromConfiguration() {
			var str = ConfigurationManager.AppSettings[ ConfigurationConstants.ConnectionString ];
			if( str.IsNullOrWhiteSpace() )
				throw new ApplicationException( $"Attempted to read {ConfigurationConstants.ConnectionString} from <appSettings>. " + errorRecommendation );
			return str;
		}

		private static SupportedDatabaseType readSupportedDatabaseTypeFromConfiguration() {
			var type = ConfigurationManager.AppSettings[ ConfigurationConstants.SupportedDatabaseType ];
			string validValues() => StringTools.GetEnumValues<SupportedDatabaseType>().Select( e => e.ToString() ).GetCommaDelimitedList();

			if( type.IsNullOrWhiteSpace() ) {
				throw new ApplicationException(
					$"Attempted to read {ConfigurationConstants.SupportedDatabaseType} from <appSettings>. " + errorRecommendation + $"Valid values are {validValues()}." );
			}

			SupportedDatabaseType toEnum;
			try {
				toEnum = type.ToEnum<SupportedDatabaseType>();
			}
			catch( ArgumentException ae ) {
				throw new ApplicationException(
					$"Attempted to read {ConfigurationConstants.SupportedDatabaseType} from <appSettings>. " + $"{type} is an invalid value. Valid values are: {validValues()}." +
					errorRecommendation,
					ae );
			}

			return toEnum;
		}


		/// <summary>
		/// This should only be used for two purposes. First, to create objects that will be returned by the
		/// mainDataAccessStateGetter argument of
		/// GlobalInitializationOps.InitStatics. Second, to create supplemental data-access state objects, which you may need if
		/// you want to communicate with a
		/// database outside of the main transaction.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="databaseConnectionInitializer">
		/// A method that is called whenever a database connection is requested. Can be used to initialize the
		/// connection.
		/// </param>
		/// <param name="databaseType"></param>
		public DataAccessState( SupportedDatabaseType databaseType, string connectionString, Action<DBConnection> databaseConnectionInitializer = null ) {
			this.databaseType = databaseType;
			this.connectionString = connectionString;
			connectionInitializer = databaseConnectionInitializer ?? ( connection => { } );
		}


		private static Func<DataAccessState> mainStateGetter;

		private static readonly ThreadLocal<Stack<DataAccessState>> mainStateOverrideStack = new ThreadLocal<Stack<DataAccessState>>( () => new Stack<DataAccessState>() );

		/// <summary>
		/// Initializes the <paramref name="mainDataAccessStateGetter" />.
		/// </summary>
		[ UsedImplicitly ]
		public static void Init( Func<DataAccessState> mainDataAccessStateGetter ) {
			mainStateGetter = mainDataAccessStateGetter;
		}


		/// <summary>
		/// Gets the current data-access state.
		/// </summary>
		public static DataAccessState Current {
			get {
				if( mainStateOverrideStack.Value.Any() )
					return mainStateOverrideStack.Value.Peek();
				if( mainStateGetter == null )
					throw new ApplicationException( "No main data-access state getter was specified during application initialization." );
				var mainDataAccessState = mainStateGetter();
				if( mainDataAccessState == null )
					throw new ApplicationException( "No main data-access state exists at this time." );
				return mainDataAccessState;
			}
		}

		private bool cacheEnabled;
		private Cache<string, object> cache;
		private DBConnection dbConnection;

		private static readonly string errorRecommendation =
			$"You must either set this value, or user the {nameof(DataAccessState)} constructor that allows you to provide this value manually.";

		/// <summary>
		/// Gets the connection to the database.
		/// </summary>
		[ UsedImplicitly ]
		public DBConnection DatabaseConnection =>
			initConnection( dbConnection ?? ( dbConnection = new DBConnection( DatabaseFactory.CreateDatabaseInfo( databaseType, connectionString ) ) ) );

		/// <summary>
		/// Returns true if the <see cref="DatabaseConnection" /> has ever been accessed.
		/// </summary>
		[ UsedImplicitly ]
		public bool ConnectionInitialized { get; private set; }

		private DBConnection initConnection( DBConnection connection ) {
			ConnectionInitialized = true;
			connectionInitializer( connection );
			return connection;
		}

		/// <summary>
		/// Gets the cache value associated with the specified key. If no value exists, adds one by executing the specified creator
		/// function.
		/// </summary>
		public T GetCacheValue<T>( string key, Func<T> valueCreator ) {
			if( !cacheEnabled )
				return valueCreator();
			return (T)cache.GetOrAdd( key, () => valueCreator() );
		}

		/// <summary>
		/// Executes the specified method with the cache enabled. Supports nested calls by leaving the cache alone if it is already
		/// enabled. Do not modify data in
		/// the method; this could cause a stale cache and lead to data integrity problems!
		/// </summary>
		public T ExecuteWithCache<T>( Func<T> method ) {
			if( cacheEnabled )
				return method();
			ResetCache();
			try {
				return method();
			}
			finally {
				DisableCache();
			}
		}

		public void ResetCache() {
			cacheEnabled = true;
			cache = new Cache<string, object>( false );
		}

		internal void DisableCache() {
			cacheEnabled = false;
		}

		/// <summary>
		/// Executes the specified method with the cache enabled. Supports nested calls by leaving the cache alone if it is already
		/// enabled. Do not modify data in
		/// the method; this could cause a stale cache and lead to data integrity problems!
		/// </summary>
		[ UsedImplicitly ]
		public void ExecuteWithCache( Action method ) {
			if( cacheEnabled )
				method();
			else {
				ResetCache();
				try {
					method();
				}
				finally {
					DisableCache();
				}
			}
		}

		/// <summary>
		/// Executes the specified method with this as the current data-access state. Only necessary when using supplemental
		/// data-access state objects.
		/// </summary>
		[ UsedImplicitly ]
		public void ExecuteWithThis( Action method ) {
			mainStateOverrideStack.Value.Push( this );
			try {
				method();
			}
			finally {
				mainStateOverrideStack.Value.Pop();
			}
		}

		/// <summary>
		/// Executes the specified method with this as the current data-access state. Only necessary when using supplemental
		/// data-access state objects.
		/// </summary>
		[ UsedImplicitly ]
		public T ExecuteWithThis<T>( Func<T> method ) {
			mainStateOverrideStack.Value.Push( this );
			try {
				return method();
			}
			finally {
				mainStateOverrideStack.Value.Pop();
			}
		}
	}
}