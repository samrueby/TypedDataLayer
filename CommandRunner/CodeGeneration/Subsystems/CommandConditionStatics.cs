using System.Collections.Generic;
using System.IO;
using CommandRunner.DatabaseAbstraction;
using TypedDataLayer.DataAccess;
using TypedDataLayer.Tools;

namespace CommandRunner.CodeGeneration.Subsystems {
	internal static class CommandConditionStatics {
		internal static void Generate( DBConnection cn, TextWriter writer, string baseNamespace, IDatabase database, IEnumerable<Table> tableNames ) {
			writer.WriteLine( "namespace " + baseNamespace + ".CommandConditions {" );
			foreach( var table in tableNames ) {
				writer.WrapInTableNamespaceIfNecessary( table, () => {
					// Write the interface for all of the table's conditions.
					writer.WriteLine( "public interface " + GetTableConditionInterfaceName( cn, table.Name ) + ": TableCondition {}" );

					writeEqualityConditionClasses( cn, writer, table );
					writeInequalityConditionClasses( cn, writer, table );
					writeInConditionClasses( cn, writer, table );
					writeLikeConditionClasses( cn, writer, table );
				} );
			}
			writer.WriteLine( "}" ); // CommandConditions namespace
		}

		private static void writeEqualityConditionClasses( DBConnection cn, TextWriter writer, Table table ) {
			writer.WriteLine( "public static class " + GetTableEqualityConditionsClassName( cn, table.Name ) + " {" );
			foreach( var column in new TableColumns( cn, table.ObjectIdentifier, false ).AllColumnsExceptRowVersion ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column ) + ": " + GetTableConditionInterfaceName( cn, table.Name ) + " {" );
				writer.WriteLine( "private readonly " + column.DataTypeName + " value;" );

				CodeGenerationStatics.AddSummaryDocComment( writer, "Creates a condition to narrow the scope of a command." );
				writer.WriteLine( "public " + GetConditionClassName( column ) + "( " + column.DataTypeName + " value ) {" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				writer.WriteLine( "internal " + column.DataTypeName + " Value { get { return value; } }" );
				var colVal = column.GetCommandColumnValueExpression( "value" );
				writer.WriteLine( "InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( " + colVal + " ); } }" );
				writer.WriteLine( "}" );
			}
			writer.WriteLine( "}" ); // class
		}

		internal static string GetTableEqualityConditionsClassName( DBConnection cn, string table )
			=> Utility.GetCSharpIdentifier( table.TableNameToPascal( cn ) + "TableEqualityConditions" );

		private static void writeInequalityConditionClasses( DBConnection cn, TextWriter writer, Table table ) {
			// NOTE: This kind of sucks. It seems like we could use generics to not have to write N of these methods into ISU.cs.
			writer.WriteLine( "public static class " + Utility.GetCSharpIdentifier( table.Name.TableNameToPascal( cn ) + "TableInequalityConditions" ) + " {" );
			foreach( var column in new TableColumns( cn, table.ObjectIdentifier, false ).AllColumnsExceptRowVersion ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column ) + ": " + GetTableConditionInterfaceName( cn, table.Name ) + " {" );
				writer.WriteLine( "private readonly InequalityCondition.Operator op; " );
				writer.WriteLine( "private readonly " + column.DataTypeName + " value;" );

				CodeGenerationStatics.AddSummaryDocComment(
					writer,
					"Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'." );
				writer.WriteLine( "public " + GetConditionClassName( column ) + "( InequalityCondition.Operator op, " + column.DataTypeName + " value ) {" );
				writer.WriteLine( "this.op = op;" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				var colVal = column.GetCommandColumnValueExpression( "value" );
				writer.WriteLine( "InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, " + colVal + " ); } }" );
				writer.WriteLine( "}" );
			}
			writer.WriteLine( "}" ); // class
		}

		private static void writeInConditionClasses( DBConnection cn, TextWriter writer, Table table ) {
			writer.WriteLine( "public static class " + Utility.GetCSharpIdentifier( table.Name.TableNameToPascal( cn ) + "TableInConditions" ) + " {" );
			foreach( var column in new TableColumns( cn, table.ObjectIdentifier, false ).AllColumnsExceptRowVersion ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column ) + ": " + GetTableConditionInterfaceName( cn, table.Name ) + " {" );
				writer.WriteLine( "private readonly string subQuery;" );

				CodeGenerationStatics.AddSummaryDocComment( writer, "Creates a condition to narrow the scope of a command." );
				writer.WriteLine( "public " + GetConditionClassName( column ) + "( string subQuery ) {" );
				writer.WriteLine( "this.subQuery = subQuery;" );
				writer.WriteLine( "}" );

				writer.WriteLine( "InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( \"" + column.Name + "\", subQuery ); } }" );

				writer.WriteLine( "}" );
			}
			writer.WriteLine( "}" );
		}

		private static void writeLikeConditionClasses( DBConnection cn, TextWriter writer, Table table ) {
			writer.WriteLine( "public static class " + Utility.GetCSharpIdentifier( table.Name.TableNameToPascal( cn ) + "TableLikeConditions" ) + " {" );
			foreach( var column in new TableColumns( cn, table.ObjectIdentifier, false ).AllColumnsExceptRowVersion ) {
				CodeGenerationStatics.AddSummaryDocComment( writer, "A condition that narrows the scope of a command." );
				writer.WriteLine( "public class " + GetConditionClassName( column ) + ": " + GetTableConditionInterfaceName( cn, table.Name ) + " {" );
				writer.WriteLine( "private readonly LikeCondition.Behavior behavior; " );
				writer.WriteLine( "private readonly string value;" );

				CodeGenerationStatics.AddSummaryDocComment( writer, "Creates a condition to narrow the scope of a command." );
				writer.WriteLine( "public " + GetConditionClassName( column ) + "( LikeCondition.Behavior behavior, string value ) {" );
				writer.WriteLine( "this.behavior = behavior;" );
				writer.WriteLine( "this.value = value;" );
				writer.WriteLine( "}" );

				writer.WriteLine(
					"InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, \"" + column.Name + "\", value ); } }" );

				writer.WriteLine( "}" ); // class
			}
			writer.WriteLine( "}" ); // class
		}

		// GMS NOTE: Now that table is an object, these methods could be on the Table object.
		internal static string GetTableConditionInterfaceName( DBConnection cn, string tableName ) => tableName.TableNameToPascal( cn ) + "TableCondition";

		internal static string GetConditionClassName( Column column )
			=> Utility.GetCSharpIdentifier( column.PascalCasedNameExceptForOracle == "Value" ? "_Value" : column.PascalCasedNameExceptForOracle );
	}
}