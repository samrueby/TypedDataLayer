using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypedDataLayer.DataAccess.CommandWriting.Commands {
	/// <summary>
	/// Allows simple inserting of rows into a table without the use of any stored procedures.
	/// </summary>
	public class InlineInsert: InlineDbModificationCommand {
		private readonly string table;
		private readonly bool tableIsAutoIncrement;
		private readonly int? timeout;
		private readonly List<InlineDbCommandColumnValue> columnModifications = new List<InlineDbCommandColumnValue>();

		/// <summary>
		/// Create a command to insert a row in the given table.
		/// </summary>
		public InlineInsert( string table, bool tableIsAutoIncrement, int? timeout ) {
			this.table = table;
			this.tableIsAutoIncrement = tableIsAutoIncrement;
			this.timeout = timeout;
		}

		/// <summary>
		/// Add a data parameter to the command. Value may be null.
		/// </summary>
		public void AddColumnModification( InlineDbCommandColumnValue columnModification ) => columnModifications.Add( columnModification );

		/// <summary>
		/// Executes this command against the specified database connection and returns the auto-increment value of the inserted row, or null if it is not an
		/// auto-increment table.
		/// </summary>
		public object Execute( DBConnection cn ) {
			var cmd = cn.DatabaseInfo.CreateCommand( timeout );
			var sb = new StringBuilder( "INSERT INTO " );
			sb.Append( table );
			if( columnModifications.Count == 0 ) {
				sb.Append( " DEFAULT VALUES" );
			}
			else {
				sb.Append( "(" );
				foreach( var columnMod in columnModifications ) {
					sb.Append( columnMod.ColumnName );
					sb.Append( ", " );
				}
				sb.Remove( sb.Length - 2, 2 );

				sb.Append( ") VALUES (" );
				foreach( var columnMod in columnModifications ) {
					var parameter = columnMod.GetParameter();
					sb.Append( parameter.GetNameForCommandText( cn.DatabaseInfo ) );
					sb.Append( "," );
					cmd.Parameters.Add( parameter.GetAdoDotNetParameter( cn.DatabaseInfo ) );
				}
				sb.Remove( sb.Length - 1, 1 );
				sb.Append( ")" );
			}
			cmd.CommandText = sb.ToString();
			if( timeout.HasValue )
				cmd.CommandTimeout = timeout.Value;
			cn.ExecuteNonQueryCommand( cmd );

			if( !tableIsAutoIncrement || !cn.DatabaseInfo.LastAutoIncrementValueExpression.Any() )
				return null;

			var autoIncrementRetriever = cn.DatabaseInfo.CreateCommand( timeout );
			autoIncrementRetriever.CommandText = "SELECT " + cn.DatabaseInfo.LastAutoIncrementValueExpression;

			var autoIncrementValue = cn.ExecuteScalarCommand( autoIncrementRetriever );
			return autoIncrementValue != DBNull.Value ? autoIncrementValue : null;
		}
	}
}