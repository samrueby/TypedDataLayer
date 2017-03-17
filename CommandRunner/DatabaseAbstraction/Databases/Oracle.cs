using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using JetBrains.Annotations;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.DatabaseAbstraction.Databases {
	[ UsedImplicitly ]
	public class Oracle: IDatabase {
		private readonly OracleInfo info;
		private readonly OracleConnectionStringBuilder connectionString;

		internal Oracle( OracleInfo info ) {
			this.info = info;
			connectionString = new OracleConnectionStringBuilder( info.ConnectionString );
		}

		void IDatabase.ExecuteSqlScriptInTransaction( string script ) {
			using( var sw = new StringWriter() ) {
				// Carriage returns seem to be significant here.
				sw.WriteLine( "WHENEVER SQLERROR EXIT SQL.SQLCODE;" );
				sw.Write( script );
				sw.WriteLine( "EXIT SUCCESS COMMIT;" );

				executeMethodWithDbExceptionHandling(
					() => {
						try {
							// -L option stops it from prompting on failed logon.
							Utility.RunProgram( "sqlplus", "-L " + getLogonString(), sw.ToString(), true );
						}
						catch( Exception e ) {
							throw DataAccessMethods.CreateDbConnectionException( info, "updating logic in", e );
						}
					} );
			}
		}

		int IDatabase.GetLineMarker() {
			var value = 0;
			ExecuteDbMethod(
				cn => {
					var cmd = cn.DatabaseInfo.CreateCommand();
					cmd.CommandText = "SELECT v FROM global_numbers WHERE k = 'LineMarker'";
					value = Convert.ToInt32( cn.ExecuteScalarCommand( cmd ) );
				} );
			return value;
		}

		void IDatabase.UpdateLineMarker( int value ) {
			ExecuteDbMethod(
				cn => {
					var command = new InlineUpdate( "global_numbers", null );
					command.AddColumnModification( new InlineDbCommandColumnValue( "v", new DbParameterValue( value ) ) );
					command.AddCondition( new EqualityCondition( new InlineDbCommandColumnValue( "k", new DbParameterValue( "LineMarker" ) ) ) );
					command.Execute( cn );
				} );
		}

		List<string> IDatabase.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				cn => {
					var command = cn.DatabaseInfo.CreateCommand();
					command.CommandText = "SELECT table_name FROM user_tables";
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
			var procedures = new List<string>();
			ExecuteDbMethod(
				cn => {
					foreach( DataRow row in cn.GetSchema( "Procedures", connectionString.UserID ).Rows )
						procedures.Add( (string)row[ "OBJECT_NAME" ] );
				} );
			return procedures;
		}

		List<ProcedureParameter> IDatabase.GetProcedureParameters( string procedure ) {
			var parameters = new List<ProcedureParameter>();
			ExecuteDbMethod(
				cn => {
					var rows = new List<DataRow>();
					foreach( DataRow row in cn.GetSchema( "ProcedureParameters", null, procedure ).Rows )
						rows.Add( row );
					rows.Sort( ( x, y ) => (int)( (decimal)x[ "POSITION" ] - (decimal)y[ "POSITION" ] ) );
					foreach( var row in rows ) {
						var dataType = (string)row[ "DATA_TYPE" ];
						var parameterDirection = getParameterDirection( (string)row[ "IN_OUT" ] );

						// The parameters returned by this method are used with an OracleCommand object that will be executed using
						// ExecuteReader. Per the Oracle Data Provider for .NET documentation for OracleCommand.ExecuteReader, output
						// REF CURSOR parameters in a procedure can be accessed through the returned data reader and don't need to be
						// treated as ordinary command parameters. That's why we don't include them here.
						if( dataType != "REF CURSOR" || parameterDirection != ParameterDirection.Output )
							parameters.Add( new ProcedureParameter( cn, (string)row[ "ARGUMENT_NAME" ], dataType, (int)row[ "DATA_LENGTH" ], parameterDirection ) );
					}
				} );
			return parameters;
		}

		private ParameterDirection getParameterDirection( string direction ) {
			switch( direction ) {
				case "IN":
					return ParameterDirection.Input;
				case "OUT":
					return ParameterDirection.Output;
				case "IN/OUT":
					return ParameterDirection.InputOutput;
			}
			throw new ApplicationException( "Unknown parameter direction string." );
		}

		private string getLogonString() => connectionString.UserID + "/" + connectionString.Password + "@" + connectionString.DataSource;

		public void ExecuteDbMethod( Action<DBConnection> method ) => executeDbMethodWithSpecifiedDatabaseInfo( info, method );

		private void executeDbMethodWithSpecifiedDatabaseInfo( OracleInfo info, Action<DBConnection> method ) {
			executeMethodWithDbExceptionHandling(
				() => {
					var connection = new DBConnection( new OracleInfo( info.ConnectionString, info.SupportsLinguisticIndexes ) );

					connection.ExecuteWithConnectionOpen( () => method( connection ) );
				} );
		}

		private void executeMethodWithDbExceptionHandling( Action method ) {
			try {
				method();
			}
			catch( DbConnectionFailureException e ) {
				throw new ApplicationException( "Failed to connect to Oracle.", e );
			}
		}
	}
}