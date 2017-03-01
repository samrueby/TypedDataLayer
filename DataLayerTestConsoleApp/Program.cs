using System;
using System.Linq;
using ExampleNamespace.DataAccess.TableRetrieval;
using TypedDataLayer.DataAccess;

namespace DataLayerTestConsoleApp {
	class Program {
		static void Main( string[] args ) {
			ExecuteInDbConnection(
				() => {
					var allUSers = UsersTableRetrieval.GetRows();
					Console.WriteLine( allUSers.Count() );
				} );
		}

		public static void ExecuteInDbConnection( Action method )
			=> new DataAccessState().ExecuteWithThis( () => DataAccessState.Current.PrimaryDatabaseConnection.ExecuteWithConnectionOpen( method ) );
	}
}