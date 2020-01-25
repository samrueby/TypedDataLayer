using System.Diagnostics;

namespace Tdl.Tester {
	class Program {
		static void Main( string[] args ) {
			// GMS NOTE: The floor effort for loading configuration in .net core is like a hundred lines of code spread across nine fucking files, so hardcoding here for now.
			Debug.WriteLine( "Hello World!" );
			CommandRunner.Program.LoadConfigAndRunUpdateAllDependentLogic( @"C:\GitHub\TypedDataLayer\TdlTester", true );
		}
	}
}