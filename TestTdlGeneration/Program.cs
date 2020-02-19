using System.Diagnostics;

namespace Tdl.TestGeneration {
	class Program {
		static void Main( string[] args ) {
			Debug.WriteLine( "Hello World!" );
			//var workingDirectory = ConfigurationManager.AppSettings[ "token" ];
			CommandRunner.Program.LoadConfigAndRunUpdateAllDependentLogic( @"C:\GitHub\TypedDataLayer\TestTdlGeneration", true );
		}

		/*
		Where to try runtime retrieval of "keys" or columns with IsKey=true?
		What is TablesUsingRowVersionedDataCaching? Is that revision history or something else? Can we axe it?

		Why does backspace no longer delete the entire line regardless of whitespace? 
		Why does debug work when I start VS but not after? 
		Why does it always step in/pretend I have a backpoint set on the first line?
		 */
	}
}
