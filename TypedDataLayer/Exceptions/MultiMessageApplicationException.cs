using System;
using System.Collections.Generic;
using TypedDataLayer.Tools;

namespace TypedDataLayer.Exceptions {
	/// <summary>
	/// An application exception with multiple messages.
	/// </summary>
	public class MultiMessageApplicationException: ApplicationException {
		private readonly string[] messages;

		/// <summary>
		/// Creates an exception with the specified messages.
		/// </summary>
		public MultiMessageApplicationException( params string[] messages ): base( StringTools.ConcatenateWithDelimiter( Environment.NewLine, messages ) ) => this.messages = messages;

		/// <summary>
		/// Gets the messages that describe the exception.
		/// </summary>
		public IReadOnlyCollection<string> Messages => messages;
	}
}