using System.Collections;
using System.Collections.Generic;

namespace TypedDataLayer.Tools {
	/// <summary>
	/// A generic version of StructuralComparisons.StructuralEqualityComparer.
	/// </summary>
	public class StructuralEqualityComparer<T>: EqualityComparer<T> {
		public override bool Equals( T x, T y ) => StructuralComparisons.StructuralEqualityComparer.Equals( x, y );

		public override int GetHashCode( T obj ) => StructuralComparisons.StructuralEqualityComparer.GetHashCode( obj );
	}
}