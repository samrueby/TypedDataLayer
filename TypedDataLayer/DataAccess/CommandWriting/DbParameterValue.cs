using System;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.CommandWriting {
	/// <summary>
	/// A value used in a database command parameter.
	/// </summary>
	public class DbParameterValue: IEquatable<DbParameterValue>, IComparable, IComparable<DbParameterValue> {
		/// <summary>
		/// Creates a value with an unspecified type. This is not recommended since it forces the database type to be inferred from
		/// the .NET type of the value, and
		/// this process is imperfect and has lead to problems in the past with blobs.
		/// </summary>
		public DbParameterValue( object value ) => Value = value;

		/// <summary>
		/// Creates a value with the specified type.
		/// </summary>
		public DbParameterValue( object value, string dbTypeString ) {
			Value = value;
			DbTypeString = dbTypeString;
		}

		internal object Value { get; }

		internal string DbTypeString { get; }

		public override bool Equals( object obj ) => Equals( obj as DbParameterValue );

		public bool Equals( DbParameterValue other ) => other != null && Utility.AreEqual( Value, other.Value ) && DbTypeString == other.DbTypeString;

		public override int GetHashCode() => Value != null ? Value.GetHashCode() : -1;

		int IComparable.CompareTo( object obj ) {
			var otherCondition = obj as DbParameterValue;
			if( otherCondition == null && obj != null )
				throw new ArgumentException();
			return CompareTo( otherCondition );
		}

		public int CompareTo( DbParameterValue other ) {
			if( other == null )
				return 1;
			var valueResult = Utility.Compare( Value, other.Value );
			return valueResult != 0 ? valueResult : Utility.Compare( DbTypeString, other.DbTypeString, comparer: StringComparer.InvariantCulture );
		}
	}
}