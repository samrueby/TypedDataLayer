using System;
using System.Collections.Generic;
using CommandRunner.Tools;
using JetBrains.Annotations;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.DatabaseAbstraction.Databases {
	[ UsedImplicitly ]
	public class SqlServer: Database {
		private readonly SqlServerInfo info;

		public SqlServer( SqlServerInfo info ) {
			this.info = info;
		}

		void Database.ExecuteSqlScriptInTransaction( string script ) {
			executeMethodWithDbExceptionHandling(
				() => {
					try {
						Utility.RunProgram(
							"sqlcmd",
							$"{( info.Server != null ? $"-S {info.Server} " : "" )}-d {info.Database} -e -b",
							$"BEGIN TRAN{Environment.NewLine}GO{Environment.NewLine}{script}COMMIT TRAN{Environment.NewLine}GO{Environment.NewLine}EXIT{Environment.NewLine}",
							true );
					}
					catch( Exception e ) {
						throw DataAccessMethods.CreateDbConnectionException( info, "updating logic in", e );
					}
				} );
		}

		int Database.GetLineMarker() {
			var value = 0;
			ExecuteDbMethod(
				cn => {
					var cmd = cn.DatabaseInfo.CreateCommand();
					cmd.CommandText = "SELECT ParameterValue FROM GlobalInts WHERE ParameterName = 'LineMarker'";
					value = (int)cn.ExecuteScalarCommand( cmd );
				} );
			return value;
		}

		void Database.UpdateLineMarker( int value ) {
			ExecuteDbMethod(
				cn => {
					var command = new InlineUpdate( "GlobalInts" );
					command.AddColumnModification( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value ) ) );
					command.AddCondition( new EqualityCondition( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( "LineMarker" ) ) ) );
					command.Execute( cn );
				} );
		}

		List<string> Database.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				cn => {
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