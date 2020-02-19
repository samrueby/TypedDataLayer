using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// GMS NOTE: Should really reference TEWL.
namespace TypedDataLayer.Tools {
	/// <summary>
	/// Provides helpful string methods.
	/// </summary>
	internal static class StringTools {
		/// <summary>
		/// Returns the given string with its first letter-or-digit character capitalized.
		/// </summary>
		public static string CapitalizeString( this string text ) {
			if( text == null )
				return null;

			return new string( text.ToCharArray().Select( ( c, index ) => index == getIndexOfFirstLetterOrDigit( text ) ? char.ToUpper( c ) : c ).ToArray() );
		}

		/// <summary>
		/// Returns the given string with its first letter-or-digit character lowercased. Do not pass null.
		/// </summary>
		public static string LowercaseString( this string text ) =>
			new string( text.ToCharArray().Select( ( c, index ) => index == getIndexOfFirstLetterOrDigit( text ) ? char.ToLower( c ) : c ).ToArray() );

		private static int getIndexOfFirstLetterOrDigit( string text ) => text.IndexOfAny( text.ToCharArray().Where( char.IsLetterOrDigit ).ToArray() );

		/// <summary>
		/// Returns the given string with every instance of "xY" where x is a lowercase
		/// letter and Y is a capital letter with "x Y".  Therefore, "LeftLeg" becomes "Left Leg".
		/// Also handles digits and converts a string such as "Reference1Name" to "Reference 1 Name".
		/// </summary>
		public static string CamelToEnglish( this string text ) {
			// Don't do anything with null
			// Skip empty string since we'll get out of range errors
			if( string.IsNullOrEmpty( text ) )
				return text;

			// When a space should be inserted directly before the current character onto the new string:
			// Y/N insert space
			//													text[i]
			//										lower		upper		digit
			//							lower   N				Y				Y
			//	text[i-1]		upper		N				N				Y
			//							digit		Y				Y				N

			var newText = "";
			for( var i = 1; i < text.Length; i++ ) {
				newText += text[ i - 1 ];

				var previousChar = new { IsUpper = char.IsUpper( text[ i - 1 ] ), IsLower = char.IsLower( text[ i - 1 ] ), IsDigit = char.IsDigit( text[ i - 1 ] ) };
				var currentChar = new { IsUpper = char.IsUpper( text[ i ] ), IsLower = char.IsLower( text[ i ] ), IsDigit = char.IsDigit( text[ i ] ) };

				if( currentChar.IsUpper && ( previousChar.IsLower || previousChar.IsDigit ) || currentChar.IsDigit && ( previousChar.IsLower || previousChar.IsUpper ) ||
				    currentChar.IsLower && previousChar.IsDigit )
					newText += " ";
			}

			return newText + text[ text.Length - 1 ];
		}

		/// <summary>
		/// Returns the given string with underscores replaced by spaces and capitalization at the beginning of every word.
		/// Example: "FIRST_NAME" becomes "First Name".
		/// </summary>
		public static string OracleToEnglish( this string text ) => ConcatenateWithDelimiter( " ", text.Separate( "_", true ).Select( s => s.ToLower().CapitalizeString() ).ToArray() );

		/// <summary>
		/// Removes whitespace from between words, capitalizes the first letter of each word, and lowercases the remainder of each
		/// word (ex: "one two" becomes "OneTwo").
		/// Trims the resulting string.
		/// Do not call this on the null string.
		/// </summary>
		public static string EnglishToPascal( this string text ) => ConcatenateWithDelimiter( "", text.Separate().Select( t => t.ToLower().CapitalizeString() ).ToArray() );

		/// <summary>
		/// Returns true if the given string is null or contains only whitespace (is empty after being trimmed). Do not call this
		/// unless you understand its
		/// appropriate and inappropriate uses as documented in coding standards.
		/// </summary>
		public static bool IsNullOrWhiteSpace( this string text ) => text == null || text.IsWhitespace();

		/// <summary>
		/// Returns true if the string is empty or made up entirely of whitespace characters (as defined by the Trim method).
		/// The string must not be null.
		/// </summary>
		public static bool IsWhitespace( this string text ) => text.Trim().Length == 0;

		/// <summary>
		/// Concatenates two strings together with a space between them. If either string is empty or if both strings are empty,
		/// there will be no space added.
		/// Null strings are treated as empty strings.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// </summary>
		public static string ConcatenateWithSpace( this string s1, string s2 ) => ConcatenateWithDelimiter( " ", s1, s2 );

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// </summary>
		internal static string ConcatenateWithDelimiter( string delimiter, params string[] strings ) => ConcatenateWithDelimiter( delimiter, (IEnumerable<string>)strings );

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// </summary>
		internal static string ConcatenateWithDelimiter( string delimiter, IEnumerable<string> strings ) {
			var tokens = strings.Select( i => ( i ?? "" ).Trim() ).Where( i => i.Length > 0 );
			if( !tokens.Any() )
				return "";
			var result = new StringBuilder( tokens.First() );
			foreach( var token in tokens.Skip( 1 ) )
				result.Append( delimiter + token );
			return result.ToString();
		}

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// Returns the StringBuilder that is passed to this function.
		/// </summary>
		internal static StringBuilder ConcatenateWithDelimiter( StringBuilder sb, string delimiter, IEnumerable<string> strings ) {
			var tokens = strings.Select( i => ( i ?? "" ).Trim() ).Where( i => i.Length > 0 );
			if( !tokens.Any() )
				return sb;

			sb.Append( tokens.First() );
			foreach( var token in tokens.Skip( 1 ) ) {
				sb.Append( delimiter );
				sb.Append( token );
			}

			return sb;
		}

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// Returns the StringBuilder that is passed to this function.
		/// </summary>
		internal static StringBuilder ConcatenateWithDelimiter( StringBuilder sb, string delimiter, params string[] strings ) =>
			ConcatenateWithDelimiter( sb, delimiter, (IEnumerable<string>)strings );

