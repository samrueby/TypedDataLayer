using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using TypedDataLayer.Collections;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess {
	public class DataAccessState {
		/// <summary>
		/// NOTE SJR: Figure out what the point of this is and maybe allow it to not be null sometimes.
		/// </summary>
		private static Func<DataAccessState> mainStateGetter;

		private static readonly ThreadLocal<Stack<DataAccessState>> mainStateOverrideStack =
			new ThreadLocal<Stack<DataAccessState>>( () => new Stack<DataAccessState>() );

		public static void Init( Func<DataAccessState> mainDataAccessStateGetter ) {
			mainStateGetter = mainDataAccessStateGetter;
		}


		/// <summary>
		/// Gets the current data-access state. In EWF web applications, this will throw an exception when called from the worker threads used by parallel
		/// programming tools such as PLINQ and the Task Parallel Library unless it is executed with a supplemental data-access state object. While it would be
		/// possible for us to create a main data-access state getter implementation that returns a separate object for each thread, we've decided against this
		/// because we feel it's a leaky abstraction. Each thread would silently have its own database transactions and its own cache, and not being aware of this
		/// fact could be extremely frustrating. Therefore we require developers to use supplemental data-access state objects on worker threads.
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

		private readonly Action<DBConnection> connectionInitializer;

		private bool cacheEnabled;
		private Cache<string, object> cache;
		private readonly DatabaseConfiguration database;

		/// <summary>
		/// This should only be used for two purposes. First, to create objects that will be returned by the mainDataAccessStateGetter argument of
		/// GlobalInitializationOps.InitStatics. Second, to create supplemental data-access state objects, which you may need if you want to communicate with a
		/// database outside of the main transaction.
		/// </summary>
		/// <param name="databaseConnectionInitializer">A method that is called whenever a database connection is requested. Can be used to initialize the
		/// connection.</param>
		public DataAccessState( Action<DBConnection> databaseConnectionInitializer = null )
			// NOTE SJR: Have them set the config to copy-local true
			: this( Utility.FindConfigFiles( AppDomain.CurrentDomain.BaseDirectory ).First(), databaseConnectionInitializer ) { }

		/// <summary>
		/// This should only be used for two purposes. First, to create objects that will be returned by the mainDataAccessStateGetter argument of
		/// GlobalInitializationOps.InitStatics. Second, to create supplemental data-access state objects, which you may need if you want to communicate with a
		/// database outside of the main transaction.
		/// </summary>
		/// <param name="configFilePath">Absolute path to config file.</param>
		/// /// <param name="databaseConnectionInitializer">A method that is called whenever a database connection is requested. Can be used to initialize the
		/// connection.</param>
		public DataAccessState( string configFilePath, Action<DBConnection> databaseConnectionInitializer = null ) {
			if( !File.Exists( configFilePath ) ) {
				throw new ApplicationException( "Unable to find config file at " + configFilePath );
			}
			database = Utility.XmlDeserialize<SystemDevelopmentConfiguration>( configFilePath ).databaseConfiguration;
			connectionInitializer = databaseConnectionInitializer ?? ( connection => { } );
		}

		private DBConnection dbConnection;

		/// <summary>
		/// Gets the connection to the database.
		/// </summary>
		[ UsedImplicitly ]
		public DBConnection DatabaseConnection
			=>
				initConnection( dbConnection ?? ( dbConnection = new DBConnection( database != null ? DatabaseFactory.CreateDatabaseInfo( database ) : null ) ) );

		/// <summary>
		/// Returns true if the <see cref="DatabaseConnection"/> has ever been accessed.
		/// </summary>
		public bool ConnectionInitialized { get; private set; }

		private DBConnection initConnection( DBConnection connection ) {
			ConnectionInitialized = true;
			connectionInitializer( connection );
			return connection;
		}

		/// <summary>
		/// Gets the cache value associated with the specified key. If no value exists, adds one by executing the specified creator function.
		/// </summary>
		public T GetCacheValue<T>( string key, Func<T> valueCreator ) {
			if( !cacheEnabled )
				return valueCreator();
			return (T)cache.GetOrAdd( key, () => valueCreator() );
		}

		/// <summary>
		/// Executes the specified method with the cache enabled. Supports nested calls by leaving the cache alone if it is already enabled. Do not modify data in
		/// the method; this could cause a stale cache and lead to data integrity problems!
		/// </summary>
		public void ExecuteWithCache( Action method ) {
			if( cacheEnabled ) {
				method();
			}
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
		/// Executes the specified method with the cache enabled. Supports nested calls by leaving the cache alone if it is already enabled. Do not modify data in
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

		internal void ResetCache() {
			cacheEnabled = true;
			cache = new Cache<string, object>( false );
		}

		internal void DisableCache() {
			cacheEnabled = false;
		}

		/// <summary>
		/// Executes the specified method with this as the current data-access state. Only necessary when using supplemental data-access state objects.
		/// </summary>
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
		/// Executes the specified method with this as the current data-access state. Only necessary when using supplemental data-access state objects.
		/// </summary>
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