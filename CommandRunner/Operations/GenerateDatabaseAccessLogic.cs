using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandRunner.CodeGeneration;
using CommandRunner.CodeGeneration.Subsystems;
using CommandRunner.CodeGeneration.Subsystems.StandardModification;
using CommandRunner.DatabaseAbstraction;
using CommandRunner.Tools;
using CommandRunner.XML_Schemas;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;
using Database = CommandRunner.DatabaseAbstraction.Database;

namespace CommandRunner.Operations {
	internal static class GenerateDatabaseAccessLogic {
		public static void Run( string projectFolder, SystemDevelopmentConfiguration configuration, Logger log ) {
			var outputFilePath = Path.Combine( projectFolder, "GeneratedCode", "TypedDataLayer.cs" );
			log.Info( "Writing generated code to " + outputFilePath );
			var outputDir = Path.GetDirectoryName( outputFilePath );
			log.Debug( "Creating directory: " + outputDir );
			Directory.CreateDirectory( outputDir );

			var baseNamespace = configuration.LibraryNamespaceAndAssemblyName + ".DataAccess";

			using( var writer = new StreamWriter( File.OpenWrite( outputFilePath ) ) ) {
				writeUsingStatements( writer );

				var databaseInfo = DatabaseOps.CreateDatabase( getDatabaseInfo( "", configuration.databaseConfiguration ), new List<string>() );

				ExecuteDatabaseUpdatesScript.Run( projectFolder, databaseInfo, log );
				generateDataAccessCodeForDatabase( log, databaseInfo, projectFolder, writer, baseNamespace, configuration );
			}
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
			foreach(
				var @namespace in
					Assembly.GetAssembly( typeof( SystemDevelopmentConfiguration ) ).GetTypes().Select( t => t.Namespace ).Distinct().Where( n => !string.IsNullOrEmpty( n ) )
				) {
				writer.WriteLine( "using " + @namespace + ";" );
			}
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