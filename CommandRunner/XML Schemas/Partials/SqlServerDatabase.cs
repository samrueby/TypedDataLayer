using TypedDataLayer.DatabaseSpecification;

// ReSharper disable once CheckNamespace

namespace CommandRunner {
	partial class SqlServerDatabase {
		/// <summary>
		/// The database type.
		/// </summary>
		public override SupportedDatabaseType DatabaseType => SupportedDatabaseType.SqlServer;
	}
}