using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdlTestFp {
	class Program {
		static void Main( string[] args ) {
			Debug.WriteLine( "Hello World!" );
			CommandRunner.Program.LoadConfigAndRunUpdateAllDependentLogic( @"C:\GitHub\TypedDataLayer\TdlTestFp", true );
		}
	}
}
