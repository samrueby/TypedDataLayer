using System;
using System.Data;
using System.Text;
using TypedDataLayer.DatabaseSpecification;

namespace TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public interface InlineDbCommandCondition: IEquatable<InlineDbCommandCondition>, IComparable, IComparable<InlineDbCommandCondition> {
		/// <summary>
		/// Use at your own risk.
		/// </summary>
		void AddToCommand( IDbCommand command, StringBuilder commandText, DatabaseInfo databaseInfo, string parameterName );
	}
}