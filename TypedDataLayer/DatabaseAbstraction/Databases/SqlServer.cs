using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseAbstraction.Databases {
	[ UsedImplicitly ]
	public class SqlServer: Database {
		private readonly SqlServerInfo info;

		public SqlServer( SqlServerInfo info ) {
			this.info = info;
		}

		List<string> Database.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE Table_Type = 'Base Table'";
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


		public void ExecuteDbMethod( Action<DBConnection> method ) => executeDbMethodWithSpecifiedDatabaseInfo( info, method );


		private void executeDbMethodWithSpecifiedDatabaseInfo( SqlServerInfo info, Action<DBConnection> method ) => executeMethodWithDbExceptionHandling(
			() => {
				var connection =
					new DBConnection(
						new SqlServerInfo(
							( info as DatabaseInfo ).SecondaryDatabaseName,
							info.Server,
							info.LoginName,
							info.Password,
							info.Database,
							false,
							info.FullTextCatalog ) );
				connection.ExecuteWithConnectionOpen( () => method( connection ) );
			} );

		private void executeMethodWithDbExceptionHandling( Action method ) {
			try {
				method();
			}
			catch( DbConnectionFailureException e ) {
				throw new ApplicationException( "Failed to connect to SQL Server.", e );
			}
			catch( DbCommandTimeoutException e ) {
				throw new ApplicationException( "A SQL Server command timeout occurred.", e );
			}
		}
	}
}