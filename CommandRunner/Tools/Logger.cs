using System;

namespace CommandRunner.Tools {
	internal class Logger {
		private readonly bool debug;

		public Logger( bool debug ) {
			this.debug = debug;
		}

		/// <summary>
		/// Writes to the log regardless.
		/// </summary>
		public void Info( string s ) => log( s );

		public void Info() => Info( "" );

		/// <summary>
		/// Writes to the log if we're in Debug mode.
		/// </summary>
		public void Debug( string s ) {
			if( debug ) {
				log( s );
			}
		}

		private static void log( string s ) => Console.WriteLine( DateTime.Now + ": " +s );
	}
}