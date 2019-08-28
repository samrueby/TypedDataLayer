using System;
using System.Data;
using System.Text;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class EqualityCondition: InlineDbCommandCondition {
		private readonly InlineDbCommandColumnValue columnValue;

		// IMPORTANT: If we implement Not Equals in this class, then it is extremely important that we modify the generated code to not use the value of the Not Equals condition
		// to initialize mod object data.

		/// <summary>
		/// Use at your own risk.
		/// </summary>
		public EqualityCondition( InlineDbCommandColumnValue columnValue ) => this.columnValue = columnValue;

		void InlineDbCommandCondition.AddToCommand( IDbCommand command, StringBuilder commandText, DatabaseInfo databaseInfo, string parameterName ) {
			var parameter = columnValue.GetParameter( name: parameterName );

			if( parameter.ValueIsNull ) {
				commandText.Append( columnValue.ColumnName );
				commandText.Append( " IS NULL" );
			}
			else {
				commandText.Append( columnValue.ColumnName );
				commandText.Append( " = " );
				commandText.Append( parameter.GetNameForCommandText( databaseInfo ) );
				command.Parameters.Add( parameter.GetAdoDotNetParameter( databaseInfo ) );
			}
		}

		public override bool Equals( object obj ) => Equals( obj as InlineDbCommandCondition );

		public bool Equals( InlineDbCommandCondition other ) =>
			other is EqualityCondition otherEqualityCondition && Utility.AreEqual( columnValue, otherEqualityCondition.columnValue );

		public override int GetHashCode() => columnValue.GetHashCode();

		int IComparable.CompareTo( object obj ) {
			var otherCondition = obj as InlineDbCommandCondition;
			if( otherCondition == null && obj != null )
				throw new ArgumentException();
			return CompareTo( otherCondition );
		}

		public int CompareTo( InlineDbCommandCondition other ) {
			if( other == null )
				return 1;
			if( !( other is EqualityCondition otherEqualityCondition ) )
				return DataAccessMethods.CompareCommandConditionTypes( this, other );

			return Utility.Compare( columnValue, otherEqualityCondition.columnValue );
		}
	}
}