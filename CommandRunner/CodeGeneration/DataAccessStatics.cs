﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandRunner.CodeGeneration.Subsystems;
using CommandRunner.Collections;
using CommandRunner.DatabaseAbstraction;
using TypedDataLayer;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration {
	internal static class DataAccessStatics {
		internal const string CSharpTemplateFileExtension = ".ewlt.cs";

		/// <summary>
		/// Given a string, returns all instances of @abc in an ordered set containing abc (the token without the @ sign). If a token is used more than once, it
		/// only appears in the list once. A different prefix may be used for certain databases.
		/// </summary>
		internal static ListSet<string> GetNamedParamList( DatabaseInfo info, string statement ) {
			// We don't want to find parameters in quoted text.
			statement = statement.RemoveTextBetweenStrings( "'", "'" ).RemoveTextBetweenStrings( "\"", "\"" );

			var parameters = new ListSet<string>();
			foreach( Match match in Regex.Matches( statement, getParamRegex( info ) ) )
				parameters.Add( match.Value.Substring( 1 ) );

			return parameters;
		}

		// Matches spaced followed by @abc. The space prevents @@identity, etc. from getting matched.
		private static string getParamRegex( DatabaseInfo info ) => @"(?<!{0}){0}\w*\w".FormatWith( info.ParameterPrefix );

		/// <summary>
		/// Given raw query text such as that from Development.xml, returns a command that has had all of its parameters filled in with
		/// good dummy values and is ready to safely execute using schema only or key info behavior.
		/// </summary>
		internal static DbCommand GetCommandFromRawQueryText( DBConnection cn, string commandText ) {
			// This replacement is necessary because SQL Server chooses to care about the type of the parameter passed to TOP.
			commandText = Regex.Replace( commandText, @"TOP\( *@\w+ *\)", "TOP 0", RegexOptions.IgnoreCase );

			var cmd = cn.DatabaseInfo.CreateCommand();
			cmd.CommandText = commandText;
			foreach( var param in GetNamedParamList( cn.DatabaseInfo, cmd.CommandText ) )
				cmd.Parameters.Add( new DbCommandParameter( param, new DbParameterValue( "0" ) ).GetAdoDotNetParameter( cn.DatabaseInfo ) );
			return cmd;
		}

		internal static void WriteRowClasses(
			TextWriter writer, IEnumerable<Column> columns, Action<TextWriter> transactionPropertyWriter, Action<TextWriter> toModificationMethodWriter ) {
			// BasicRow

			writer.WriteLine( "internal class BasicRow {" );
			foreach( var column in columns.Where( i => !i.IsRowVersion ) )
				writer.WriteLine( "private readonly " + column.DataTypeName + " " + getMemberVariableName( column ) + ";" );

			writer.WriteLine( "internal BasicRow( DbDataReader reader ) {" );
			foreach( var column in columns.Where( i => !i.IsRowVersion ) )
				writer.WriteLine( $"{getMemberVariableName( column )} = {column.GetDataReaderValueExpression( "reader" )};" );
			writer.WriteLine( "}" );

			foreach( var column in columns.Where( i => !i.IsRowVersion ) ) {
				writer.WriteLine(
					"internal " + column.DataTypeName + " " + Utility.GetCSharpIdentifier( column.PascalCasedName ) + " { get { return " + getMemberVariableName( column ) +
					"; } }" );
			}

			writer.WriteLine( "}" );

			// Row

			CodeGenerationStatics.AddSummaryDocComment( writer, "Holds data for a row of this result." );
			writer.WriteLine( "public partial class Row: IEquatable<Row> {" );
			writer.WriteLine( "private readonly BasicRow __basicRow;" );

			writer.WriteLine( "internal Row( BasicRow basicRow ) {" );
			writer.WriteLine( "__basicRow = basicRow;" );
			writer.WriteLine( "}" );

			foreach( var column in columns.Where( i => !i.IsRowVersion ) )
				writeColumnProperty( writer, column );

			// NOTE: Being smarter about the hash code could make searches of the collection faster.
			writer.WriteLine( "public override int GetHashCode() { " );
			// NOTE: Catch an exception generated by not having any uniquely identifying columns and rethrow it as a ApplicationException.
			writer.WriteLine(
				"return " + Utility.GetCSharpIdentifier( columns.First( c => c.UseToUniquelyIdentifyRow ).PascalCasedNameExceptForOracle ) + ".GetHashCode();" );
			writer.WriteLine( "}" ); // Object override of GetHashCode

			writer.WriteLine( @"public static bool operator == ( Row row1, Row row2 ) => Equals( row1, row2 );
			public static bool operator !=( Row row1, Row row2 ) => !Equals( row1, row2 );" );

			writer.WriteLine( "public override bool Equals( object obj ) {" );
			writer.WriteLine( "return Equals( obj as Row );" );
			writer.WriteLine( "}" ); // Object override of Equals

			writer.WriteLine( "public bool Equals( Row other ) {" );
			writer.WriteLine( "if( other == null ) return false;" );

			var condition = "";
			foreach( var column in columns.Where( c => c.UseToUniquelyIdentifyRow ) ) {
				condition = StringTools.ConcatenateWithDelimiter(
					" && ",
					condition,
					Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) + " == other." + Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) );
			}
			writer.WriteLine( "return " + condition + ";" );
			writer.WriteLine( "}" ); // Equals method

			transactionPropertyWriter( writer );
			toModificationMethodWriter( writer );

			writer.WriteLine( "}" ); // class
		}

		private static void writeColumnProperty( TextWriter writer, Column column ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"This object will " + ( column.AllowsNull && !column.NullValueExpression.Any() ? "sometimes" : "never" ) + " be null." );
			writer.WriteLine(
				"public " + column.DataTypeName + " " + Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle ) + " { get { return __basicRow." +
				Utility.GetCSharpIdentifier( column.PascalCasedName ) + "; } }" );
		}

		// A single underscore is a pretty common thing for other code generators and even some developers to use, so two is more unique and avoids problems.
		private static string getMemberVariableName( Column column ) => Utility.GetCSharpIdentifier( "__" + column.CamelCasedName );

		internal static string GetMethodParamsFromCommandText( DatabaseInfo info, string commandText )
			=> StringTools.ConcatenateWithDelimiter( ", ", GetNamedParamList( info, commandText ).Select( i => "object " + i ).ToArray() );

		internal static void WriteAddParamBlockFromCommandText( TextWriter writer, string commandVariable, DatabaseInfo info, string commandText, IDatabase database ) {
			foreach( var param in GetNamedParamList( info, commandText ) ) {
				writer.WriteLine(
					commandVariable + ".Parameters.Add( new DbCommandParameter( \"" + param + "\", new DbParameterValue( " + param + " ) ).GetAdoDotNetParameter( " +
					DataAccessStateCurrentDatabaseConnectionExpression + ".DatabaseInfo ) );" );
			}
		}

		internal static bool IsRevisionHistoryTable( string table, Database configuration )
			=>
				configuration.revisionHistoryTables != null &&
				configuration.revisionHistoryTables.Any( revisionHistoryTable => revisionHistoryTable.EqualsIgnoreCase( table ) );

		internal static string GetTableConditionInterfaceName( DBConnection cn, IDatabase database, string table )
			=> "CommandConditions." + CommandConditionStatics.GetTableConditionInterfaceName( cn, table );

		internal static string GetEqualityConditionClassName( DBConnection cn, IDatabase database, string tableName, Column column )
			=>
				"CommandConditions." + CommandConditionStatics.GetTableEqualityConditionsClassName( cn, tableName ) + "." +
				CommandConditionStatics.GetConditionClassName( column );

		internal static void WriteGetLatestRevisionsConditionMethod( TextWriter writer, string revisionIdColumn ) {
			writer.WriteLine( "private static InlineDbCommandCondition getLatestRevisionsCondition() {" );
			writer.WriteLine( "var provider = RevisionHistoryStatics.SystemProvider;" );
			writer.WriteLine( $@"return new InCondition( ""{revisionIdColumn}"", provider.GetLatestRevisionsQuery() );" );
			writer.WriteLine( "}" );
		}

		internal static string TableNameToPascal( this string tableName, DBConnection cn )
			=> cn.DatabaseInfo is MySqlInfo ? tableName.OracleToEnglish().EnglishToPascal() : tableName;

		internal static readonly string DataAccessStateCurrentDatabaseConnectionExpression =
			$"{nameof( DataAccessState )}.{nameof( DataAccessState.Current )}.{nameof( DataAccessState.Current.DatabaseConnection )}";
	}
}