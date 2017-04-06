// ReSharper disable once CheckNamespace

namespace CommandRunner {
	partial class Database {
		/// <summary>
		/// The <see cref="Database.CommandTimeoutSeconds"/> or null if it wasn't specified. 
		/// </summary>
		public int? CommandTimeoutSecondsTyped => CommandTimeoutSecondsSpecified ? CommandTimeoutSeconds : (int?)null;
	}
}