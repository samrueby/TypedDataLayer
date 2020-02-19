﻿using System;
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
	public class Program {
		/// <summary>
		/// Loads config and always runs UpdateDependentLogic.
		/// </summary>
		public static int Main( string[] args ) {
			var debug = args.Any( a => a == "-debug" );
			var log = new Logger( debug );

			if( args.Any( a => a == "-attachDebugger" ) ) {
				log.Info( "Waiting 15s for debugger" );
				var stop = Stopwatch.StartNew();
				while( stop.Elapsed.TotalSeconds < 15 && !Debugger.IsAttached )
					Thread.Sleep( 250 );
			}

			log.Debug( "TypedDataLayer version " + Assembly.GetExecutingAssembly().GetName().Version );
			log.Debug( "args: " + string.Join( " ", args ) );

			var workingDirectory = Environment.CurrentDirectory;
			return LoadConfigAndRunUpdateAllDependentLogic( workingDirectory, debug );
		}

		public static int LoadConfigAndRunUpdateAllDependentLogic( string workingDirectory, bool debug ) {
			var log = new Logger( debug );
			log.Debug( "Executing directory: " + AppDomain.CurrentDomain.BaseDirectory );
			log.Info( "Current working directory: " + workingDirectory );

			var sw = new Stopwatch();
			try {
				sw.Start();
				var configurationFiles = findConfigFiles( workingDirectory );
				if( configurationFiles.Any() ) {
					foreach( var config in configurationFiles ) {
						log.Debug( "Found config file: " + config );
						var projectFolderName = getFirstFolder( config, workingDirectory );
						var projectFolderPath = Path.Combine( workingDirectory, projectFolderName );
						log.Debug( "Project folder path: " + projectFolderPath );

						log.Debug( "Deserializing config." );
						var configuration = Utility.XmlDeserialize<SystemDevelopmentConfiguration>( config );

						log.Debug( "Generating database access logic." );
						GenerateDatabaseAccessLogic.Run( projectFolderPath, configuration, log );
					}
				}
				else {
					log.Info( "Unable to find any configuration files." );
					log.Info( $"Searched {workingDirectory} for {FileNames.ConfigurationFileName} recursively." );
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
				log.Info( $"Done in {sw.Elapsed.TotalSeconds:G0} seconds." );
			}

			return 0;
		}

		private static string getFirstFolder( string filePath, string solutionPath ) {
			var relative = filePath.Replace( solutionPath, "" );
			var startIndex = relative.StartsWith( "\\" ) ? 1 : 0;
			return relative.Substring( startIndex, relative.IndexOf( '\\', startIndex ) - startIndex );
		}

		private static IEnumerable<string> findConfigFiles( string basePath ) {
			return Directory.EnumerateFiles( basePath, FileNames.ConfigurationFileName, SearchOption.AllDirectories )
				.Where( p => p.Contains( @"TypedDataLayer\" + FileNames.ConfigurationFileName ) );
		}
	}
}