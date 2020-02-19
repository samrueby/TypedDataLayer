using System;
using System.Diagnostics;
using System.Linq;
using Tdl.TestGeneration.DataAccess.CommandConditions.dbo;
using Tdl.TestGeneration.DataAccess.Modification.dbo;
using Tdl.TestGeneration.DataAccess.TableRetrieval.dbo;
using TypedDataLayer;

namespace Tdl.TestDml {
	class Program {
		static void Main() {
			assertTransactionsRollbackOnException();
			assertUpdatesAffectOnlySpecifiedRows();
			assertCachePkLookupsWork();
		}

		private static void assertTransactionsRollbackOnException() {
			try {
				Database.ExecuteInDbConnectionWithTransaction(
					() => {
						assertStateHasName( "NY", "New York" );
						var stateMod = StatesModification.CreateForUpdate( new StatesTableEqualityConditions.Abbreviation( "NY" ) );
						stateMod.StateName = "ShouldBeRolledBack";
						stateMod.Execute();
						assertStateHasName( "NY", "ShouldBeRolledBack" );
						throw new ApplicationException( "Simulated unexpected error thrown by developer code inside a transaction." );
					} );
			}
			catch { }

			Database.ExecuteInDbConnectionWithTransaction( () => { assertStateHasName( "NY", "New York" ); } );
		}

		private static void assertUpdatesAffectOnlySpecifiedRows() {
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

		private static void assertCachePkLookupsWork() {
			Database.ExecuteInDbConnectionWithTransactionWithCaching(
				() => {
					var ny = StatesTableRetrieval.GetRowMatchingId( 4000 );
					var nyCached = StatesTableRetrieval.GetRowMatchingId( 4000 );
					// GMS NOTE: Not sure what we can really assert here - we can't tell if it really went to the database. Pretty much had to debug it.
				} );
		}
	}
}