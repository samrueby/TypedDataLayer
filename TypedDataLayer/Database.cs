using System;
using JetBrains.Annotations;
using TypedDataLayer.DataAccess;

namespace TypedDataLayer {
	/// <summary>
	/// Functions that can be used to access the database.
	/// </summary>
	public static class Database {
		/// <summary>
		/// Executes a query in the default isolation level.
		/// </summary>
		public static void ExecuteInDbConnection( [ InstantHandle ] Action method ) =>
			new DataAccessState().ExecuteWithThis( () => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen( method ) );

		/// <summary>
		/// Executes a query in the default isolation level.
		/// </summary>
		public static T ExecuteInDbConnection<T>( [ InstantHandle ] Func<T> method ) =>
			new DataAccessState().ExecuteWithThis( () => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen( method ) );

		/// <summary>
		/// Executes a query in Snapshot isolation.
		/// </summary>
		public static void ExecuteInDbConnectionWithTransaction( [ InstantHandle ] Action method ) =>
			new DataAccessState().ExecuteWithThis(
				() => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen(
					() => DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( method ) ) );

		/// <summary>
		/// Executes a query in Snapshot isolation.
		/// </summary>
		public static T ExecuteInDbConnectionWithTransaction<T>( [ InstantHandle ] Func<T> method ) =>
			new DataAccessState().ExecuteWithThis(
				() => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen(
					() => DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( method ) ) );

		/// <summary>
		/// Executes a query in Snapshot isolation with caching. Not safe to use when modifying data.
		/// </summary>
		/// <param name="method"></param>
		public static void ExecuteInDbConnectionWithTransactionWithCaching( [ InstantHandle ] Action method ) =>
			new DataAccessState().ExecuteWithThis(
				() => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen(
					() => DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => DataAccessState.Current.ExecuteWithCache( method ) ) ) );

		/// <summary>
		/// Executes a query in Snapshot isolation with caching. Not safe to use when modifying data.
		/// </summary>
		public static T ExecuteInDbConnectionWithTransactionCaching<T>( [ InstantHandle ] Func<T> method ) =>
			new DataAccessState().ExecuteWithThis(
				() => DataAccessState.Current.DatabaseConnection.ExecuteWithConnectionOpen(
					() => DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => DataAccessState.Current.ExecuteWithCache( method ) ) ) );
	}
}