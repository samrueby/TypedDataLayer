using TypedDataLayer.DatabaseSpecification;

// ReSharper disable once CheckNamespace

namespace CommandRunner {
	partial class MySqlDatabase {
		/// <summary>
		/// The database type.
		/// </summary>
		public override SupportedDatabaseType DatabaseType => SupportedDatabaseType.MySql;
	}
}