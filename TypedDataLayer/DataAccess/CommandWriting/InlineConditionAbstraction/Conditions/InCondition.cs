using System;
using System.Data;
using System.Text;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class InCondition: InlineDbCommandCondition {
		private readonly string columnName;
		private readonly string subQuery;

		/// <summary>
		/// Use at your own risk. Nothing in the sub-query is escaped, so do not base any part of it on user input.
		/// </summary>
		public InCondition( string columnName, string subQuery ) {
			this.columnName = columnName;
			this.subQuery = subQuery;
		}

		void InlineDbCommandCondition.AddToCommand( IDbCommand command, StringBuilder commandText, DatabaseInfo databaseInfo, string parameterName ) {
			commandText.Append( columnName );
			commandText.Append( " IN ( " );
			commandText.Append( subQuery );
			commandText.Append( " )" );
		}

		public override bool Equals( object obj ) => Equals( obj as InlineDbCommandCondition );

		public bool Equals( InlineDbCommandCondition other ) {
			var otherInCondition = other as InCondition;
			return otherInCondition != null && columnName == otherInCondition.columnName && subQuery == otherInCondition.subQuery;
		}

		public override int GetHashCode() => new { columnName, subQuery }.GetHashCode();

		int IComparable.CompareTo( object obj ) {
			var otherCondition = obj as InlineDbCommandCondition;
			if( otherCondition == null && obj != null )
				throw new ArgumentException();
			return CompareTo( otherCondition );
		}

		public int CompareTo( InlineDbCommandCondition other ) {
			if( other == null )
				return 1;
			var otherInCondition = other as InCondition;
			if( otherInCondition == null )
				return DataAccessMethods.CompareCommandConditionTypes( this, other );

			var columnNameResult = Utility.Compare( columnName, otherInCondition.columnName, comparer: StringComparer.InvariantCulture );
			return columnNameResult != 0 ? columnNameResult : Utility.Compare( subQuery, otherInCondition.subQuery, comparer: StringComparer.InvariantCulture );
		}
	}
}