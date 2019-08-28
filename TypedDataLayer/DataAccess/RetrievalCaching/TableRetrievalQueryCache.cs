using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TypedDataLayer.Collections;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.RetrievalCaching {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class TableRetrievalQueryCache<RowType> {
		private readonly Cache<InlineDbCommandCondition[], IEnumerable<RowType>> cache;

		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public TableRetrievalQueryCache() =>
			cache = new Cache<InlineDbCommandCondition[], IEnumerable<RowType>>( false, comparer: new StructuralEqualityComparer<InlineDbCommandCondition[]>() );

		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public IEnumerable<RowType> GetResultSet(
			IEnumerable<InlineDbCommandCondition> conditions, Func<IEnumerable<InlineDbCommandCondition>, IEnumerable<RowType>> resultSetCreator ) {
			var conditionArray = conditions.OrderBy( i => i ).ToArray();
			return cache.GetOrAdd( conditionArray, () => resultSetCreator( conditionArray ) );
		}
	}
}