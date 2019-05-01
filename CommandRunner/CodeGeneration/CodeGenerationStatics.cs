using System;
using System.IO;
using System.Linq;
using CommandRunner.DatabaseAbstraction;

namespace CommandRunner.CodeGeneration {
	internal static class CodeGenerationStatics {
		internal static void AddSummaryDocComment( TextWriter writer, string text ) {
			if( text.Length == 0 )
				return;
			text = text.Replace( writer.NewLine, writer.NewLine + "/// " );
			writer.WriteLine( "/// <summary>" );
			writer.WriteLine( "/// " + text );
			writer.WriteLine( "/// </summary>" );
		}

		internal static void AddParamDocComment( TextWriter writer, string name, string description ) {
			if( description.Length == 0 )
				return;
			writer.WriteLine( "/// <param name=\"" + name + "\">" + description + "</param>" );
		}

		internal static void AddGeneratedCodeUseOnlyComment( TextWriter writer ) => AddSummaryDocComment( writer, "Auto-generated code use only." );

		/// <summary>
		/// Closes your block for you with a }.
		/// </summary>
		/// <param name="writer">The writer being used.</param>
		/// <param name="begin">The expression beginning the block, which should end with {.</param>
		/// <param name="a">immediately executed.</param>
		internal static void CodeBlock( this TextWriter writer, string begin, Action a ) {
			writer.WriteLine( begin );
			a();
			writer.WriteLine( "}" );
		}

		internal static void WrapInTableNamespaceIfNecessary( this TextWriter writer, Table table, Action a ) {
			if( table.Schema.Any() )
				CodeBlock( writer, $"namespace {table.Schema} {{", a );
			else
				a();
		}
	}
}