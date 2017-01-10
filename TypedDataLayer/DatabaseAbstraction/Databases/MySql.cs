using System;
using System.Collections.Generic;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseAbstraction.Databases {
	public class MySql: Database {
		private readonly MySqlInfo info;

		public MySql( MySqlInfo info ) {
			this.info = info;
		}
		
		List<string> Database.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_TYPE = 'BASE TABLE'".FormatWith(
						info.Database );
					cn.ExecuteReaderCommand(
						command,
						reader => {
							while( reader.Read() )
								tables.Add( reader.GetString( 0 ) );
						} );
				} );
			return tables;
		}

		List<string> Database.GetProcedures() {
			throw new NotSupportedException();
		}

		List<ProcedureParameter> Database.GetProcedureParameters( string procedure ) {
			throw new NotSupportedException();
		}

		public void ExecuteDbMethod( Action<DBConnection> method ) => executeMethodWithDbExceptionHandling(
			() => {
				var connection = new DBConnection( new MySqlInfo( ( info as DatabaseInfo ).SecondaryDatabaseName, info.Database, false ) );
				connection.ExecuteWithConnectionOpen( () => method( connection ) );
			} );

		private void executeMethodWithDbExceptionHandling( Action method ) {
			try {
				method();
			}
			catch( DbConnectionFailureException e ) {
				throw new ApplicationException( "Failed to connect to MySQL.", e );
			}
		}
	}
}