using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace TypedDataLayer.DatabaseAbstraction.Databases {
	[ UsedImplicitly ]
	public class Oracle: Database {
		private static readonly string dataPumpFolderPath = null;

		private readonly OracleInfo info;

		internal Oracle( OracleInfo info ) {
			this.info = info;
		}
		
		List<string> Database.GetTables() {
			var tables = new List<string>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
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

		List<string> Database.GetProcedures() {
			var procedures = new List<string>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
					foreach( DataRow row in cn.GetSchema( "Procedures", info.UserAndSchema ).Rows )
						procedures.Add( (string)row[ "OBJECT_NAME" ] );
				} );
			return procedures;
		}

		List<ProcedureParameter> Database.GetProcedureParameters( string procedure ) {
			var parameters = new List<ProcedureParameter>();
			ExecuteDbMethod(
				delegate( DBConnection cn ) {
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
			if( direction == "IN" )
				return ParameterDirection.Input;
			if( direction == "OUT" )
				return ParameterDirection.Output;
			if( direction == "IN/OUT" )
				return ParameterDirection.InputOutput;
			throw new ApplicationException( "Unknown parameter direction string." );
		}


		public void ExecuteDbMethod( Action<DBConnection> method ) => executeDbMethodWithSpecifiedDatabaseInfo( info, method );

		private void executeDbMethodWithSpecifiedDatabaseInfo( OracleInfo info, Action<DBConnection> method ) {
			executeMethodWithDbExceptionHandling(
				() => {
					// Before we disabled pooling, we couldn't repeatedly perform Update Data operations since users with open connections can't be dropped.
					var connection =
						new DBConnection(
							new OracleInfo(
								( info as DatabaseInfo ).SecondaryDatabaseName,
								info.DataSource,
								info.UserAndSchema,
								info.Password,
								false,
								info.SupportsLinguisticIndexes ) );

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