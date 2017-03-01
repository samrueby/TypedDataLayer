using System;
using System.Collections.Generic;
using System.IO;
using CommandRunner.Tools;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.DatabaseAbstraction.Databases {
	public class MySql: Database {
		private const string binFolderPath = @"C:\Program Files\MySQL\MySQL Server 5.5\bin";

		private readonly MySqlInfo info;

		public MySql( MySqlInfo info ) {
			this.info = info;
		}

		void Database.ExecuteSqlScriptInTransaction( string script ) {
			using( var sw = new StringWriter() ) {
				sw.WriteLine( "START TRANSACTION;" );
				sw.Write( script );
				sw.WriteLine( "COMMIT;" );
				sw.WriteLine( "quit" );

				executeMethodWithDbExceptionHandling(
					() => {
						try {
							Utility.RunProgram(
								Utility.CombinePaths( binFolderPath, "mysql" ),
								$"--host=localhost --user=root --password=password {info.Database} --disable-reconnect --batch --disable-auto-rehash",
								sw.ToString(),
								true );
						}
						catch( Exception e ) {
							throw DataAccessMethods.CreateDbConnectionException( info, "updating logic in", e );
						}
					} );
			}
		}

		int Database.GetLineMarker() {
			var value = 0;
			ExecuteDbMethod(
				cn => {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText = "SELECT ParameterValue FROM global_ints WHERE ParameterName = 'LineMarker'";
					value = (int)cn.ExecuteScalarCommand( command );
				} );
			return value;
		}

		void Database.UpdateLineMarker( int value ) {
			ExecuteDbMethod(
				cn => {
					var command = new InlineUpdate( "global_ints" );
					command.AddColumnModification( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value ) ) );
					command.AddCondition( new EqualityCondition( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( "LineMarker" ) ) ) );
					command.Execute( cn );
				} );
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