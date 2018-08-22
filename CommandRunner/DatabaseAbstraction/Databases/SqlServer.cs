using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace CommandRunner.DatabaseAbstraction.Databases {
	[ UsedImplicitly ]
	public class SqlServer: IDatabase {
		private readonly SqlServerInfo info;

		public SqlServer( SqlServerInfo info ) {
			this.info = info;
		}

		void IDatabase.ExecuteSqlScriptInTransaction( string script ) {
			executeMethodWithDbExceptionHandling(
				() => {
					try {
						runDatabaseScript( info.ConnectionString, script );
					}
					catch( Exception e ) {
						throw DataAccessMethods.CreateDbConnectionException( info, "updating logic in", e );
					}
				} );
		}

		private static void runDatabaseScript( string connectionString, string script ) {
			using( var sqlConnection = new SqlConnection( connectionString ) ) {
				var svrConnection = new ServerConnection( sqlConnection );
				var server = new Server( svrConnection );
				server.ConnectionContext.ExecuteNonQuery( script );
			}
		}

		int IDatabase.GetLineMarker() {
			var value = 0;
			ExecuteDbMethod(
				cn => {
					var cmd = cn.DatabaseInfo.CreateCommand( null );
					cmd.CommandText = "SELECT ParameterValue FROM GlobalInts WHERE ParameterName = 'LineMarker'";
					value = (int)cn.ExecuteScalarCommand( cmd );
				} );
			return value;
		}

		void IDatabase.UpdateLineMarker( int value ) {
			ExecuteDbMethod(
				cn => {
					var command = new InlineUpdate( "GlobalInts", null );
					command.AddColumnModification( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value ) ) );
					command.AddCondition( new EqualityCondition( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( "LineMarker" ) ) ) );
					command.Execute( cn );
				} );
		}

		List<string> IDatabase.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				cn => {
					var command = cn.DatabaseInfo.CreateCommand( null );
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

		List<string> IDatabase.GetProcedures() {
			throw new NotSupportedException();
		}

		List<ProcedureParameter> IDatabase.GetProcedureParameters( string procedure ) {
			throw new NotSupportedException();
		}


		public void ExecuteDbMethod( Action<DBConnection> method ) => executeDbMethodWithSpecifiedDatabaseInfo( info, method );


		private void executeDbMethodWithSpecifiedDatabaseInfo( SqlServerInfo info, Action<DBConnection> method ) =>
			executeMethodWithDbExceptionHandling(
				() => {
					var connection = new DBConnection( new SqlServerInfo( info.ConnectionString ) );
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