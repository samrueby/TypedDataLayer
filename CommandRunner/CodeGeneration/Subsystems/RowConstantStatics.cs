using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandRunner.DatabaseAbstraction;
using CommandRunner.Exceptions;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration.Subsystems {
	internal static class RowConstantStatics {
		private const string dictionaryName = "valuesAndNames";

		internal static void Generate( DBConnection cn, TextWriter writer, string baseNamespace, IDatabase database, Database configuration, IEnumerable<Table> tables ) {
			if( configuration.rowConstantTables == null )
				return;

			writer.WriteLine( "namespace " + baseNamespace + ".RowConstants {" );
			foreach( var table in configuration.rowConstantTables ) {
				var tableObject = tables.SingleOrDefault( t => t.ObjectIdentifier == table.tableName );
				if( tableObject == null )
					throw new UserCorrectableException( $"{table.tableName} does not match a known table. Ensure you have fully qualified it with the schema." );
				writer.WrapInTableNamespaceIfNecessary(
					tableObject,
					() => {
						Column valueColumn;
						var orderIsSpecified = !table.orderByColumn.IsNullOrWhiteSpace();
						var values = new List<string>();
						var names = new List<string>();
						try {
							var columns = new TableColumns( cn, table.tableName, false );
							valueColumn = columns.AllColumnsExceptRowVersion.Single( column => string.Equals( column.Name, table.valueColumn, StringComparison.CurrentCultureIgnoreCase ) );
							var nameColumn = columns.AllColumnsExceptRowVersion.Single( column => string.Equals( column.Name, table.nameColumn, StringComparison.CurrentCultureIgnoreCase ) );

							var cmd = new InlineSelect(
								new[] { valueColumn.Name, nameColumn.Name },
								"FROM " + table.tableName,
								false,
								configuration.CommandTimeoutSecondsTyped,
								orderByClause: orderIsSpecified ? "ORDER BY " + table.orderByColumn : "" );
							cmd.Execute(
								cn,
								reader => {
									while( reader.Read() ) {
										if( reader.IsDBNull( reader.GetOrdinal( valueColumn.Name ) ) )
											values.Add( valueColumn.NullValueExpression.Any() ? valueColumn.NullValueExpression : "null" );
										else {
											var valueString = valueColumn.ConvertIncomingValue( reader[ valueColumn.Name ] ).ToString();
											values.Add( valueColumn.DataTypeName == typeof( string ).ToString() ? "\"{0}\"".FormatWith( valueString ) : valueString );
										}

										names.Add( nameColumn.ConvertIncomingValue( reader[ nameColumn.Name ] ).ToString() );
									}
								} );
						}
						catch( Exception e ) {
							throw new ApplicationException(
								"Column or data retrieval failed for the " + table.tableName + " row constant table. Make sure the table and the value, name, and order by columns exist.",
								e );
						}

						CodeGenerationStatics.AddSummaryDocComment( writer, "Provides constants copied from the " + table.tableName + " table." );
						var className = tableObject.Name.TableNameToPascal( cn ) + "Rows";
						writer.WriteLine( $"public class {className} {{" );

						// constants
						for( var i = 0; i < values.Count; i++ ) {
							CodeGenerationStatics.AddSummaryDocComment( writer, "Constant generated from row in database table." );

							// It's important that row constants actually *be* constants (instead of static readonly) so they can be used in switch statements.
							writer.WriteLine( $"public const {valueColumn.DataTypeName} {Utility.GetCSharpIdentifier( names[ i ].CamelToEnglish().EnglishToPascal() )} = {values[ i ]};" );
						}

						// one to one map
						var dictionaryType = "OneToOneMap<" + valueColumn.DataTypeName + ", string>";
						writer.WriteLine( "private static readonly " + dictionaryType + " " + dictionaryName + " = new " + dictionaryType + "();" );

						writeStaticConstructor( writer, className, names, values, valueColumn.DataTypeName );

						// methods
						writeGetNameFromValueMethod( writer, valueColumn.DataTypeName );
						writeGetValueFromNameMethod( writer, valueColumn.DataTypeName );
						if( orderIsSpecified )
							writeGetValuesToNamesMethod( writer, valueColumn.DataTypeName );

						writer.WriteLine( "}" ); // class
					} );
			}

			writer.WriteLine( "}" ); // namespace
		}

		private static void writeStaticConstructor( TextWriter writer, string className, List<string> names, List<string> values, string valueTypeName ) {
			writer.WriteLine( "static " + className + "() {" );

			for( var i = 0; i < names.Count; i++ )
				writer.WriteLine( @"{0}.Add( ({1})({2}), ""{3}"" );".FormatWith( dictionaryName, valueTypeName, values[ i ], names[ i ] ) );

			writer.WriteLine( "}" ); // constructor
		}

		private static void writeGetNameFromValueMethod( TextWriter writer, string valueTypeName ) {
			CodeGenerationStatics.AddSummaryDocComment( writer, "Returns the name of the constant given the constant's value." );
			const string parameterName = "constantValue";
			writer.WriteLine( "public static string GetNameFromValue( " + valueTypeName + " " + parameterName + " ) {" );
			writer.WriteLine( "return " + dictionaryName + ".GetRightFromLeft( " + parameterName + " );" );
			writer.WriteLine( "}" ); // method
		}

		private static void writeGetValueFromNameMethod( TextWriter writer, string valueTypeName ) {
			CodeGenerationStatics.AddSummaryDocComment( writer, "Returns the value of the constant given the constant's name." );
			const string parameterName = "constantName";
			writer.WriteLine( "public static " + valueTypeName + " GetValueFromName( string " + parameterName + " ) {" );
			writer.WriteLine( "return " + dictionaryName + ".GetLeftFromRight( " + parameterName + " );" );
			writer.WriteLine( "}" ); // method
		}

		private static void writeGetValuesToNamesMethod( TextWriter writer, string valueTypeName ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"Returns a list of key value pairs where the key is the value of the row constant and the value is the name of the row constant." );
			writer.WriteLine( "public static ICollection<KeyValuePair<" + valueTypeName + ", string>> GetValuesToNames() {" );
			writer.WriteLine( "return valuesAndNames.GetAllPairs();" );
			writer.WriteLine( "}" ); // method
		}
	}
}