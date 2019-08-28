using System;

namespace TypedDataLayer.Exceptions {
	/// <summary>
	/// Thrown when an ID should have returned a row, but instead returned none.
	/// </summary>
	public class InvalidPrimaryKeyException: Exception {
		/// <summary>
		/// The pk from the statement.
		/// </summary>
		public readonly string PrimaryKey;

		public InvalidPrimaryKeyException( string message ): base( message ) { }

		/// <summary>
		/// Thrown when an ID should have returned a row, but instead returned none.
		/// </summary>
		public InvalidPrimaryKeyException( string primaryKey, Exception innerException ): base( $"The primary key '{primaryKey}' did not return any results.", innerException ) =>
			PrimaryKey = primaryKey;
	}
}