		/// <summary>
		/// Performs ConcatenateWithDelimiter with characters instead of strings.
		/// Null strings are treated as empty strings.
		/// </summary>
		public static string ConcatenateWithDelimiter( string delimiter, params char[] chars ) {
			var strings = new string[ chars.Length ];
			var cnt = 0;
			foreach( var character in chars )
				strings[ cnt++ ] = character.ToString();
			return ConcatenateWithDelimiter( delimiter, strings );
		}

		/// <summary>
		/// Returns the given string truncated to the given max length (if necessary).
		/// </summary>
		public static string Truncate( this string s, int maxLength ) {
			if( s == null )
				return null;

			return s.Substring( 0, Math.Min( maxLength, s.Length ) );
		}

		/// <summary>
		/// Removes all characters that are between the begin string and the end string, not including the begin and end strings.
		/// For example, "This 'quoted text'.".RemoveTextBetweenStrings( "'", "'" ) returns "This ''.";
		/// </summary>
		public static string RemoveTextBetweenStrings( this string s, string beginString, string endString ) =>
			Regex.Replace( s, getRegexSafeString( beginString ) + @"(.*?\s*)*" + getRegexSafeString( endString ), beginString + endString, RegexOptions.Multiline );

		private static string getRegexSafeString( string s ) => @"\" + ConcatenateWithDelimiter( @"\", s.ToCharArray() );

		/// <summary>
		/// Splits this non null string into a list of non null substrings using white space characters as separators. Empty
		/// substrings will be excluded from the
		/// list, and therefore, if this string is empty or contains only white space characters, the list will be empty.
		/// All strings in the resulting list are trimmed, not explicitly, but by definition because any surrounding whitespace
		/// would have counted as part of the delimiter.
		/// </summary>
		public static List<string> Separate( this string s ) {
			// Impossible to respond to R# warning because if you replace inline separators, the compiler can't figure out what method to call.
			string[] separators = null;
			return s.Split( separators, StringSplitOptions.RemoveEmptyEntries ).ToList();
		}

		/// <summary>
		/// Splits this non null string into a list of non null substrings using the specified separator. If substrings are trimmed
		/// and empties excluded, and this
		/// string is empty or contains only separators and white space characters, the list will be empty.
		/// </summary>
		public static List<string> Separate( this string s, string separator, bool trimSubStringsAndExcludeEmpties ) {
			var strings = s.Split( new[] { separator }, StringSplitOptions.None ).AsEnumerable();
			if( trimSubStringsAndExcludeEmpties )
				strings = strings.Select( str => str.Trim() ).Where( str => str.Length > 0 );
			return strings.ToList();
		}

		/// <summary>
		/// Returns a string representing the list of items in the form "one, two, three and four".
		/// </summary>
		public static string GetEnglishListPhrase( IEnumerable<string> items, bool useSerialComma ) {
			items = items.Where( i => i.Any() ).ToArray();
			switch( items.Count() ) {
				case 0:
					return "";
				case 1:
					return items.First();
				case 2:
					return items.First() + " and " + items.ElementAt( 1 );
				default:
					return ConcatenateWithDelimiter( ", ", items.Take( items.Count() - 1 ).ToArray() ) + ( useSerialComma ? ", and " : " and " ) + items.Last();
			}
		}


		/// <summary>
		/// Returns true if the two strings are equal, ignoring case.
		/// </summary>
		public static bool EqualsIgnoreCase( this string s, string otherString ) => s.Equals( otherString, StringComparison.OrdinalIgnoreCase );

		/// <summary>
		/// Converts this string to a given Enum value. Case sensitive.
		/// This method does not enforce valid Enum values.
		/// </summary>
		/// C# doesn't allow constraining the value to an Enum
		public static T ToEnum<T>( this string s ) => (T)Enum.Parse( typeof( T ), s );

		public static IEnumerable<T> GetEnumValues<T>() => Enum.GetValues( typeof( T ) ).Cast<T>();

		public static string FormatWith( this string s, params object[] os ) => string.Format( s, os );

		/// <summary>
		/// Returns a string in the format of "a, b, c" for the provided collection.
		/// </summary>
		public static string GetCommaDelimitedList( this IEnumerable<string> strs ) => ConcatenateWithDelimiter( ", ", strs );
	}
}