using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.CommandWriting.Commands {
	/// <summary>
	/// A SELECT query that can be executed against a database.
	/// </summary>
	public class InlineSelect: InlineDbCommandWithConditions {
		private readonly IEnumerable<string> selectExpressions;
		private readonly string fromClause;
		private readonly string orderByClause;
		private readonly bool cacheQueryInDatabase;
		private readonly List<InlineDbCommandCondition> conditions = new List<InlineDbCommandCondition>();
		private readonly int? timeout;

		/// <summary>
		/// Creates a new inline SELECT command.
		/// </summary>
		public InlineSelect( IEnumerable<string> selectExpressions, string fromClause, bool cacheQueryInDatabase, int? timeout, string orderByClause = "" ) {
			this.selectExpressions = selectExpressions;
			this.fromClause = fromClause;
			this.orderByClause = orderByClause;
			this.cacheQueryInDatabase = cacheQueryInDatabase;
			this.timeout = timeout;
		}

		/// <summary>
		/// Adds a condition to the command.
		/// </summary>
		public void AddCondition( InlineDbCommandCondition condition ) => conditions.Add( condition );

		/// <summary>
		/// Executes this command using the specified database connection to get a data reader and then executes the specified method with the reader.
		/// </summary>
		public void Execute( DBConnection cn, Action<DbDataReader> readerMethod ) {
			var command = cn.DatabaseInfo.CreateCommand();

			var sb = new StringBuilder( "SELECT" );
			if( cacheQueryInDatabase && cn.DatabaseInfo.QueryCacheHint.Any() ) {
				sb.Append( " " );
				sb.Append( cn.DatabaseInfo.QueryCacheHint );
			}
			sb.Append( " " );
			sb.Append( StringTools.ConcatenateWithDelimiter( ", ", selectExpressions ) );
			sb.Append( fromClause );

			if( conditions.Any() ) {
				sb.Append( " WHERE " );

				var first = true;
				var paramNumber = 0;
				foreach( var condition in conditions ) {
					if( !first )
						sb.Append( " AND " );
					first = false;
					condition.AddToCommand( command, sb, cn.DatabaseInfo, InlineUpdate.GetParamNameFromNumber( paramNumber++ ) );
				}
			}

			if( orderByClause != "" ) {
				sb.Append( " " );
				sb.Append( orderByClause );
			}

			command.CommandText = sb.ToString();
			if( timeout.HasValue ) {
				command.CommandTimeout = timeout.Value;
			}

			cn.ExecuteReaderCommand( command, readerMethod );
		}
	}
}