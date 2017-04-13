using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandRunner.CodeGeneration;
using CommandRunner.CodeGeneration.Subsystems;
using CommandRunner.CodeGeneration.Subsystems.StandardModification;
using CommandRunner.DatabaseAbstraction;
using CommandRunner.Exceptions;
using CommandRunner.Tools;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.Operations {
	internal static class GenerateDatabaseAccessLogic {
		public static void Run( string projectFolder, SystemDevelopmentConfiguration configuration, Logger log ) {
			var outputFilePath = Path.Combine( projectFolder, "GeneratedCode", "TypedDataLayer.cs" );
			log.Info( "Writing generated code to " + outputFilePath );
			var outputDir = Path.GetDirectoryName( outputFilePath );
			log.Debug( "Creating directory: " + outputDir );
			Directory.CreateDirectory( outputDir );

			var baseNamespace = configuration.LibraryNamespaceAndAssemblyName + ".DataAccess";

			using( var writer = new StreamWriter( outputFilePath ) ) {
				writeUsingStatements( writer );

				var databaseInfo =
					DatabaseOps.CreateDatabase(
						DatabaseFactory.CreateDatabaseInfo( configuration.databaseConfiguration.DatabaseType, configuration.databaseConfiguration.ConnectionString ) );

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

			// Include every namespace in our TypedDataLayer assembly.
			foreach( var @namespace in
				Assembly.GetAssembly( typeof( DBConnection ) ).GetTypes().Select( t => t.Namespace ).Distinct().Where( n => !string.IsNullOrEmpty( n ) ) ) {
				writer.WriteLine( "using " + @namespace + ";" );
			}
		}


		private static void generateDataAccessCodeForDatabase(
			Logger log, IDatabase database, string libraryBasePath, TextWriter writer, string baseNamespace, SystemDevelopmentConfiguration configuration ) {
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
				cn => {
					writer.WriteLine();
					ConfigurationRetrievalStatics.Generate( writer, baseNamespace, configuration.database );

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
						SequenceStatics.Generate( cn, writer, baseNamespace, database, configuration.database.CommandTimeoutSecondsTyped );
						writer.WriteLine();
						ProcedureStatics.Generate( cn, writer, baseNamespace, database );
					}
				} );
		}

		private static void ensureTablesExist( IEnumerable<string> databaseTables, IEnumerable<string> specifiedTables, string tableAdjective ) {
			if( specifiedTables == null )
				return;
			var nonexistentTables = specifiedTables.Where( specifiedTable => databaseTables.All( i => !i.EqualsIgnoreCase( specifiedTable ) ) ).ToList();
			if( nonexistentTables.Any() )
				throw new UserCorrectableException(
					$"{tableAdjective.CapitalizeString()} {( nonexistentTables.Count > 1 ? "tables" : "table" )} {StringTools.GetEnglishListPhrase( nonexistentTables.Select( i => "'" + i + "'" ), true )} {( nonexistentTables.Count > 1 ? "do" : "does" )} not exist." );
		}
	}
}