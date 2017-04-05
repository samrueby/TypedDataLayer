using System;

namespace TypedDataLayer.Exceptions {
	/// <summary>
	/// Thrown when less than 2 rows were expected.
	/// </summary>
	public class MoreThanOneRowException: Exception {
		/// <summary>
		/// Thrown when less than 2 rows were expected.
		/// </summary>
		public MoreThanOneRowException( string message ): base( message ) { }

		/// <summary>
		/// Thrown when less than 2 rows were expected.
		/// </summary>
		public MoreThanOneRowException( string message, Exception innerException ): base( message, innerException ) { }
	}
}