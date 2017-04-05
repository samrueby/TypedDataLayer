using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TypedDataLayer.Exceptions;

namespace TypedDataLayer.Collections {
	/// <summary>
	/// Collection Exceptions.
	/// </summary>
	[ UsedImplicitly( ImplicitUseTargetFlags.Members ) ]
	public static class CollectionExtensions {
		/// <summary>
		/// Ensures there is exactly 1 or 0 items in the collection depending on <paramref name="allowZero"/>.
		/// </summary>
		/// <exception cref="InvalidPrimaryKeyException">Thrown if <paramref name="allowZero"/> is null and there are no results</exception>
		/// <exception cref="MoreThanOneRowException">If more than one item in the collection.</exception>
		public static T PrimaryKeySingle<T>( this IEnumerable<T> source, bool allowZero ) where T: class {
			// Based on the implementation of Single()

			if( source == null )
				throw new ArgumentException( nameof( source ) );

			InvalidPrimaryKeyException noResults() => new InvalidPrimaryKeyException( "One row was expected, but zero were returned." );

			var sourceList = source as IList<T>;
			if( sourceList != null ) {
				switch( sourceList.Count ) {
					case 0:
						if( allowZero ) {
							return null;
						}
						throw noResults();
					case 1:
						return sourceList[ 0 ];
				}
			}
			else {
				using( var enumerator = source.GetEnumerator() ) {
					if( !enumerator.MoveNext() ) {
						if( allowZero ) {
							return null;
						}
						throw noResults();
					}
					var current = enumerator.Current;
					if( !enumerator.MoveNext() )
						return current;
				}
			}
			throw new MoreThanOneRowException( $"Only 1 {( allowZero ? "or 0 rows were" : "row was" )} expected" );
		}
	}
}