using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TypedDataLayer.DataAccess.RetrievalCaching {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public class ParameterlessQueryCache<RowType> {
		private IEnumerable<RowType> resultSet;

		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public IEnumerable<RowType> GetResultSet( Func<IEnumerable<RowType>> resultSetCreator ) => resultSet ?? ( resultSet = resultSetCreator() );
	}
}