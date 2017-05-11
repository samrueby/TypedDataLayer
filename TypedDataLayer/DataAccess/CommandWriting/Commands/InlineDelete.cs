using System;
using System.Collections.Generic;
using System.Text;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction;

namespace TypedDataLayer.DataAccess.CommandWriting.Commands {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class InlineDelete: InlineDbCommandWithConditions {
		private readonly string tableName;
		private readonly int? timeout;
		private readonly List<InlineDbCommandCondition> conditions = new List<InlineDbCommandCondition>();

		/// <summary>
		/// Creates a modification that will execute an inline DELETE statement.
		/// </summary>
		public InlineDelete( string tableName, int? timeout ) {
			this.tableName = tableName;
			this.timeout = timeout;
		}

		/// <summary>
		/// Use at your own risk.
		/// </summary>
		public void AddCondition( InlineDbCommandCondition condition ) => conditions.Add( condition );

		/// <summary>
		/// Executes this command against the specified database connection and returns the number of rows affected.
		/// </summary>
		public int Execute( DBConnection cn ) {
			if( conditions.Count == 0 )
				throw new ApplicationException( "Executing an inline delete command with no parameters in the where clause is not allowed." );
			var cmd = cn.DatabaseInfo.CreateCommand( timeout );

			var sb = new StringBuilder( "DELETE FROM " );
			sb.Append( tableName );
			sb.Append( " WHERE " );

			var paramNumber = 0;
			const string and = " AND ";

			foreach( var condition in conditions ) {
				condition.AddToCommand( cmd, sb, cn.DatabaseInfo, InlineUpdate.GetParamNameFromNumber( paramNumber++ ) );
				sb.Append( and );
			}
			sb.Remove( sb.Length - and.Length, and.Length );

			cmd.CommandText = sb.ToString();

			return cn.ExecuteNonQueryCommand( cmd );
		}
	}
}