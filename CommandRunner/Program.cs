using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommandRunner.Exceptions;
using CommandRunner.Operations;
using CommandRunner.Tools;
using CommandRunner.XML_Schemas;
using TypedDataLayer;
using TypedDataLayer.Tools;

namespace CommandRunner {
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
				var filePath = Directory.EnumerateFiles( solutionPath, FileNames.ConfigurationFileName, SearchOption.AllDirectories ).First();
				// NOTE SJR: We can find a config file in each project and run it for that project.
				if( !File.Exists( filePath ) ) {
					log.Info( "Unable to find configuration file." );
					log.Info( $"Searched {solutionPath} for {FileNames.ConfigurationFileName} recursively." );
					return;
				}
				var projectFolder = getFirstFolder( filePath, solutionPath );

				var configuration = Utility.XmlDeserialize<SystemDevelopmentConfiguration>( filePath );

				GenerateDatabaseAccessLogic.Run( projectFolder, configuration, log );
			}
			catch( UserCorrectableException e ) {
				log.Info();
				log.Info( "An error occurred." );
				log.Info( e.ToFriendlyStack() );
				log.Debug( e.ToString() );
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

		private static string getFirstFolder( string filePath, string solutionPath ) {
			var relative = filePath.Replace( solutionPath, "" );
			var startIndex = relative.StartsWith( "\\" ) ? 1 : 0;
			return relative.Substring( startIndex, relative.IndexOf( '\\', startIndex ) - startIndex );
		}
	}
}