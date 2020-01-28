using System.Diagnostics;
using System.Linq;
using Tdl.TestGeneration.DataAccess.CommandConditions.dbo;
using Tdl.TestGeneration.DataAccess.Modification.dbo;
using Tdl.TestGeneration.DataAccess.TableRetrieval.dbo;
using TypedDataLayer;

namespace Tdl.TestDml {
	class Program {
		static void Main( string[] args ) {
			Database.ExecuteInDbConnection(
				() => {
					assertStateHasName( "NY", "New York" );
					assertStateHasName( "AZ", "Arizona" );

					var stateMod = StatesModification.CreateForUpdate( new StatesTableEqualityConditions.Abbreviation( "NY" ) );
					stateMod.StateName = "Modified";
					stateMod.Execute();

					assertStateHasName( "NY", "Modified" );
					assertStateHasName( "AZ", "Arizona" );

					stateMod.StateName = "New York";
					stateMod.Execute();
					assertStateHasName( "NY", "New York" );
				} );
		}

		private static void assertStateHasName( string stateAbbreviation, string stateName ) {
			var state = StatesTableRetrieval.GetRowsMatchingConditions( new StatesTableEqualityConditions.Abbreviation( stateAbbreviation ) ).Single();
			Debug.Assert( stateName == state.StateName );
		}
	}
}