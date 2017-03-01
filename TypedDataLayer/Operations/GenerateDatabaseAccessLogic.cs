using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TypedDataLayer.CodeGeneration;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.Subsystems;
using TypedDataLayer.DataAccess.Subsystems.StandardModification;
using TypedDataLayer.DatabaseAbstraction;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;
using TypedDataLayer.XML_Schemas;
using Database = TypedDataLayer.DatabaseAbstraction.Database;

namespace TypedDataLayer.Operations {
	internal static class GenerateDatabaseAccessLogic {
		public const string ConfigurationFileName = "TypedDataLayerConfig.xml";


		public static bool Run( string solutionPath, Logger log ) {
			var filePath = Directory.EnumerateFiles( solutionPath, ConfigurationFileName, SearchOption.AllDirectories ).First();
			// NOTE SJR: We can find a config file in each project and run it for that project.
			if( !File.Exists( filePath ) ) {
				log.Info( "Unable to find configuration file." );
				log.Info( $"Searched {solutionPath} for {ConfigurationFileName} recursively." );
				return true;
			}
			var projectFolder = getFirstFolder( filePath, solutionPath );
			var outputFilePath = Path.Combine( projectFolder, "GeneratedCode", "TypedDataLayer.cs" );
			log.Info( "Writing generated code to " + outputFilePath );
			var outputDir = Path.GetDirectoryName( outputFilePath );
			log.Debug( "Creating directory: " + outputDir );
			Directory.CreateDirectory( outputDir );

			var configuration = Utility.XmlDeserialize<SystemDevelopmentConfiguration>( filePath );

			var baseNamespace = configuration.LibraryNamespaceAndAssemblyName + ".DataAccess";
			foreach( var database in new[] { configuration.databaseConfiguration } ) {
				try {
					using( var writer = new StreamWriter( File.OpenWrite( outputFilePath ) ) ) {
						writeUsingStatements( writer );

						generateDataAccessCodeForDatabase(
							log,
							DatabaseOps.CreateDatabase( getDatabaseInfo( "", database ), new List<string>() ),
							projectFolder,
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
			return false;
		}

		private static void writeUsingStatements( StreamWriter writer ) {
			writer.WriteLine( "using System;" );
			writer.WriteLine( "using System.Globalization;" );
			writer.WriteLine( "using System.Reflection;" );
			writer.WriteLine( "using System.Runtime.InteropServices;" );
			writer.WriteLine( "using System.Collections.Generic;" );
			writer.WriteLine( "using System.Data;" );
			writer.WriteLine( "using System.Data.Common;" );
			writer.WriteLine( "using System.Diagnostics;" );
			writer.WriteLine( "using System.Linq;" );
			writer.WriteLine( "using System.Reflection;" );
			writer.WriteLine( "using System.Runtime.InteropServices;" );

			// NOTE SJR: If I separate the program the generates the code and the dll that includes the classes for these types, this will have to change.
			foreach( var @namespace in Assembly.GetExecutingAssembly().GetTypes().Select( t => t.Namespace ).Distinct().Where( n => !String.IsNullOrEmpty( n ) ) ) {
				writer.WriteLine( "using " + @namespace + ";" );
			}
		}

		private static string getFirstFolder( string filePath, string solutionPath ) {
			var relative = filePath.Replace( solutionPath, "" );
			var startIndex = relative.StartsWith( "\\" ) ? 1 : 0;
			return relative.Substring( startIndex, relative.IndexOf( '\\', startIndex ) - startIndex );
		}


		private static void generateDataAccessCodeForDatabase(
			Logger log, Database database, string libraryBasePath, TextWriter writer, string baseNamespace, SystemDevelopmentConfiguration configuration ) {
			var tableNames = DatabaseOps.GetDatabaseTables( database );

			if( configuration.database == null ) {
				log.Info( $"Configuration is missing the <{nameof( configuration.database )}> element." );
				return;
			}

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

		private static DatabaseInfo getDatabaseInfo( string secondaryDatabaseName, DatabaseConfiguration database ) {
			if( database is SqlServerDatabase ) {
				var sqlServerDatabase = (SqlServerDatabase)database;
				return new SqlServerInfo(
					secondaryDatabaseName,
					sqlServerDatabase.server,
					sqlServerDatabase.SqlServerAuthenticationLogin?.LoginName,
					sqlServerDatabase.SqlServerAuthenticationLogin?.Password,
					sqlServerDatabase.database,
					true,
					sqlServerDatabase.FullTextCatalog );
			}
			if( database is MySqlDatabase ) {
				var mySqlDatabase = (MySqlDatabase)database;
				return new MySqlInfo( secondaryDatabaseName, mySqlDatabase.database, true );
			}
			if( database is OracleDatabase ) {
				var oracleDatabase = (OracleDatabase)database;
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
	}
}