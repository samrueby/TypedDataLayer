using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using TypedDataLayer.DatabaseSpecification;

namespace TypedDataLayer.DataAccess.CommandWriting.Commands {
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public class SprocExecution {
		private readonly string sproc;
		private readonly int? timeout;
		private readonly List<DbCommandParameter> parameters = new List<DbCommandParameter>();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public SprocExecution( string sproc, int? timeout ) {
			this.sproc = sproc;
			this.timeout = timeout;
		}

		/// <summary>
		/// Adds the specified parameter to this command.
		/// </summary>
		public void AddParameter( DbCommandParameter parameter ) => parameters.Add( parameter );

		/// <summary>
		/// Executes this procedure against the specified database connection to get a data reader and then executes the specified method with the reader.
		/// </summary>
		public void ExecuteReader( DBConnection cn, Action<DbDataReader> readerMethod ) {
			var cmd = getCommand( cn );
			setupDbCommand( cmd, cn.DatabaseInfo );
			cn.ExecuteReaderCommand( cmd, readerMethod );
		}

		/// <summary>
		/// Executes this sproc against the specified database connection and returns the number of rows affected.
		/// </summary>
		public int ExecuteNonQuery( DBConnection cn ) {
			var cmd = getCommand( cn );
			setupDbCommand( cmd, cn.DatabaseInfo );

			return cn.ExecuteNonQueryCommand( cmd );
		}

		/// <summary>
		/// Executes this sproc against the specified database connection and returns a single value.
		/// </summary>
		public object ExecuteScalar( DBConnection cn ) {
			var cmd = getCommand( cn );
			setupDbCommand( cmd, cn.DatabaseInfo );

			return cn.ExecuteScalarCommand( cmd );
		}

		private DbCommand getCommand( DBConnection cn ) {
			var cmd = cn.DatabaseInfo.CreateCommand();
			if( timeout.HasValue )
				cmd.CommandTimeout = timeout.Value;
			return cmd;
		}

		private void setupDbCommand( DbCommand dbCmd, DatabaseInfo databaseInfo ) {
			dbCmd.CommandText = sproc;
			dbCmd.CommandType = CommandType.StoredProcedure;
			foreach( var p in parameters )
				dbCmd.Parameters.Add( p.GetAdoDotNetParameter( databaseInfo ) );
		}
	}
}