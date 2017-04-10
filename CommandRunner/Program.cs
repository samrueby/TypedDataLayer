using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommandRunner.Configuration;
using CommandRunner.Exceptions;
using CommandRunner.Operations;
using CommandRunner.Tools;
using TypedDataLayer.Tools;

namespace CommandRunner {
	class Program {
		static int Main( string[] args ) {
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

			log.Debug( "Executing directory: " + AppDomain.CurrentDomain.BaseDirectory );
			log.Debug( "Current working directory: " + Environment.CurrentDirectory );
			log.Debug( "Solution path: " + solutionPath );
			log.Debug( "Command: " + command );
			log.Info( "Running " + command );

			try {
				var configurationFiles = findConfigFiles( solutionPath );
				if( configurationFiles.Any() ) {
					foreach( var config in configurationFiles ) {
						log.Debug( "Found config file: " + config );
						var projectFolderName = getFirstFolder( config, solutionPath );
						var projectFolderPath = Path.Combine( solutionPath, projectFolderName );
						log.Debug( "Project folder path: " + projectFolderPath );

						log.Debug( "Deserializing config." );
						var configuration = Utility.XmlDeserialize<SystemDevelopmentConfiguration>( config );

						log.Debug( "Generating database access logic." );
						GenerateDatabaseAccessLogic.Run( projectFolderPath, configuration, log );
					}
				}
				else {
					log.Info( "Unable to find any configuration files." );
					log.Info( $"Searched {solutionPath} for {FileNames.ConfigurationFileName} recursively." );
				}
			}
			catch( UserCorrectableException e ) {
				log.Info();
				log.Info( "An error occurred." );
				log.Info( e.ToFriendlyStack() );
				log.Debug( e.ToString() );
				return 1;
			}
			catch( Exception e ) {
				log.Info();
				log.Info( "An error occurred." );
				log.Info( e.ToString() );
				return 2;
			}
			finally {
				log.Info( "Done." );
			}
			return 0;
		}

		private static string getFirstFolder( string filePath, string solutionPath ) {
			var relative = filePath.Replace( solutionPath, "" );
			var startIndex = relative.StartsWith( "\\" ) ? 1 : 0;
			return relative.Substring( startIndex, relative.IndexOf( '\\', startIndex ) - startIndex );
		}

		private static IEnumerable<string> findConfigFiles( string basePath ) {
			return
				Directory.EnumerateFiles( basePath, FileNames.ConfigurationFileName, SearchOption.AllDirectories )
					.Where( p => p.Contains( @"TypedDataLayer\" + FileNames.ConfigurationFileName ) );
		}
	}
}