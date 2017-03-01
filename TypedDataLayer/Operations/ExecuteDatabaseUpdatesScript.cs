using System;
using System.IO;
using System.Linq;
using System.Text;
using TypedDataLayer.DatabaseAbstraction;
using TypedDataLayer.Exceptions;
using TypedDataLayer.Tools;

namespace TypedDataLayer.Operations {
	internal static class ExecuteDatabaseUpdatesScript {
		private const string databaseUpdatesFilesName = "Database Updates.sql";

		public static void Run( string projectFolder, Database databaseInfo, Logger log ) {
			var dbUpdatesPath = Path.Combine( projectFolder, databaseUpdatesFilesName );

			var lineNumber = getNumberOfLinesInDatabaseScript( dbUpdatesPath );
			if( lineNumber == null ) {
				log.Info( dbUpdatesPath + " was not found. No database updates will be performed." );
				return;
			}
			log.Debug( "Line number: " + lineNumber );

			int lineMarker;
			try {
				lineMarker = databaseInfo.GetLineMarker();
			}
			catch {
				log.Info( "Failed to get line marker from the database." );
				throw;
			}

			log.Debug( "Database Line Marker: " + lineMarker );

			if( lineNumber <= lineMarker ) {
				log.Debug( "Line number <= line marker. Nothing to execute." );
				return;
			}

			log.Debug( "Executing script." );
			try {
				databaseInfo.ExecuteSqlScriptInTransaction(
					File.ReadLines( dbUpdatesPath ).Skip( lineMarker ).Aggregate( new StringBuilder(), ( sb, line ) => sb.AppendLine( line ), sb => sb.ToString() ) );
			}
			catch( Exception e ) {
				throw new UserCorrectableException( "Failed to execute update script.", e );
			}

			// Technically there's a slight race condition here. The file could have gotten longer between us counting the
			// lines and executing the script.
			log.Debug( "Updating line marker." );
			databaseInfo.UpdateLineMarker( lineNumber.Value );
		}

		/// <summary>
		/// Returns null if no database script exists.
		/// </summary>
		private static int? getNumberOfLinesInDatabaseScript( string databaseUpdateFilePath ) {
			if( !File.Exists( databaseUpdateFilePath ) )
				return null;

			return File.ReadLines( databaseUpdateFilePath ).Count();
		}
	}
}