namespace CommandRunner.DatabaseAbstraction {
	/// <summary>
	/// Represents a table and, optionally, its schema.
	/// </summary>
	public class Table {
		/// <summary>
		/// Maybe be blank.
		/// </summary>
		public string Schema { get; }

		/// <summary>
		/// The name of the table, only.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The string used to identify a table in a SQL script. 'schemaName.TableName'.
		/// </summary>
		public string ObjectIdentifier => Schema == "" ? Name : $"{Schema}.{Name}";

		/// <summary>
		/// Create a table representation with the given schema and name. Schema may be blank.
		/// </summary>
		public Table( string schema, string name ) {
			Schema = schema;
			Name = name;
		}
	}
}