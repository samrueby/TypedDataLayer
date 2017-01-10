using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.Subsystems;
using TypedDataLayer.DataAccess.Subsystems.StandardModification;
using TypedDataLayer.DatabaseAbstraction;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.XML_Schemas;
using Database = TypedDataLayer.DatabaseAbstraction.Database;

namespace TypedDataLayer {
	class Program {
		static void Main( string[] args ) {
			try {
				SystemDevelopmentConfiguration configuration;
				using( var file = File.OpenRead( "" ) )
					configuration = (SystemDevelopmentConfiguration)new XmlSerializer( typeof( SystemDevelopmentConfiguration ) ).Deserialize( file );

				var baseNamespace = configuration.LibraryNamespaceAndAssemblyName + ".DataAccess";
				foreach( var database in new[] { configuration.databaseConfiguration } ) {
					try {
						using( var writer = new StreamWriter( File.OpenWrite( "Output.cs" ) ) ) {
							generateDataAccessCodeForDatabase(
								DatabaseOps.CreateDatabase( getDatabaseInfo( "", database ), new List<string>() ),
								"",
								// Library path
								writer,
								baseNamespace,
								configuration );
						}
					}
					catch( Exception e ) {
						throw;
						//throw ApplicationException(
						//	"An exception occurred while generating data access logic for the " +
						//	(database.SecondaryDatabaseName.Length == 0 ? "primary" : database.SecondaryDatabaseName + " secondary") + " database.",
						//	e);
					}
				}
			}
			catch( Exception e ) {
				Console.WriteLine();
				Console.WriteLine( "An error occurred." );
				Console.WriteLine( e.ToString() );
			}
		}

		private static DatabaseInfo getDatabaseInfo( string secondaryDatabaseName, DatabaseConfiguration database ) {
			if( database is SqlServerDatabase ) {
				var sqlServerDatabase = database as SqlServerDatabase;
				return new SqlServerInfo(
					secondaryDatabaseName,
					sqlServerDatabase.server,
					sqlServerDatabase.SqlServerAuthenticationLogin != null ? sqlServerDatabase.SqlServerAuthenticationLogin.LoginName : null,
					sqlServerDatabase.SqlServerAuthenticationLogin != null ? sqlServerDatabase.SqlServerAuthenticationLogin.Password : null,
					sqlServerDatabase.database,
					true,
					sqlServerDatabase.FullTextCatalog );
			}
			if( database is MySqlDatabase ) {
				var mySqlDatabase = database as MySqlDatabase;
				return new MySqlInfo( secondaryDatabaseName, mySqlDatabase.database, true );
			}
			if( database is OracleDatabase ) {
				var oracleDatabase = database as OracleDatabase;
				return new OracleInfo(
					secondaryDatabaseName,
					oracleDatabase.tnsName,
					oracleDatabase.userAndSchema,
					oracleDatabase.password,
					!oracleDatabase.SupportsConnectionPoolingSpecified || oracleDatabase.SupportsConnectionPooling,
					!oracleDatabase.SupportsLinguisticIndexesSpecified || oracleDatabase.SupportsLinguisticIndexes );
			}
			throw new ApplicationException( $"{nameof( database )} is a {database.GetType().Name} which is an unknown database type." );
		}

		private static void generateDataAccessCodeForDatabase(
			Database database, string libraryBasePath, TextWriter writer, string baseNamespace, SystemDevelopmentConfiguration configuration ) {
			var tableNames = DatabaseOps.GetDatabaseTables( database );

			ensureTablesExist( tableNames, configuration.database.SmallTables, "small" );
			ensureTablesExist( tableNames, configuration.database.TablesUsingRowVersionedDataCaching, "row-versioned data caching" );
			ensureTablesExist( tableNames, configuration.database.revisionHistoryTables, "revision history" );

			ensureTablesExist( tableNames, configuration.database.WhitelistedTables, "whitelisted" );
			tableNames =
				tableNames.Where(
					table => configuration.database.WhitelistedTables == null || configuration.database.WhitelistedTables.Any( i => i.EqualsIgnoreCase( table ) ) );

			database.ExecuteDbMethod(
				delegate( DBConnection cn ) {
					// database logic access - standard
					writer.WriteLine();
					TableConstantStatics.Generate( cn, writer, baseNamespace, database, tableNames );

					// database logic access - custom
					writer.WriteLine();
					RowConstantStatics.Generate( cn, writer, baseNamespace, database, configuration.database );

					// retrieval and modification commands - standard
					writer.WriteLine();
					CommandConditionStatics.Generate( cn, writer, baseNamespace, database, tableNames );

					writer.WriteLine();
					var tableRetrievalNamespaceDeclaration = TableRetrievalStatics.GetNamespaceDeclaration( baseNamespace, database );
					TableRetrievalStatics.Generate( cn, writer, tableRetrievalNamespaceDeclaration, database, tableNames, configuration.database );

					writer.WriteLine();
					var modNamespaceDeclaration = StandardModificationStatics.GetNamespaceDeclaration( baseNamespace, database );
					StandardModificationStatics.Generate( cn, writer, modNamespaceDeclaration, database, tableNames, configuration.database );

					foreach( var tableName in tableNames ) {
						TableRetrievalStatics.WritePartialClass( cn, libraryBasePath, tableRetrievalNamespaceDeclaration, database, tableName );
						StandardModificationStatics.WritePartialClass(
							cn,
							libraryBasePath,
							modNamespaceDeclaration,
							database,
							tableName,
							DataAccessStatics.IsRevisionHistoryTable( tableName, configuration.database ) );
					}

					// retrieval and modification commands - custom
					writer.WriteLine();
					QueryRetrievalStatics.Generate( cn, writer, baseNamespace, database, configuration.database );
					writer.WriteLine();
					CustomModificationStatics.Generate( cn, writer, baseNamespace, database, configuration.database );

					// other commands
					if( cn.DatabaseInfo is OracleInfo ) {
						writer.WriteLine();
						SequenceStatics.Generate( cn, writer, baseNamespace, database );
						writer.WriteLine();
						ProcedureStatics.Generate( cn, writer, baseNamespace, database );
					}
				} );
		}

		private static void ensureTablesExist( IEnumerable<string> databaseTables, IEnumerable<string> specifiedTables, string tableAdjective ) {
			if( specifiedTables == null )
				return;
			var nonexistentTables = specifiedTables.Where( specifiedTable => databaseTables.All( i => !i.EqualsIgnoreCase( specifiedTable ) ) ).ToArray();
			if( nonexistentTables.Any() )
				throw new ApplicationException(
					tableAdjective.CapitalizeString() + " " + ( nonexistentTables.Length > 1 ? "tables" : "table" ) + " " +
					StringTools.GetEnglishListPhrase( nonexistentTables.Select( i => "'" + i + "'" ), true ) + " " + ( nonexistentTables.Length > 1 ? "do" : "does" ) +
					" not exist." );
		}
	}
}