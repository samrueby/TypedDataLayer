using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypedDataLayer {
	/// <summary>
	/// A collection of miscellaneous statics that may be useful.
	/// </summary>
	public static class EwlStatics {
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
			if( GetCSharpKeywords().Contains( s ) )
				return "@" + s;

			s = s.Replace( ' ', '_' ).Replace( '-', '_' );

			// Remove invalid characters.
			s = Regex.Replace( s, @"[^\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]", "" );

			// Prepend underscore if start character is invalid.
			if( Regex.IsMatch( s, @"^[^\p{L}\p{Nl}_]" ) )
				s = "_" + s;

			return s;
		}

		/// <summary>
		/// Gets a very limited set of CSharp keywords.
		/// </summary>

		// There is no programmatic way to get C# keywords. So we'll expand this list as needed.
		// GMS: This being public puts a lot of burden on us to have this list be complete.
		public static string[] GetCSharpKeywords() => new[] { "base", "enum" };


		/// <summary>
		/// Returns true if the specified objects are equal according to the default equality comparer.
		/// </summary>
		public static bool AreEqual<T>( T x, T y ) => EqualityComparer<T>.Default.Equals( x, y );

		/// <summary>
		/// Returns an integer indicating whether the first specified object precedes (negative value), follows (positive value), or occurs in the same position in
		/// the sort order (zero) as the second specified object, according to the default sort-order comparer. If you are comparing strings, Microsoft recommends
		/// that you use a StringComparer instead of the default comparer.
		/// </summary>
		public static int Compare<T>( T x, T y, IComparer<T> comparer = null ) => ( comparer ?? Comparer<T>.Default ).Compare( x, y );
	}
}