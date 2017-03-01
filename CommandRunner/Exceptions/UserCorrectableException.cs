using System;
using System.Text;

namespace CommandRunner.Exceptions {
	/// <summary>
	/// Represents an exception where its message is friendly to display to the user
	/// with feedback on how to correct the problem.
	/// </summary>
	internal class UserCorrectableException: ApplicationException {
		public UserCorrectableException( string message ): base( message ) { }
		public UserCorrectableException( string message, Exception innerException ): base( message, innerException ) { }

		public string ToFriendlyStack() {
			var sb = new StringBuilder();
			writeMessageAndInnerExceptionMessages( sb, this );
			return sb.ToString();
		}

		private static void writeMessageAndInnerExceptionMessages( StringBuilder sw, Exception e ) {
			sw.AppendLine( e.Message );
			if( e.InnerException != null ) {
				sw.AppendLine( "---------- Because ----------" );
				writeMessageAndInnerExceptionMessages( sw, e.InnerException );
			}
		}
	}
}