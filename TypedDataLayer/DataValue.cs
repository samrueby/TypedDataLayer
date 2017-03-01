using System;

namespace TypedDataLayer {
	/// <summary>
	/// A value that knows whether it has been initialized and whether it has changed.
	/// </summary>
	public class DataValue<T>: IEquatable<DataValue<T>> {
		private readonly InitializationAwareValue<T> val = new InitializationAwareValue<T>();

		public bool Changed { get; private set; }

		public T Value {
			get { return val.Value; }
			set {
				if( val.Initialized && Utility.AreEqual( val.Value, value ) )
					return;
				val.Value = value;
				Changed = true;
			}
		}

		public void ClearChanged() => Changed = false;

		public override bool Equals( object obj ) => Equals( obj as DataValue<T> );

		public bool Equals( DataValue<T> other ) => other != null && Utility.AreEqual( val, other.val );

		public override int GetHashCode() => val.GetHashCode();
	}
}