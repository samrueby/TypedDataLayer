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


		/*Replacing Tuple-based unique/primary key lookups with structs:
		 as an inner struct of StatesTableRetrieval:
			once per table/key: struct StateKey {
				public int StateId;
			}

		once per lookup/old tuple use.
		new StateKey{StateId=id }
			

		Actually put it in the Cache class:

		internal struct
		Cache.StateKey

		Where to try runtime retrieval of "keys" or columns with IsKey=true?
		What is TablesUsingRowVersionedDataCaching? Is that revision history or something else? Can we axe it?
		 */
	}
}
