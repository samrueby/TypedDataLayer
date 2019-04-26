using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandRunner.DatabaseAbstraction;
using CommandRunner.Tools;
using TypedDataLayer.DataAccess;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration.Subsystems.StandardModification {
	internal static class StandardModificationStatics {
		private static TextWriter writer;
		private static IDatabase database;
		private static TableColumns columns;

		public static string GetNamespaceDeclaration( string baseNamespace, IDatabase database ) => "namespace " + baseNamespace + ".Modification {";

		internal static void Generate(
			DBConnection cn, TextWriter writer, string namespaceDeclaration, IDatabase database, IEnumerable<Table> tables, Database configuration ) {
			StandardModificationStatics.writer = writer;
			StandardModificationStatics.database = database;

			writer.WriteLine( namespaceDeclaration );
			foreach( var table in tables ) {
				var isRevisionHistoryTable = DataAccessStatics.IsRevisionHistoryTable( table.Name, configuration );

				writer.WrapInTableNamespaceIfNecessary(
					table,
					() => {
						writeClass( cn, table, isRevisionHistoryTable, false, configuration.CommandTimeoutSecondsTyped );
						if( isRevisionHistoryTable )
							writeClass( cn, table, true, true, configuration.CommandTimeoutSecondsTyped );
					} );
			}
			writer.WriteLine( "}" ); // Standard modification namespace.
		}

		internal static void WritePartialClass(
			DBConnection cn, string libraryBasePath, string namespaceDeclaration, IDatabase database, Table table, bool isRevisionHistoryTable ) {
			// We do not create templates for direct modification classes.
			var folderPath = Utility.CombinePaths( libraryBasePath, "DataAccess", "Modification" );
			var templateFilePath = Utility.CombinePaths(
				folderPath,
				getClassFilePath( cn, table ) + DataAccessStatics.CSharpTemplateFileExtension );
			IoMethods.DeleteFile( templateFilePath );

			// If a real file exists, don't create a template.
			if( File.Exists( Utility.CombinePaths( folderPath, getClassFilePath( cn, table ) + ".cs" ) ) )
				return;

			using( var templateWriter = IoMethods.GetTextWriterForWrite( templateFilePath ) ) {
				templateWriter.WriteLine( namespaceDeclaration );
				templateWriter.WrapInTableNamespaceIfNecessary(
					table,
					() => {
						templateWriter.WriteLine( $"\tpartial class {table.GetStandardModificationClassDeclaration()} {{" );
						templateWriter.WriteLine(
							"		// IMPORTANT: Change extension from \"{0}\" to \".cs\" before including in project and editing.".FormatWith(
								DataAccessStatics.CSharpTemplateFileExtension ) );
						templateWriter.WriteLine( "	}" ); // table class
					} );
				templateWriter.WriteLine( "}" ); // Standard Modification namespace
			}
		}

		private static void writeClass( DBConnection cn, Table table, bool isRevisionHistoryTable, bool isRevisionHistoryClass, int? commandTimeoutSeconds ) {
			columns = new TableColumns( cn, table.ObjectIdentifier, isRevisionHistoryClass );

			writer.WriteLine( $"public partial class {table.GetStandardModificationClassDeclaration()} {{" );

			var revisionHistorySuffix = GetRevisionHistorySuffix( isRevisionHistoryClass );

			// Write public static methods.
			writeInsertRowMethod( table, revisionHistorySuffix, "", columns.KeyColumns );
			writeInsertRowMethod( table, revisionHistorySuffix, "WithoutAdditionalLogic", columns.KeyColumns );
			writeUpdateRowsMethod( cn, table, revisionHistorySuffix, "" );
			writeUpdateRowsMethod( cn, table, revisionHistorySuffix, "WithoutAdditionalLogic" );
			writeDeleteRowsMethod( cn, table, revisionHistorySuffix, true );
			writeDeleteRowsMethod( cn, table, revisionHistorySuffix + "WithoutAdditionalLogic", false );
			writePrivateDeleteRowsMethod( cn, table, isRevisionHistoryClass, commandTimeoutSeconds );
			writer.WriteLine(
				$"static partial void preDelete( List<{table.GetTableConditionInterfaceReference()}> conditions, ref {getPostDeleteCallClassName( cn, table )} postDeleteCall );" );

			writer.WriteLine( "private ModificationType modType;" );
			writer.WriteLine( $"private List<{table.GetTableConditionInterfaceReference()}> conditions;" );

			foreach( var column in columns.AllColumnsExceptRowVersion )
				writeFieldsAndPropertiesForColumn( column );

			//foreach( var column in columns.DataColumns.Where( i => !columns.KeyColumns.Contains( i ) ) )
			//	FormItemStatics.WriteFormItemGetters( writer, column.GetModificationField() );

			// Write constructors.
			writeCreateForInsertMethod( cn, table, isRevisionHistoryTable, isRevisionHistoryClass, revisionHistorySuffix );
			writeCreateForUpdateMethod( cn, table, isRevisionHistoryTable, isRevisionHistoryClass, revisionHistorySuffix );
			if( columns.DataColumns.Any() )
				writeCreateForSingleRowUpdateMethod( cn, table, isRevisionHistoryTable, isRevisionHistoryClass, revisionHistorySuffix );
			writeGetConditionListMethod( cn, table );
			writer.WriteLine( $"private {table.GetStandardModificationClassDeclaration()}() {{}}" );

			if( columns.DataColumns.Any() )
				writeSetAllDataMethod();

			// Write execute methods and helpers.
			writeExecuteMethod( table );
			writer.WriteLine( "partial void preInsert();" );
			writer.WriteLine( "partial void preUpdate();" );
			writeExecuteWithoutAdditionalLogicMethod( table );
			writeExecuteInsertOrUpdateMethod( cn, table, isRevisionHistoryClass, columns.KeyColumns, columns.IdentityColumn, commandTimeoutSeconds );
			writeAddColumnModificationsMethod( columns.AllNonIdentityColumnsExceptRowVersion );
			if( isRevisionHistoryClass ) {
				writeCopyLatestRevisionsMethod( cn, table, columns.AllNonIdentityColumnsExceptRowVersion, commandTimeoutSeconds );
				DataAccessStatics.WriteGetLatestRevisionsConditionMethod( writer, columns.PrimaryKeyAndRevisionIdColumn.Name );
			}
			writeRethrowAsEwfExceptionIfNecessary();
			writer.WriteLine(
				"static partial void populateConstraintNamesToViolationErrorMessages( Dictionary<string,string> constraintNamesToViolationErrorMessages );" );
			writer.WriteLine( "partial void postInsert();" );
			writer.WriteLine( "partial void postUpdate();" );
			writeMarkColumnValuesUnchangedMethod();

			writer.WriteLine( "}" );
		}

		internal static string GetRevisionHistorySuffix( bool isRevisionHistoryClass ) => isRevisionHistoryClass ? "AsRevision" : "";

		private static void writeInsertRowMethod( Table table, string revisionHistorySuffix, string additionalLogicSuffix, IEnumerable<Column> keyColumns ) {
			Column returnColumn = null;
			var returnComment = "";
			if( keyColumns.Count() == 1 && !columns.DataColumns.Contains( keyColumns.Single() ) ) {
				returnColumn = keyColumns.Single();
				returnComment = " Returns the value of the " + returnColumn.Name + " column.";
			}

			// header
			CodeGenerationStatics.AddSummaryDocComment( writer, "Inserts a row into the " + table.ObjectIdentifier + " table." + returnComment );
			writeDocCommentsForColumnParams( columns.DataColumns );
			writer.Write( "public static " );
			writer.Write( returnColumn != null ? returnColumn.DataTypeName : "void" );
			writer.Write( " InsertRow" + revisionHistorySuffix + additionalLogicSuffix + "( " );
			writeColumnParameterDeclarations( columns.DataColumns );
			writer.WriteLine( " ) { " );

			// body
			writer.WriteLine( "var mod = CreateForInsert" + revisionHistorySuffix + "();" );
			writeColumnValueAssignmentsFromParameters( columns.DataColumns, "mod" );
			writer.WriteLine( "mod.Execute" + additionalLogicSuffix + "();" );
			if( returnColumn != null )
				writer.WriteLine( "return mod." + returnColumn.Name + ";" );
			writer.WriteLine( "}" );
		}

		private static void writeUpdateRowsMethod( DBConnection cn, Table table, string revisionHistorySuffix, string additionalLogicSuffix ) {
			// header
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Updates rows in the " + table.ObjectIdentifier + " table that match the specified conditions with the specified data." );
			writeDocCommentsForColumnParams( columns.DataColumns );
			CodeGenerationStatics.AddParamDocComment( writer, "requiredCondition", "A condition." ); // This prevents Resharper warnings.
			CodeGenerationStatics.AddParamDocComment( writer, "additionalConditions", "Additional conditions." ); // This prevents Resharper warnings.
			writer.Write( "public static void UpdateRows" + revisionHistorySuffix + additionalLogicSuffix + "( " );
			writeColumnParameterDeclarations( columns.DataColumns );
			if( columns.DataColumns.Any() )
				writer.Write( ", " );
			writer.WriteLine( "" + getConditionParameterDeclarations( cn, table ) + " ) {" );

			// body
			writer.WriteLine( "var mod = CreateForUpdate" + revisionHistorySuffix + "( requiredCondition, additionalConditions );" );
			writeColumnValueAssignmentsFromParameters( columns.DataColumns, "mod" );
			writer.WriteLine( "mod.Execute" + additionalLogicSuffix + "();" );
			writer.WriteLine( "}" );
		}

		private static void writeDeleteRowsMethod( DBConnection cn, Table table, string methodNameSuffix, bool executeAdditionalLogic ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"<para>Deletes the rows that match the specified conditions and returns the number of rows deleted.</para>" +
				"<para>WARNING: After calling this method, delete referenced rows in other tables that are no longer needed.</para>" );
			writer.WriteLine( "public static int DeleteRows" + methodNameSuffix + "( " + getConditionParameterDeclarations( cn, table ) + " ) {" );
			if( executeAdditionalLogic )
				writer.WriteLine( "return " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteInTransaction( () => {" );

			writer.WriteLine( "var conditions = getConditionList( requiredCondition, additionalConditions );" );

			if( executeAdditionalLogic ) {
				writer.WriteLine( getPostDeleteCallClassName( cn, table ) + " postDeleteCall = null;" );
				writer.WriteLine( "preDelete( conditions, ref postDeleteCall );" );
			}

			writer.WriteLine( "var rowsDeleted = deleteRows( conditions );" );

			if( executeAdditionalLogic ) {
				writer.WriteLine( "if( postDeleteCall != null )" );
				writer.WriteLine( "postDeleteCall.Execute();" );
			}

			writer.WriteLine( "return rowsDeleted;" );

			if( executeAdditionalLogic )
				writer.WriteLine( "} );" ); // cn.ExecuteInTransaction
			writer.WriteLine( "}" );
		}

		private static void writePrivateDeleteRowsMethod( DBConnection cn, Table table, bool isRevisionHistoryClass, int? commandTimeoutSeconds ) {
			// NOTE: For revision history tables, we should have the delete method automatically clean up the revisions table (but not user transactions) for us when doing direct-with-revision-bypass deletions.

			writer.WriteLine( $"private static int deleteRows( List<{table.GetTableConditionInterfaceReference()}> conditions ) {{" );
			if( isRevisionHistoryClass )
				writer.WriteLine( "return " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteInTransaction( () => {" );

			if( isRevisionHistoryClass )
				writer.WriteLine( "copyLatestRevisions( conditions );" );

			writer.WriteLine( $@"var delete = new {TypeNames.InlineDelete}( ""{table.ObjectIdentifier}"", {commandTimeoutSeconds?.ToString() ?? "null"} );" );
			writer.WriteLine( "conditions.ForEach( condition => delete.AddCondition( condition.CommandCondition ) );" );

			if( isRevisionHistoryClass )
				writer.WriteLine( "delete.AddCondition( getLatestRevisionsCondition() );" );

			writer.WriteLine( "try {" );
			writer.WriteLine( "return delete.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + " );" );
			writer.WriteLine( "}" ); // try
			writer.WriteLine( "catch(" + nameof( Exception ) + " e) {" );
			writer.WriteLine( "rethrowAsDataModificationExceptionIfNecessary( e );" );
			writer.WriteLine( "throw;" );
			writer.WriteLine( "}" ); // catch

			if( isRevisionHistoryClass )
				writer.WriteLine( "} );" ); // cn.ExecuteInTransaction
			writer.WriteLine( "}" );
		}

		private static string getPostDeleteCallClassName( DBConnection cn, Table table )
			=> "PostDeleteCall<IEnumerable<TableRetrieval." + table.GetTableRetrievalClassReference() + ".Row>>";

		private static void writeFieldsAndPropertiesForColumn( Column column ) {
			var columnIsReadOnly = !columns.DataColumns.Contains( column );

			writer.WriteLine(
				$"private readonly {TypeNames.DataValue}<{column.DataTypeName}> {getColumnFieldName( column )} = new {TypeNames.DataValue}<{column.DataTypeName}>();" );
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Gets " + ( columnIsReadOnly ? "" : "or sets " ) + "the value for the " + column.Name +
				" column. Throws an exception if the value has not been initialized. " + getComment( column ) );
			var propertyDeclarationBeginning = "public " + column.DataTypeName + " " + Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) +
			                                   " { get { return " + getColumnFieldName( column ) + ".Value; } ";
			if( columnIsReadOnly ) {
				writer.WriteLine( propertyDeclarationBeginning + "}" );
			}
			else {
				writer.WriteLine( propertyDeclarationBeginning + "set { " + getColumnFieldName( column ) + ".Value = value; } }" );

				CodeGenerationStatics.AddSummaryDocComment(
					writer,
					"Indicates whether or not the value for the " + column.Name + " has been set since object creation or the last call to Execute, whichever was latest." );
				writer.WriteLine(
					"public bool " + Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle + "HasChanged" ) + " { get { return " + getColumnFieldName( column ) +
					".Changed; } }" );
			}
		}

		private static void writeCreateForInsertMethod(
			DBConnection cn, Table table, bool isRevisionHistoryTable, bool isRevisionHistoryClass, string methodNameSuffix ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Creates a modification object in insert mode, which can be used to do a piecemeal insert of a new row in the " + table + " table." );
			var className = table.GetStandardModificationClassReference();
			writer.WriteLine( $"public static {className} CreateForInsert{methodNameSuffix}() {{" );
			writer.WriteLine( $"return new {className} {{ modType = ModificationType.Insert }};" );
			writer.WriteLine( "}" );
		}

		private static void writeCreateForUpdateMethod(
			DBConnection cn, Table table, bool isRevisionHistoryTable, bool isRevisionHistoryClass, string methodNameSuffix ) {
			// header
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Creates a modification object in update mode with the specified conditions, which can be used to do a piecemeal update of the " + table.ObjectIdentifier + " table." );
			writer.WriteLine(
				$"public static {table.GetStandardModificationClassReference()} CreateForUpdate{methodNameSuffix}( {getConditionParameterDeclarations( cn, table )} ) {{" );


			// body

			writer.WriteLine(
				$"var mod = new {table.GetStandardModificationClassReference()} {{ modType = ModificationType.Update, conditions = getConditionList( requiredCondition, additionalConditions ) }};" );

			// Set column values that correspond to modification conditions to the values of those conditions. One reason this is important is so the primary
			// key can be retrieved in a consistent way regardless of whether the modification object is an insert or an update.
			writer.WriteLine( "foreach( var condition in mod.conditions ) {" );
			var prefix = "if";
			foreach( var column in columns.AllColumnsExceptRowVersion ) {
				writer.WriteLine( $"{prefix}( condition is {table.GetEqualityConditionClassReference( cn, column )} )" );
				writer.WriteLine( $"mod.{getColumnFieldName( column )}.Value = ( condition as {table.GetEqualityConditionClassReference( cn, column )} ).Value;" );
				prefix = "else if";
			}
			writer.WriteLine( "}" );
			writer.WriteLine( writer.NewLine + "mod.markColumnValuesUnchanged();" );

			writer.WriteLine( "return mod;" );
			writer.WriteLine( "}" );
		}

		private static void writeCreateForSingleRowUpdateMethod(
			DBConnection cn, Table table, bool isRevisionHistoryTable, bool isRevisionHistoryClass, string methodNameSuffix ) {
			// header
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Creates a modification object in single-row update mode with the specified current data. All column values in this object will have HasChanged = false, despite being initialized. This object can then be used to do a piecemeal update of the " +
				table.ObjectIdentifier + " table." );
			writer.Write( $"public static {table.GetStandardModificationClassReference()} CreateForSingleRowUpdate{methodNameSuffix}( " );
			writeColumnParameterDeclarations( columns.AllColumnsExceptRowVersion );
			writer.WriteLine( " ) {" );


			// body

			writer.WriteLine( $"var mod = new {table.GetStandardModificationClassReference()} {{ modType = ModificationType.Update }};" );

			// Use the values of key columns as conditions.
			writer.WriteLine( $"mod.conditions = new List<{table.GetTableConditionInterfaceReference()}>();" );
			foreach( var column in columns.KeyColumns ) {
				writer.WriteLine(
					$"mod.conditions.Add( new {table.GetEqualityConditionClassReference( cn, column )}( {Utility.GetCSharpIdentifier( column.CamelCasedName )} ) );" );
			}

			writeColumnValueAssignmentsFromParameters( columns.AllColumnsExceptRowVersion, "mod" );
			writer.WriteLine( "mod.markColumnValuesUnchanged();" );
			writer.WriteLine( "return mod;" );
			writer.WriteLine( "}" );
		}

		private static void writeGetConditionListMethod( DBConnection cn, Table table ) {
			writer.WriteLine(
				$"private static List<{table.GetTableConditionInterfaceReference()}> getConditionList( {getConditionParameterDeclarations( cn, table )} ) {{" );
			writer.WriteLine( $"var conditions = new List<{table.GetTableConditionInterfaceReference()}>();" );
			writer.WriteLine( "conditions.Add( requiredCondition );" );
			writer.WriteLine( "foreach( var condition in additionalConditions )" );
			writer.WriteLine( "conditions.Add( condition );" );
			writer.WriteLine( "return conditions;" );
			writer.WriteLine( "}" );
		}

		private static string getConditionParameterDeclarations( DBConnection cn, Table table )
			=>
				$"{table.GetTableConditionInterfaceReference()} requiredCondition, params {table.GetTableConditionInterfaceReference()}[] additionalConditions";

		private static string getClassFilePath( DBConnection cn, Table table ) {
			var fileName = table.Name.TableNameToPascal( cn ) + "Modification";
			if( table.Schema.Any() )
				return Path.Combine( table.Schema.TableNameToPascal( cn ), fileName );

			return fileName;
		}

		private static void writeSetAllDataMethod() {
			// header
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Sets all column values. This is useful for enforcing the number of arguments when deferred execution is needed." );
			writeDocCommentsForColumnParams( columns.DataColumns );
			writer.Write( "public void SetAllData( " );
			writeColumnParameterDeclarations( columns.DataColumns );
			writer.WriteLine( " ) {" );

			// body
			writeColumnValueAssignmentsFromParameters( columns.DataColumns, "this" );
			writer.WriteLine( "}" );
		}

		private static void writeDocCommentsForColumnParams( IEnumerable<Column> columns ) {
			foreach( var column in columns )
				CodeGenerationStatics.AddParamDocComment( writer, column.CamelCasedName, getComment( column ) );
		}

		private static string getComment( Column column )
			=> column.AllowsNull && !column.NullValueExpression.Any() ? "Object allows null." : "Object does not allow null.";

		private static void writeColumnParameterDeclarations( IEnumerable<Column> columns ) {
			writer.Write(
				StringTools.ConcatenateWithDelimiter( ", ", columns.Select( i => i.DataTypeName + " " + Utility.GetCSharpIdentifier( i.CamelCasedName ) ).ToArray() ) );
		}

		private static void writeColumnValueAssignmentsFromParameters( IEnumerable<Column> columns, string modObjectName ) {
			foreach( var column in columns )
				writer.WriteLine( modObjectName + "." + getColumnFieldName( column ) + ".Value = " + Utility.GetCSharpIdentifier( column.CamelCasedName ) + ";" );
		}

		private static void writeExecuteMethod( Table table ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Executes this " + table.ObjectIdentifier +
				" modification, persisting all changes. Executes any pre-insert, pre-update, post-insert, or post-update logic that may exist in the class." );
			writer.WriteLine( "public void Execute() {" );
			writer.WriteLine( DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteInTransaction( () => {" );

			// The mod type may change during execute.
			writer.WriteLine( "var frozenModType = modType;" );

			writer.WriteLine( "if( frozenModType == ModificationType.Insert )" );
			writer.WriteLine( "preInsert();" );
			writer.WriteLine( "else if( frozenModType == ModificationType.Update )" );
			writer.WriteLine( "preUpdate();" );

			writer.WriteLine( "executeInsertOrUpdate();" );

			writer.WriteLine( "if( frozenModType == ModificationType.Insert )" );
			writer.WriteLine( "postInsert();" );
			writer.WriteLine( "else if( frozenModType == ModificationType.Update )" );
			writer.WriteLine( "postUpdate();" );

			// This must be after the calls to postInsert and postUpdate in case their implementations need to know which column values changed.
			writer.WriteLine( "markColumnValuesUnchanged();" );

			writer.WriteLine( "} );" );
			writer.WriteLine( "}" );
		}

		private static void writeExecuteWithoutAdditionalLogicMethod( Table table ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Executes this " + table.ObjectIdentifier +
				" modification, persisting all changes. Does not execute pre-insert, pre-update, post-insert, or post-update logic that may exist in the class." );
			writer.WriteLine( "public void ExecuteWithoutAdditionalLogic() {" );
			writer.WriteLine( "executeInsertOrUpdate();" );
			writer.WriteLine( "markColumnValuesUnchanged();" );
			writer.WriteLine( "}" );
		}

		private static void writeExecuteInsertOrUpdateMethod(
			DBConnection cn, Table table, bool isRevisionHistoryClass, IEnumerable<Column> keyColumns, Column identityColumn, int? commandTimeoutSeconds ) {
			writer.WriteLine( "private void executeInsertOrUpdate() {" );
			writer.WriteLine( "try {" );
			if( isRevisionHistoryClass )
				writer.WriteLine( DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteInTransaction( () => {" );


			// insert

			writer.WriteLine( "if( modType == ModificationType.Insert ) {" );

			// If this is a revision history table, write code to insert a new revision when a row is inserted into this table.
			if( isRevisionHistoryClass ) {
				writer.WriteLine( "var revisionHistorySetup = RevisionHistoryStatics.SystemProvider;" );
				writer.WriteLine( getColumnFieldName( columns.PrimaryKeyAndRevisionIdColumn ) + ".Value = revisionHistorySetup.GetNextMainSequenceValue();" );
				writer.WriteLine(
					"revisionHistorySetup.InsertRevision( System.Convert.ToInt32( " + getColumnFieldName( columns.PrimaryKeyAndRevisionIdColumn ) +
					".Value ), System.Convert.ToInt32( " + getColumnFieldName( columns.PrimaryKeyAndRevisionIdColumn ) + ".Value ), " +
					DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".GetUserTransactionId() );" );
			}

			writer.WriteLine( $@"var insert = new {TypeNames.InlineInsert}( ""{table.ObjectIdentifier}"", {( identityColumn != null ? "true" : "false" )}, {commandTimeoutSeconds?.ToString() ?? "null"} );" );
			writer.WriteLine( "addColumnModifications( insert );" );
			if( identityColumn != null ) {
				// One reason the ChangeType call is necessary: SQL Server identities always come back as decimal, and you can't cast a boxed decimal to an int.
				var convertedIdentityValue = identityColumn.GetIncomingValueConversionExpression(
					$"{nameof(Utility)}.{nameof(Utility.ChangeType)}( insert.Execute( {DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression} ), typeof( {identityColumn.UnconvertedDataTypeName} ) )" );
				writer.WriteLine( $"{getColumnFieldName( identityColumn )}.Value = {convertedIdentityValue};" );
			} 
			else
				writer.WriteLine( "insert.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + " );" );

			// Future calls to Execute should perform updates, not inserts. Use the values of key columns as conditions.

			// We have to abort turning it into an update if doing so would result in an exception from trying to retrieve a key value that hasn't been set or retrieved (as in the case where the primary key is set by a default constraint, for example).
			if( identityColumn == null ) {
				foreach( var column in keyColumns ) {
					writer.WriteLine( $"if( !{ Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) }HasChanged ) return;" );
				}
			}

			writer.WriteLine( "modType = ModificationType.Update;" );
			writer.WriteLine( $"conditions = new List<{table.GetTableConditionInterfaceReference()}>();" );
			foreach( var column in keyColumns ) {
				writer.WriteLine(
					$"conditions.Add( new {table.GetEqualityConditionClassReference( cn, column )}( {Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle )} ) );" );
			}

			writer.WriteLine( "}" ); // if insert


			// update
			writer.WriteLine( "else {" );
			if( isRevisionHistoryClass )
				writer.WriteLine( "copyLatestRevisions( conditions );" );
			writer.WriteLine( $@"var update = new {TypeNames.InlineUpdate}( ""{table.ObjectIdentifier}"", {commandTimeoutSeconds?.ToString() ?? "null"} );" );
			writer.WriteLine( "addColumnModifications( update );" );
			writer.WriteLine( "conditions.ForEach( condition => update.AddCondition( condition.CommandCondition ) );" );
			if( isRevisionHistoryClass )
				writer.WriteLine( "update.AddCondition( getLatestRevisionsCondition() );" );
			writer.WriteLine( "update.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + " );" );
			writer.WriteLine( "}" ); // else

			if( isRevisionHistoryClass )
				writer.WriteLine( "} );" ); // cn.ExecuteInTransaction
			writer.WriteLine( "}" ); // try

			writer.WriteLine( "catch(" + nameof( Exception ) + " " + "e) {" );
			writer.WriteLine( "rethrowAsDataModificationExceptionIfNecessary( e );" );
			writer.WriteLine( "throw;" );
			writer.WriteLine( "}" ); // catch

			writer.WriteLine( "}" ); // method
		}

		private static void writeAddColumnModificationsMethod( IEnumerable<Column> nonIdentityColumns ) {
			writer.WriteLine( "private void addColumnModifications( InlineDbModificationCommand cmd ) {" );
			foreach( var column in nonIdentityColumns ) {
				writer.WriteLine( "if( " + getColumnFieldName( column ) + ".Changed )" );
				var columnValueExpression = column.GetCommandColumnValueExpression( Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) );
				writer.WriteLine( "cmd.AddColumnModification( " + columnValueExpression + " );" );
			}
			writer.WriteLine( "}" );
		}

		private static void writeCopyLatestRevisionsMethod( DBConnection cn, Table table, IEnumerable<Column> nonIdentityColumns, int? commandTimeoutSeconds ) {
			writer.WriteLine( $"private static void copyLatestRevisions( List<{table.GetTableConditionInterfaceReference()}> conditions ) {{" );

			writer.WriteLine( "var revisionHistorySetup = RevisionHistoryStatics.SystemProvider;" );

			writer.WriteLine(
				$@"var command = new {TypeNames.InlineSelect}( ""new [] {{{columns.PrimaryKeyAndRevisionIdColumn.Name}""}}, ""FROM {table.ObjectIdentifier}"", false, {commandTimeoutSeconds
					                                                                                                                                          ?.ToString() ??
				                                                                                                                                          "null"} );" );
			writer.WriteLine( "conditions.ForEach( condition => command.AddCondition( condition.CommandCondition ) );" );
			writer.WriteLine( "command.AddCondition( getLatestRevisionsCondition() );" );
			writer.WriteLine( "var latestRevisionIds = new List<int>();" );
			writer.WriteLine(
				"command.Execute( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
				", r => { while( r.Read() ) latestRevisionIds.Add( System.Convert.ToInt32( r[0] ) ); } );" );
			writer.WriteLine( "foreach( var latestRevisionId in latestRevisionIds ) {" );

			// Get the latest revision.
			writer.WriteLine( "var latestRevision = revisionHistorySetup.GetRevision( latestRevisionId );" );

			// If this condition is true, we've already modified the row in this transaction. If we were to copy it, we'd end up with two revisions of the same entity
			// in the same user transaction, which we don't support.
			writer.WriteLine(
				"if( latestRevision.UserTransactionId == " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".GetUserTransactionId() )" );
			writer.WriteLine( "continue;" );

			// Update the latest revision with a new user transaction.
			writer.WriteLine(
				"revisionHistorySetup.UpdateRevision( latestRevisionId, latestRevisionId, " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
				".GetUserTransactionId(), latestRevisionId );" );

			// Insert a copy of the latest revision with a new ID. This will represent the revision of the data before it was changed.
			writer.WriteLine( "var copiedRevisionId = revisionHistorySetup.GetNextMainSequenceValue();" );
			writer.WriteLine( "revisionHistorySetup.InsertRevision( copiedRevisionId, latestRevisionId, latestRevision.UserTransactionId );" );

			// Insert a copy of the data row and make it correspond to the copy of the latest revision.
			writer.WriteLine( "var copyCommand = " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionCreateCommandExpression( commandTimeoutSeconds ) + ";" );
			writer.WriteLine( "copyCommand.CommandText = \"INSERT INTO " + table.ObjectIdentifier + " SELECT \";" );
			foreach( var column in nonIdentityColumns ) {
				if( column == columns.PrimaryKeyAndRevisionIdColumn ) {
					writer.WriteLine( "var revisionIdParameter = new DbCommandParameter( \"copiedRevisionId\", new DbParameterValue( copiedRevisionId ) );" );
					writer.WriteLine(
						"copyCommand.CommandText += revisionIdParameter.GetNameForCommandText( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
						".DatabaseInfo ) + \", \";" );
					writer.WriteLine(
						"copyCommand.Parameters.Add( revisionIdParameter.GetAdoDotNetParameter( " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression +
						".DatabaseInfo ) );" );
				}
				else {
					writer.WriteLine( "copyCommand.CommandText += \"" + column.Name + ", \";" );
				}
			}
			writer.WriteLine( "copyCommand.CommandText = copyCommand.CommandText.Remove( copyCommand.CommandText.Length - 2 );" );
			writer.WriteLine( "copyCommand.CommandText += \" FROM " + table.ObjectIdentifier + " WHERE \";" );
			writer.WriteLine(
				"( new EqualityCondition( new InlineDbCommandColumnValue( \"" + columns.PrimaryKeyAndRevisionIdColumn.Name +
				"\", new DbParameterValue( latestRevisionId ) ) ) as InlineDbCommandCondition ).AddToCommand( copyCommand, " +
				DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".DatabaseInfo, \"latestRevisionId\" );" );
			writer.WriteLine( DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteNonQueryCommand( copyCommand );" );

			writer.WriteLine( "}" ); // foreach
			writer.WriteLine( "}" ); // method
		}

		private static void writeRethrowAsEwfExceptionIfNecessary() {
			writer.WriteLine( "private static void rethrowAsDataModificationExceptionIfNecessary( System.Exception e ) {" );
			writer.WriteLine( "var constraintNamesToViolationErrorMessages = new Dictionary<string,string>();" );
			writer.WriteLine( "populateConstraintNamesToViolationErrorMessages( constraintNamesToViolationErrorMessages );" );
			writer.WriteLine( "foreach( var pair in constraintNamesToViolationErrorMessages )" );
			writer.WriteLine( "if( e.GetBaseException().Message.ToLower().Contains( pair.Key.ToLower() ) ) throw new DataModificationException( pair.Value );" );
			writer.WriteLine( "}" ); // method
		}

		private static void writeMarkColumnValuesUnchangedMethod() {
			writer.WriteLine( "private void markColumnValuesUnchanged() {" );
			foreach( var column in columns.AllColumnsExceptRowVersion )
				writer.WriteLine( getColumnFieldName( column ) + ".ClearChanged();" );
			writer.WriteLine( "}" );
		}

		private static string getColumnFieldName( Column column ) => Utility.GetCSharpIdentifier( column.CamelCasedName + "ColumnValue" );
	}
}