 // ReSharper disable once CheckNamespace

namespace TypedDataLayer {
	partial class Database {
		public int? CommandTimeoutSecondsTyped => CommandTimeoutSecondsSpecified ? CommandTimeoutSeconds : (int?)null;
	}
}