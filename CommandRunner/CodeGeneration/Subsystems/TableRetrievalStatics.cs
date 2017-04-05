using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandRunner.CodeGeneration.Subsystems.StandardModification;
using CommandRunner.DatabaseAbstraction;
using CommandRunner.Tools;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration.Subsystems {
	internal static class TableRetrievalStatics {
		private const string oracleRowVersionDataType = "decimal";

		public static string GetNamespaceDeclaration( string baseNamespace, IDatabase database ) => "namespace " + baseNamespace + ".TableRetrieval {";

		internal static void Generate(
			DBConnection cn, TextWriter writer, string namespaceDeclaration, IDatabase database, IEnumerable<string> tableNames, TypedDataLayer.Database configuration ) {
			writer.WriteLine( namespaceDeclaration );
			foreach( var table in tableNames ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "Contains logic that retrieves rows from the " + table + " table." );
				writer.WriteLine( "public static partial class " + GetClassName( cn, table ) + " {" );

				var isRevisionHistoryTable = DataAccessStatics.IsRevisionHistoryTable( table, configuration );
				var columns = new TableColumns( cn, table, isRevisionHistoryTable );

				// Write nested classes.
				DataAccessStatics.WriteRowClasses(
					writer,
					columns.AllColumns,
					localWriter => {
						if( !isRevisionHistoryTable )
							return;
						writer.WriteLine(
							"public UserTransaction Transaction { get { return RevisionHistoryStatics.UserTransactionsById[ RevisionHistoryStatics.RevisionsById[ System.Convert.ToInt32( " +
							Utility.GetCSharpIdentifier( columns.PrimaryKeyAndRevisionIdColumn.PascalCasedNameExceptForOracle ) + " ) ].UserTransactionId ]; } }" );
					},
					localWriter => {
						if( !columns.DataColumns.Any() )
							return;

						var modClass = "Modification." + StandardModificationStatics.GetClassName( cn, table, isRevisionHistoryTable, isRevisionHistoryTable );
						var revisionHistorySuffix = StandardModificationStatics.GetRevisionHistorySuffix( isRevisionHistoryTable );
						writer.WriteLine( "public " + modClass + " ToModification" + revisionHistorySuffix + "() {" );
						writer.WriteLine(
							"return " + modClass + ".CreateForSingleRowUpdate" + revisionHistorySuffix + "( " +
							StringTools.ConcatenateWithDelimiter(
								", ",
								columns.AllColumnsExceptRowVersion.Select( i => Utility.GetCSharpIdentifier( i.PascalCasedNameExceptForOracle ) ).ToArray() ) + " );" );
						writer.WriteLine( "}" );
					} );
				writeCacheClass( cn, writer, database, table, columns, isRevisionHistoryTable );

				var isSmallTable = configuration.SmallTables != null && configuration.SmallTables.Any( i => i.EqualsIgnoreCase( table ) );

				var tableUsesRowVersionedCaching = configuration.TablesUsingRowVersionedDataCaching != null &&
				                                   configuration.TablesUsingRowVersionedDataCaching.Any( i => i.EqualsIgnoreCase( table ) );
				if( tableUsesRowVersionedCaching && columns.RowVersionColumn == null && !( cn.DatabaseInfo is OracleInfo ) )
					throw new ApplicationException(
						cn.DatabaseInfo is MySqlInfo
							? "Row-versioned data caching cannot currently be used with MySQL databases."
							: "Row-versioned data caching can only be used with the {0} table if you add a rowversion column.".FormatWith( table ) );

				if( isSmallTable )
					writeGetAllRowsMethod( writer, isRevisionHistoryTable, false );
				writeGetRowsMethod(
					cn,
					writer,
					database,
					table,
					columns,
					isSmallTable,
					tableUsesRowVersionedCaching,
					isRevisionHistoryTable,
					false,
					configuration.CommandTimeoutSecondsTyped );
				if( isRevisionHistoryTable ) {
					if( isSmallTable )
						writeGetAllRowsMethod( writer, true, true );
					writeGetRowsMethod(
						cn,
						writer,
						database,
						table,
						columns,
						isSmallTable,
						tableUsesRowVersionedCaching,
						true,
						true,
						configuration.CommandTimeoutSecondsTyped );
				}

				writeGetRowMatchingPkMethod(
					cn,
					writer,
					database,
					table,
					columns,
					isSmallTable,
					tableUsesRowVersionedCaching,
					isRevisionHistoryTable,
					configuration.CommandTimeoutSecondsTyped );

				if( isRevisionHistoryTable )
					DataAccessStatics.WriteGetLatestRevisionsConditionMethod( writer, columns.PrimaryKeyAndRevisionIdColumn.Name );

				if( tableUsesRowVersionedCaching ) {
					var keyTupleTypeArguments = getPkAndVersionTupleTypeArguments( cn, columns );

					writer.WriteLine( $"private static Cache<{TypeNames.Tuple}<{keyTupleTypeArguments}>, BasicRow> getRowsByPkAndVersion() {{" );
					var first = $"VersionedRowDataCache<{TypeNames.Tuple}<{getPkTupleTypeArguments( columns )}>, {TypeNames.Tuple}<{keyTupleTypeArguments}>, BasicRow>";
					var second = table.TableNameToPascal( cn ) + "TableRetrievalRowsByPkAndVersion";
					var third = StringTools.ConcatenateWithDelimiter( ", ", Enumerable.Range( 1, columns.KeyColumns.Count() ).Select( i => "i.Item{0}".FormatWith( i ) ) );
					writer.WriteLine(
						$@"return AppMemoryCache.GetCacheValue<{first}>( ""{second}"", () => new {first}( i => {TypeNames.Tuple}.Create( {third} ) ) ).RowsByPkAndVersion;" );
					writer.WriteLine( "}" );
				}

				// Initially we did not generate this method for small tables, but we found a need for it when the cache is disabled since that will cause
				// GetRowMatchingId to repeatedly query.
				if( columns.KeyColumns.Count() == 1 && columns.KeyColumns.Single().Name.ToLower().EndsWith( "id" ) )
					writeToIdDictionaryMethod( writer, columns );

				writer.WriteLine( "}" ); // class
			}
			writer.WriteLine( "}" ); // namespace
		}

		internal static void WritePartialClass( DBConnection cn, string libraryBasePath, string namespaceDeclaration, IDatabase database, string tableName ) {
			var folderPath = Utility.CombinePaths( libraryBasePath, "DataAccess", "TableRetrieval" );
			var templateFilePath = Utility.CombinePaths( folderPath, GetClassName( cn, tableName ) + DataAccessStatics.CSharpTemplateFileExtension );
			IoMethods.DeleteFile( templateFilePath );

			// If a real file exists, don't create a template.
			if( File.Exists( Utility.CombinePaths( folderPath, GetClassName( cn, tableName ) + ".cs" ) ) )
				return;

			using( var writer = IoMethods.GetTextWriterForWrite( templateFilePath ) ) {
				writer.WriteLine( namespaceDeclaration );
				writer.WriteLine( "	partial class " + GetClassName( cn, tableName ) + " {" );
				writer.WriteLine(
					"		// IMPORTANT: Change extension from \"{0}\" to \".cs\" before including in project and editing.".FormatWith(
						DataAccessStatics.CSharpTemplateFileExtension ) );
				writer.WriteLine( "	}" ); // class
				writer.WriteLine( "}" ); // namespace
			}
		}

		internal static string GetClassName( DBConnection cn, string table ) => Utility.GetCSharpIdentifier( table.TableNameToPascal( cn ) + "TableRetrieval" );

		private static void writeCacheClass(
			DBConnection cn, TextWriter writer, IDatabase database, string table, TableColumns tableColumns, bool isRevisionHistoryTable ) {
			var cacheKey = table.TableNameToPascal( cn ) + "TableRetrieval";
			var pkTupleTypeArguments = getPkTupleTypeArguments( tableColumns );

			writer.WriteLine( "private partial class Cache {" );
			writer.WriteLine( "internal static Cache Current { get { return DataAccessState.Current.GetCacheValue( \"" + cacheKey + "\", () => new Cache() ); } }" );
			writer.WriteLine( "private readonly TableRetrievalQueryCache<Row> queries = new TableRetrievalQueryCache<Row>();" );
			writer.WriteLine(
				$"private readonly Dictionary<{TypeNames.Tuple}<{pkTupleTypeArguments}>, Row> rowsByPk = new Dictionary<{TypeNames.Tuple}<{pkTupleTypeArguments}>, Row>();" );
			if( isRevisionHistoryTable )
				writer.WriteLine(
					$"private readonly Dictionary<{TypeNames.Tuple}<{pkTupleTypeArguments}>, Row> latestRevisionRowsByPk = new Dictionary<{TypeNames.Tuple}<{pkTupleTypeArguments}>, Row>();" );
			writer.WriteLine( "private Cache() {}" );
			writer.WriteLine( "internal TableRetrievalQueryCache<Row> Queries => queries; " );
			writer.WriteLine( $"internal Dictionary<{TypeNames.Tuple}<" + pkTupleTypeArguments + ">, Row> RowsByPk => rowsByPk;" );
			if( isRevisionHistoryTable )
				writer.WriteLine(
					$"internal Dictionary<{TypeNames.Tuple}<" + pkTupleTypeArguments + ">, Row> LatestRevisionRowsByPk { get { return latestRevisionRowsByPk; } }" );
			writer.WriteLine( "}" );
		}

		private static void writeGetAllRowsMethod( TextWriter writer, bool isRevisionHistoryTable, bool excludePreviousRevisions ) {
			var revisionHistorySuffix = isRevisionHistoryTable && !excludePreviousRevisions ? "IncludingPreviousRevisions" : "";
			CodeGenerationStatics.AddSummaryDocComment( writer, "Retrieves the rows from the table, ordered in a stable way." );
			writer.WriteLine( "public static IEnumerable<Row> GetAllRows" + revisionHistorySuffix + "() {" );
			writer.WriteLine( "return GetRowsMatchingConditions" + revisionHistorySuffix + "();" );
			writer.WriteLine( "}" );
		}

		private static void writeGetRowsMethod(
			DBConnection cn, TextWriter writer, IDatabase database, string table, TableColumns tableColumns, bool isSmallTable, bool tableUsesRowVersionedCaching,
			bool isRevisionHistoryTable, bool excludePreviousRevisions, int? commandTimeoutSeconds ) {
			// header
			var methodName = "GetRows" + ( isSmallTable ? "MatchingConditions" : "" ) +
			                 ( isRevisionHistoryTable && !excludePreviousRevisions ? "IncludingPreviousRevisions" : "" );
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Retrieves the rows from the table that match the specified conditions, ordered in a stable way." +
				( isSmallTable ? " Since the table is specified as small, you should only use this method if you cannot filter the rows in code." : "" ) );
			writer.WriteLine(
				"public static IEnumerable<Row> " + methodName + "( params " + DataAccessStatics.GetTableConditionInterfaceName( cn, database, table ) + "[] conditions ) {" );


			// body

			// If it's a primary key query, use RowsByPk if possible.
			foreach( var i in tableColumns.KeyColumns ) {
				var equalityConditionClassName = DataAccessStatics.GetEqualityConditionClassName( cn, database, table, i );
				writer.WriteLine( "var {0}Condition = conditions.OfType<{1}>().FirstOrDefault();".FormatWith( i.CamelCasedName, equalityConditionClassName ) );
			}
			writer.WriteLine( "var cache = Cache.Current;" );
			var pkConditionVariableNames = tableColumns.KeyColumns.Select( i => i.CamelCasedName + "Condition" );
			writer.WriteLine(
				"var isPkQuery = " + StringTools.ConcatenateWithDelimiter( " && ", pkConditionVariableNames.Select( i => i + " != null" ).ToArray() ) +
				" && conditions.Count() == " + tableColumns.KeyColumns.Count() + ";" );
			writer.WriteLine( "if( isPkQuery ) {" );
			writer.WriteLine( "Row row;" );
			writer.WriteLine(
				"if( cache." + ( excludePreviousRevisions ? "LatestRevision" : "" ) + $"RowsByPk.TryGetValue( {TypeNames.Tuple}.Create( " +
				StringTools.ConcatenateWithDelimiter( ", ", pkConditionVariableNames.Select( i => i + ".Value" ).ToArray() ) + " ), out row ) )" );
			writer.WriteLine( "return new [] {row};" );
			writer.WriteLine( "}" );

			var commandConditionsExpression = "conditions.Select( i => i.CommandCondition )";

			if( excludePreviousRevisions )
				commandConditionsExpression += ".Concat( new [] {getLatestRevisionsCondition()} )";
			writer.WriteLine( "return cache.Queries.GetResultSet( " + commandConditionsExpression + ", commandConditions => {" );
			writeResultSetCreatorBody(
				cn,
				writer,
				database,
				table,
				tableColumns,
				tableUsesRowVersionedCaching,
				excludePreviousRevisions,
				"!isPkQuery",
				commandTimeoutSeconds );
			writer.WriteLine( "} );" );

			writer.WriteLine( "}" );
		}

		private static void writeGetRowMatchingPkMethod(
			DBConnection cn, TextWriter writer, IDatabase database, string table, TableColumns tableColumns, bool isSmallTable, bool tableUsesRowVersionedCaching,
			bool isRevisionHistoryTable, int? commandTimeoutSeconds ) {
			var pkIsId = tableColumns.KeyColumns.Count() == 1 && tableColumns.KeyColumns.Single().Name.ToLower().EndsWith( "id" );
			var methodName = pkIsId ? "GetRowMatchingId" : "GetRowMatchingPk";
			var pkParameters = pkIsId
				                   ? tableColumns.KeyColumns.Single().DataTypeName + " id"
				                   : StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => $"{i.DataTypeName} {i.CamelCasedName}" ) );

			const string returnNullIfNoMatch = nameof( returnNullIfNoMatch );
			const string id = nameof( id );

			writer.CodeBlock(
				$"public static Row {methodName}( {pkParameters}, bool {returnNullIfNoMatch} = false ) {{",
				() => {
					if( isSmallTable ) {
						writer.WriteLine( "var cache = Cache.Current;" );
						var commandConditionsExpression = isRevisionHistoryTable ? "new [] {getLatestRevisionsCondition()}" : "new InlineDbCommandCondition[ 0 ]";
						writer.WriteLine( "cache.Queries.GetResultSet( " + commandConditionsExpression + ", commandConditions => {" );
						writeResultSetCreatorBody(
							cn,
							writer,
							database,
							table,
							tableColumns,
							tableUsesRowVersionedCaching,
							isRevisionHistoryTable,
							"true",
							commandTimeoutSeconds );
						writer.WriteLine( "} );" );

						var rowsByPkExpression = $"cache.{( isRevisionHistoryTable ? "LatestRevision" : "" )}RowsByPk";
						var pkTupleCreationArguments = pkIsId ? id : StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => i.CamelCasedName ) );
						writer.WriteLine( $"if( !{returnNullIfNoMatch} )" );
						writer.WriteLine( $"return {rowsByPkExpression}[ {TypeNames.Tuple}.Create( {pkTupleCreationArguments} ) ];" );
						writer.WriteLine( "Row row;" );
						writer.WriteLine( $"return {rowsByPkExpression}.TryGetValue( {TypeNames.Tuple}.Create( {pkTupleCreationArguments} ), out row ) ? row : null;" );
					}
					else {
						var equalityConditionClassName = DataAccessStatics.GetEqualityConditionClassName( cn, database, table, tableColumns.KeyColumns.Single() );
						var condition = pkIsId
							                ? $"new {equalityConditionClassName}( {id} )"
							                : StringTools.ConcatenateWithDelimiter(
								                ", ",
								                from keyColumn in tableColumns.KeyColumns
								                let className = DataAccessStatics.GetEqualityConditionClassName( cn, database, table, keyColumn )
								                select $"new {className}( {keyColumn.CamelCasedName} )" );
						writer.WriteLine( $"return GetRows( {condition} ).PrimaryKeySingle({returnNullIfNoMatch});" );
					}
				} );
		}

		private static void writeResultSetCreatorBody(
			DBConnection cn, TextWriter writer, IDatabase database, string table, TableColumns tableColumns, bool tableUsesRowVersionedCaching,
			bool excludesPreviousRevisions, string cacheQueryInDbExpression, int? commandTimeoutSeconds ) {
			if( tableUsesRowVersionedCaching ) {
				writer.WriteLine( "var results = new List<Row>();" );
				writer.WriteLine( DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteInTransaction( () => {" );

				// Query for the cache keys of the results.
				writer.WriteLine(
					"var keyCommand = {0};".FormatWith(
						getInlineSelectExpression(
							table,
							tableColumns,
							"{0}, \"{1}\"".FormatWith(
								StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => "\"{0}\"".FormatWith( i.Name ) ).ToArray() ),
								cn.DatabaseInfo is OracleInfo ? "ORA_ROWSCN" : tableColumns.RowVersionColumn.Name ),
							cacheQueryInDbExpression,
							commandTimeoutSeconds ) ) );
				writer.WriteLine( getCommandConditionAddingStatement( "keyCommand" ) );
				writer.WriteLine( $"var keys = new List<{TypeNames.Tuple}<{getPkAndVersionTupleTypeArguments( cn, tableColumns )}>>();" );
				var concatenateWithDelimiter = StringTools.ConcatenateWithDelimiter(
					", ",
					tableColumns.KeyColumns.Select( ( c, i ) => c.GetDataReaderValueExpression( "r", ordinalOverride: i ) ) );
				var o = cn.DatabaseInfo is OracleInfo
					        ? "({0})r.GetValue( {1} )".FormatWith( oracleRowVersionDataType, tableColumns.KeyColumns.Count() )
					        : tableColumns.RowVersionColumn.GetDataReaderValueExpression( "r", ordinalOverride: tableColumns.KeyColumns.Count() );
				writer.WriteLine(
					"keyCommand.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ", r => { while( r.Read() ) keys.Add( " +
					$"{TypeNames.Tuple}.Create( {concatenateWithDelimiter}, {o} )" + " ); } );" );

				writer.WriteLine( "var rowsByPkAndVersion = getRowsByPkAndVersion();" );
				writer.WriteLine( "var cachedKeyCount = keys.Where( i => rowsByPkAndVersion.ContainsKey( i ) ).Count();" );

				// If all but a few results are cached, execute a single-row query for each missing result.
				writer.CodeBlock(
					"if( cachedKeyCount >= keys.Count() - 1 || cachedKeyCount >= keys.Count() * .99 ) {",
					() => {
						writer.WriteLine( "foreach( var key in keys ) {" );
						writer.WriteLine( "results.Add( new Row( rowsByPkAndVersion.GetOrAdd( key, () => {" );
						writer.WriteLine( "var singleRowCommand = {0};".FormatWith( getInlineSelectExpression( table, tableColumns, "\"*\"", "false", commandTimeoutSeconds ) ) );
						foreach( var i in tableColumns.KeyColumns.Select( ( c, i ) => new { column = c, index = i } ) ) {
							writer.WriteLine(
								"singleRowCommand.AddCondition( ( ({0})new {1}( key.Item{2} ) ).CommandCondition );".FormatWith(
									DataAccessStatics.GetTableConditionInterfaceName( cn, database, table ),
									DataAccessStatics.GetEqualityConditionClassName( cn, database, table, i.column ),
									i.index + 1 ) );
						}
						writer.WriteLine( "var singleRowResults = new List<BasicRow>();" );
						writer.WriteLine(
							"singleRowCommand.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
							", r => { while( r.Read() ) singleRowResults.Add( new BasicRow( r ) ); } );" );
						writer.WriteLine( "return singleRowResults.Single();" );
						writer.WriteLine( "} ) ) );" );
						writer.WriteLine( "}" );
					} );
				// Otherwise, execute the full query.
				writer.CodeBlock(
					"else {",
					() => {
						writer.WriteLine(
							"var command = {0};".FormatWith(
								getInlineSelectExpression(
									table,
									tableColumns,
									cn.DatabaseInfo is OracleInfo ? "\"{0}.*\", \"ORA_ROWSCN\"".FormatWith( table ) : "\"*\"",
									cacheQueryInDbExpression,
									commandTimeoutSeconds ) ) );
						writer.WriteLine( getCommandConditionAddingStatement( "command" ) );
						writer.WriteLine( "command.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ", r => {" );
						writer.WriteLine(
							"while( r.Read() ) results.Add( new Row( rowsByPkAndVersion.GetOrAdd( System.Tuple.Create( {0}, {1} ), () => new BasicRow( r ) ) ) );".FormatWith(
								StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => i.GetDataReaderValueExpression( "r" ) ).ToArray() ),
								cn.DatabaseInfo is OracleInfo
									? "({0})r.GetValue( {1} )".FormatWith( oracleRowVersionDataType, tableColumns.AllColumns.Count() )
									: tableColumns.RowVersionColumn.GetDataReaderValueExpression( "r" ) ) );
						writer.WriteLine( "} );" );
					} );

				writer.WriteLine( "} );" );
			}
			else {
				writer.WriteLine(
					"var command = {0};".FormatWith( getInlineSelectExpression( table, tableColumns, @"""*""", cacheQueryInDbExpression, commandTimeoutSeconds ) ) );
				writer.WriteLine( getCommandConditionAddingStatement( "command" ) );
				writer.WriteLine( "var results = new List<Row>();" );
				writer.WriteLine(
					"command.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
					", r => { while( r.Read() ) results.Add( new Row( new BasicRow( r ) ) ); } );" );
			}

			// Add all results to RowsByPk.
			writer.WriteLine( "foreach( var i in results ) {" );
			var pkTupleCreationArgs = tableColumns.KeyColumns.Select( i => "i." + Utility.GetCSharpIdentifier( i.PascalCasedNameExceptForOracle ) );
			var pkTuple = "System.Tuple.Create( " + StringTools.ConcatenateWithDelimiter( ", ", pkTupleCreationArgs.ToArray() ) + " )";
			writer.WriteLine( $"cache.RowsByPk[ {pkTuple} ] = i;" );
			if( excludesPreviousRevisions )
				writer.WriteLine( $"cache.LatestRevisionRowsByPk[ {pkTuple} ] = i;" );
			writer.WriteLine( "}" );

			writer.WriteLine( "return results;" );
		}

		private static string getInlineSelectExpression(
			string table, TableColumns tableColumns, string selectExpressions, string cacheQueryInDbExpression, int? commandTimeoutSeconds ) {
			var concatenateWithDelimiter = StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => i.Name ) );
			return
				$@"new {TypeNames.InlineSelect}( new[] {{ {selectExpressions} }}, ""FROM {table}"", {cacheQueryInDbExpression}, {commandTimeoutSeconds?.ToString() ?? "null"}, orderByClause: ""ORDER BY {concatenateWithDelimiter}"" )";
		}

		private static string getCommandConditionAddingStatement( string commandName ) => $"foreach( var i in commandConditions ) {commandName}.AddCondition( i );";

		private static string getPkAndVersionTupleTypeArguments( DBConnection cn, TableColumns tableColumns )
			=>
				"{0}, {1}".FormatWith(
					getPkTupleTypeArguments( tableColumns ),
					cn.DatabaseInfo is OracleInfo ? oracleRowVersionDataType : tableColumns.RowVersionColumn.DataTypeName );

		private static string getPkTupleTypeArguments( TableColumns tableColumns ) {
			return StringTools.ConcatenateWithDelimiter( ", ", tableColumns.KeyColumns.Select( i => i.DataTypeName ).ToArray() );
		}

		private static void writeToIdDictionaryMethod( TextWriter writer, TableColumns tableColumns ) {
			writer.WriteLine( "public static Dictionary<" + tableColumns.KeyColumns.Single().DataTypeName + ", Row> ToIdDictionary( this IEnumerable<Row> rows ) {" );
			writer.WriteLine(
				"return rows.ToDictionary( i => i." + Utility.GetCSharpIdentifier( tableColumns.KeyColumns.Single().PascalCasedNameExceptForOracle ) + " );" );
			writer.WriteLine( "}" );
		}
	}
}