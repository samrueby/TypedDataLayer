using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TypedDataLayer.Tools {
	/// <summary>
	/// A collection of miscellaneous statics that may be useful.
	/// </summary>
	public static class Utility {
		/// <summary>
		/// Recursively calls Path.Combine on the given paths.  Path is returned without a trailing slash.
		/// </summary>
		public static string CombinePaths( string one, string two, params string[] paths ) {
			if( one == null || two == null )
				throw new ArgumentException( "String cannot be null." );

			var pathList = new List<string>( paths );
			pathList.Insert( 0, two );
			pathList.Insert( 0, one );

			var combinedPath = "";

			foreach( var path in pathList )
				combinedPath += getTrimmedPath( path );

			return combinedPath.TrimEnd( '\\' );
		}

		private static string getTrimmedPath( string path ) {
			path = path.Trim( '\\' );
			path = path.Trim();
			if( path.Length > 0 )
				return path + "\\";
			return "";
		}

		/// <summary>
		/// Gets a valid C# identifier from the specified string.
		/// </summary>
		public static string GetCSharpIdentifier( string s ) {
			s = s.Replace( ' ', '_' ).Replace( '-', '_' );

			// Remove invalid characters.
			s = Regex.Replace( s, @"[^\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]", "" );

			// Prepend underscore if start character is invalid.
			if( Regex.IsMatch( s, @"^[^\p{L}\p{Nl}_]" ) )
				s = "_" + s;

			// The @ prefix tells the compiler to interpret the following string as an identifier instead of as a keyword. This is important if there is a table called 'new', for example.
			return "@" + s;
		}

		/// <summary>
		/// Returns true if the specified objects are equal according to the default equality comparer.
		/// </summary>
		public static bool AreEqual<T>( T x, T y ) => EqualityComparer<T>.Default.Equals( x, y );

		/// <summary>
		/// Returns an integer indicating whether the first specified object precedes (negative value), follows (positive value),
		/// or occurs in the same position in
		/// the sort order (zero) as the second specified object, according to the default sort-order comparer. If you are
		/// comparing strings, Microsoft recommends
		/// that you use a StringComparer instead of the default comparer.
		/// </summary>
		public static int Compare<T>( T x, T y, IComparer<T> comparer = null ) => ( comparer ?? Comparer<T>.Default ).Compare( x, y );

		/// <summary>
		/// Returns an Object with the specified Type and whose value is equivalent to the specified object.
		/// </summary>
		/// <param name="value">An Object that implements the IConvertible interface.</param>
		/// <param name="conversionType">The Type to which value is to be converted.</param>
		/// <returns>
		/// An object whose Type is conversionType (or conversionType's underlying type if conversionType
		/// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
		/// reference and conversionType is not a value type.
		/// </returns>
		/// <remarks>
		/// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
		/// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
		/// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
		/// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
		/// This method was written by Peter Johnson at:
		/// http://aspalliance.com/author.aspx?uId=1026.
		/// </remarks>
		public static object ChangeType( object value, Type conversionType ) {
			// This if block was taken from Convert.ChangeType as is, and is needed here since we're
			// checking properties on conversionType below.
			if( conversionType == null )
				throw new ArgumentNullException( nameof(conversionType) );

			// If it's not a nullable type, just pass through the parameters to Convert.ChangeType

			if( conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) ) {
				// It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
				// InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
				// determine what the underlying type is
				// If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
				// have a type--so just return null
				// We only do this check if we're converting to a nullable type, since doing it outside
				// would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
				// value is null and conversionType is a value type.
				if( value == null )
					return null;

				// It's a nullable type, and not null, so that means it can be converted to its underlying type,
				// so overwrite the passed-in conversion type with this underlying type
				var nullableConverter = new NullableConverter( conversionType );
				conversionType = nullableConverter.UnderlyingType;
			} // end if

			// Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
			// nullable type), pass the call on to Convert.ChangeType
			return Convert.ChangeType( value, conversionType );
		}

		public static T XmlDeserialize<T>( string filePath ) {
			using( var file = File.OpenRead( filePath ) )
				return (T)new XmlSerializer( typeof( T ) ).Deserialize( file );
		}

		/// <summary>
		/// Runs the specified program with the specified arguments and passes in the specified input. Optionally waits for the
		/// program to exit, and throws an
		/// exception if this is specified and a nonzero exit code is returned. If the program is in a folder that is included in
		/// the Path environment variable,
		/// specify its name only. Otherwise, specify a path to the program. In either case, you do NOT need ".exe" at the end.
		/// Specify the empty string for input
		/// if you do not wish to pass any input to the program.
		/// Returns the output of the program if waitForExit is true.  Otherwise, returns the empty string.
		/// </summary>
		public static string RunProgram( string program, string arguments, string input, bool waitForExit ) {
			var outputResult = "";
			using( var p = new Process() ) {
				p.StartInfo.FileName = program;
				p.StartInfo.Arguments = arguments;
				p.StartInfo.CreateNoWindow = true; // prevents command window from appearing
				p.StartInfo.UseShellExecute = false; // necessary for redirecting output
				p.StartInfo.RedirectStandardInput = true;
				if( waitForExit ) {
					// Set up output recording.
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					var output = new StringWriter();
					var errorOutput = new StringWriter();
					p.OutputDataReceived += ( sender, e ) => output.WriteLine( e.Data );
					p.ErrorDataReceived += ( sender, e ) => errorOutput.WriteLine( e.Data );

					p.Start();

					// Begin recording output.
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					// Pass input to the program.
					if( input.Length > 0 ) {
						p.StandardInput.Write( input );
						p.StandardInput.Flush();
					}

					// Throw an exception after the program exits if the code is not zero. Include all recorded output.
					p.WaitForExit();
					outputResult = output.ToString();
					if( p.ExitCode != 0 ) {
						using( var sw = new StringWriter() ) {
							sw.WriteLine( "Program exited with a nonzero code." );
							sw.WriteLine();
							sw.WriteLine( "Program: " + program );
							sw.WriteLine( "Arguments: " + arguments );
							sw.WriteLine();
							sw.WriteLine( "Output:" );
							sw.WriteLine( outputResult );
							sw.WriteLine();
							sw.WriteLine( "Error output:" );
							sw.WriteLine( errorOutput.ToString() );
							throw new ApplicationException( sw.ToString() );
						}
					}
				}
				else {
					p.Start();
					if( input.Length > 0 ) {
						p.StandardInput.Write( input );
						p.StandardInput.Flush();
					}
				}

				return outputResult;
			}
		}
	}
}