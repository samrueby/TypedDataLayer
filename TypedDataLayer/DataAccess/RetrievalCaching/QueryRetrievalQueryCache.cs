using System;
using System.Collections.Generic;
using TypedDataLayer.Collections;
using TypedDataLayer.Tools;

namespace TypedDataLayer.DataAccess.RetrievalCaching {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class QueryRetrievalQueryCache<RowType> {
		private readonly Cache<object[], IEnumerable<RowType>> cache;

		//[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public QueryRetrievalQueryCache() => cache = new Cache<object[], IEnumerable<RowType>>( false, comparer: new StructuralEqualityComparer<object[]>() );

		//[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public IEnumerable<RowType> GetResultSet( object[] parameterValues, Func<IEnumerable<RowType>> resultSetCreator ) => cache.GetOrAdd( parameterValues, resultSetCreator );
	}
}