using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions {
	/// <summary>
	/// EWL Core and Development Utility use only.
	/// </summary>
	public class LikeCondition: InlineDbCommandCondition {
		/// <summary>
		/// This enum is accessible to developers of systems.
		/// </summary>
		public enum Behavior {
			/// <summary>
			/// Treats the entire search term as though it were in quotes. Equivalent to 'table.Column LIKE '%' + @columnValue + '%' in SQL Server.
			/// </summary>
			SingleTerm,

			/// <summary>
			/// Breaks the search string into tokens and performs N number of LIKE comparisons, And-ing them together.
			/// This will return results that successfully match each individual token. Double quotes can be used to force
			/// tokens (or the entire term) to be treated as a single token.
			/// </summary>
			AndedTokens
		}

		private readonly Behavior behavior;
		private readonly string columnName;
		private readonly string searchTerm;

		/// <summary>
		/// ISU use only.
		/// </summary>
		public LikeCondition( Behavior behavior, string columnName, string searchTerm ) {
			this.behavior = behavior;
			this.columnName = columnName;
			this.searchTerm = searchTerm;
		}

		void InlineDbCommandCondition.AddToCommand( IDbCommand command, StringBuilder commandText, DatabaseInfo databaseInfo, string parameterName ) {
			var tokens = new List<string>();
			if( behavior == Behavior.SingleTerm )
				tokens.Add( searchTerm.Trim() );
			else
				tokens.AddRange( searchTerm.Separate() );

			// NOTE: We may have to do some casing stuff for Oracle because existing queries seem to do UPPER on everything.
			var concatCharacter = databaseInfo is SqlServerInfo ? "+" : "||";
			var parameterNumber = 0;
			// NOTE: Is it important to tell the user they've been capped? How would we do that?

			StringTools.ConcatenateWithDelimiter(
				commandText,
				" AND ",
				tokens.Take( 20 /*Google allows many more tokens than this.*/ ).Select(
					t => {
						var parameter = new DbCommandParameter(
							parameterName + "L" + parameterNumber++,
							new DbParameterValue( t.Truncate( 128 /*This is Google's cap on word length.*/ ) ) );
						command.Parameters.Add( parameter.GetAdoDotNetParameter( databaseInfo ) );
						return $@"{columnName} LIKE '%' {concatCharacter} {parameter.GetNameForCommandText( databaseInfo )} {concatCharacter} '%'";
					} ) );
		}

		public override bool Equals( object obj ) => Equals( obj as InlineDbCommandCondition );

		public bool Equals( InlineDbCommandCondition other ) {
			var otherLikeCondition = other as LikeCondition;
			return otherLikeCondition != null && behavior == otherLikeCondition.behavior && columnName == otherLikeCondition.columnName &&
			       searchTerm == otherLikeCondition.searchTerm;
		}

		public override int GetHashCode() => new { behavior, columnName, searchTerm }.GetHashCode();

		int IComparable.CompareTo( object obj ) {
			var otherCondition = obj as InlineDbCommandCondition;
			if( otherCondition == null && obj != null )
				throw new ArgumentException();
			return CompareTo( otherCondition );
		}

		public int CompareTo( InlineDbCommandCondition other ) {
			if( other == null )
				return 1;
			var otherLikeCondition = other as LikeCondition;
			if( otherLikeCondition == null )
				return DataAccessMethods.CompareCommandConditionTypes( this, other );

			var behaviorResult = Utility.Compare( behavior, otherLikeCondition.behavior );
			if( behaviorResult != 0 )
				return behaviorResult;
			var columnNameResult = Utility.Compare( columnName, otherLikeCondition.columnName, comparer: StringComparer.InvariantCulture );
			return columnNameResult != 0 ? columnNameResult : Utility.Compare( searchTerm, otherLikeCondition.searchTerm, comparer: StringComparer.InvariantCulture );
		}
	}
}