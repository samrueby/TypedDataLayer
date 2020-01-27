using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace TestTdlGeneration {
	class Program {
		static void Main( string[] args ) {
			Debug.WriteLine( "Hello World!" );
			//var workingDirectory = ConfigurationManager.AppSettings[ "token" ];
			CommandRunner.Program.LoadConfigAndRunUpdateAllDependentLogic( @"C:\GitHub\TypedDataLayer\TestTdlGeneration", true );
		}
	}
}
