using System;
using System.IO;
using System.Threading;

namespace CommandRunner.Tools {
	/// <summary>
	/// A collection of IO-related static methods.
	/// </summary>
	public static class IoMethods {
		/// <summary>
		/// Deletes the file at the given path, or does nothing if it does not exist. Supports deletion of partially or fully read-only files.
		/// </summary>
		public static void DeleteFile( string path ) {
			var numberOfFailures = 0;
			while( File.Exists( path ) ) {
				try {
					RecursivelyRemoveReadOnlyAttributeFromItem( path );
					File.Delete( path );
				}
				catch( IOException e ) {
					handleFailedDeletion( path, ref numberOfFailures, e );
				}
				catch( UnauthorizedAccessException e ) {
					handleFailedDeletion( path, ref numberOfFailures, e );
				}
			}
		}

		private static void handleFailedDeletion( string path, ref int numberOfFailures, Exception exception ) {
			if( ++numberOfFailures >= 100 )
				throw new IOException( "Failed to delete " + path + " 100 times in a row. The inner exception is the most recent failure.", exception );
			Thread.Sleep( 100 );
		}


		/// <summary>
		/// Recursively removes the read-only attribute from the specified file or folder.
		/// </summary>
		public static void RecursivelyRemoveReadOnlyAttributeFromItem( string path ) {
			var attributes = File.GetAttributes( path );
			if( ( attributes & FileAttributes.ReadOnly ) == FileAttributes.ReadOnly )
				File.SetAttributes( path, attributes & ~FileAttributes.ReadOnly );
			if( Directory.Exists( path ) )
				foreach( var childPath in Directory.GetFileSystemEntries( path ) )
					RecursivelyRemoveReadOnlyAttributeFromItem( childPath );
		}


		/// <summary>
		/// Returns a text writer for writing a new file or overwriting an existing file.
		/// Automatically creates any folders needed in the given path, if necessary.
		/// We recommend passing an absolute path. If a relative path is passed, the working folder
		/// is used as the root path.
		/// Caller is responsible for properly disposing the stream.
		/// </summary>
		public static TextWriter GetTextWriterForWrite( string filePath ) => new StreamWriter( GetFileStreamForWrite( filePath ) );

		/// <summary>
		/// Returns a file stream for writing a new file or overwriting an existing file.
		/// Automatically creates any folders needed in the given path, if necessary.
		/// We recommend passing an absolute path. If a relative path is passed, the working folder
		/// is used as the root path.
		/// Caller is responsible for properly disposing the stream.
		/// </summary>
		public static FileStream GetFileStreamForWrite( string filePath ) {
			Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
			return File.Create( filePath );
		}
	}
}