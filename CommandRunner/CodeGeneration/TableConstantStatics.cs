using System.Collections.Generic;
using System.IO;
using CommandRunner.DatabaseAbstraction;
using TypedDataLayer.DataAccess;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration {
	internal static class TableConstantStatics {
		internal static void Generate( DBConnection cn, TextWriter writer, string baseNamespace, IDatabase database, IEnumerable<Table> tables ) {
			writer.WriteLine( "namespace " + baseNamespace + ".TableConstants {" );
			foreach( var table in tables ) {
				writer.WrapInTableNamespaceIfNecessary(
					table,
					() => {
						CodeGenerationStatics.AddSummaryDocComment( writer, "This object represents the constants of the " + table + " table." );
						writer.WriteLine( "public class " + Utility.GetCSharpIdentifier( table.Name.TableNameToPascal( cn ) + "Table" ) + " {" );

						CodeGenerationStatics.AddSummaryDocComment( writer, "The name of this table." );
						writer.WriteLine( "public const string Name = \"" + table.ObjectIdentifier + "\";" );

						foreach( var column in new TableColumns( cn, table.ObjectIdentifier, false ).AllColumnsExceptRowVersion ) {
							CodeGenerationStatics.AddSummaryDocComment( writer, "Contains schema information about this column." );
							writer.WriteLine( "public class " + Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle + "Column" ) + " {" );

							CodeGenerationStatics.AddSummaryDocComment( writer, "The name of this column." );
							writer.WriteLine( "public const string Name = \"" + column.Name + "\";" );

							CodeGenerationStatics.AddSummaryDocComment(
								writer,
								"The size of this column. For varchars, this is the length of the biggest string that can be stored in this column." );
							writer.WriteLine( "public const int Size = " + column.Size + ";" );

							writer.WriteLine( "}" ); // Column class.
						}

						writer.WriteLine( "}" ); // Table class.
					} );
			}

			writer.WriteLine( "}" ); // TableConstants namespace.
		}
	}
}