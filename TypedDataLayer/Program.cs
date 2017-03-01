using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TypedDataLayer.Operations;
using TypedDataLayer.Tools;

namespace TypedDataLayer {
	class Program {
		static void Main( string[] args ) {
			// NOTE SJR: What are we using FullTextCatalog for?
			// NOTE SJR: Use a connection string instead of <Server> element w/ credentials?
			var log = new Logger( args.Any( a => a == "-debug" ) );

			if( args.Any( a => a == "-attachDebugger" ) ) {
				log.Info( "Waiting 15s for debugger" );
				var stop = Stopwatch.StartNew();
				while( stop.Elapsed.TotalSeconds < 15 && !Debugger.IsAttached )
					Thread.Sleep( 250 );
			}

			log.Debug( "TypedDataLayer version " + Assembly.GetExecutingAssembly().GetName().Version );

			log.Debug( "args: " + string.Join( " ", args ) );

			var solutionPath = Path.GetFullPath( args[ 0 ] );
			var command = args[ 1 ];

			log.Debug( "Solution path: " + solutionPath );
			log.Debug( "Command: " + command );

			log.Info( "Running " + command );

			try {
				if( GenerateDatabaseAccessLogic.Run( solutionPath, log ) )
					return;
			}
			catch( Exception e ) {
				log.Info();
				log.Info( "An error occurred." );
				log.Info( e.ToString() );
			}
			finally {
				log.Info( "Done." );
			}
		}
	}
}