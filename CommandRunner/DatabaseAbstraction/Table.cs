using System.Linq;
using CommandRunner.CodeGeneration;
using CommandRunner.CodeGeneration.Subsystems;
using TypedDataLayer.DataAccess;
using TypedDataLayer.Tools;

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

		/// <summary>
		/// Simply the name of the standard modification class, to be used in class declarations. Not to be used to refer to the class, as when declaring a variable of its type.
		/// </summary>
		public string GetStandardModificationClassDeclaration( DBConnection cn ) {
			return Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "Modification" );
		}

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		public string GetStandardModificationClassReference( DBConnection cn ) {
			if( Schema.Any() )
				return Utility.GetCSharpIdentifier( $"{Schema.TableNameToPascal( cn )}.{Name.TableNameToPascal( cn )}Modification" );
			return GetStandardModificationClassDeclaration( cn );
		}

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		internal string GetTableConditionInterfaceReference( DBConnection cn ) =>
			$"{CommandConditionStatics.CommandConditionsNamespace}.{GetTableConditionInterfaceDeclaration( cn )}";

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		internal string GetEqualityConditionClassReference( DBConnection cn,  Column column ) =>
			$"{CommandConditionStatics.CommandConditionsNamespace}.{GetTableEqualityConditionsClassDeclaration( cn )}.{CommandConditionStatics.GetConditionClassName( column )}";

		/// <summary>
		/// Simply the name of the table condition interface, to be used in interface declaration. Not to be used to refer to the interface, as when declaring a variable of its type.
		/// </summary>
		internal string GetTableConditionInterfaceDeclaration( DBConnection cn ) => Name.TableNameToPascal( cn ) + "TableCondition";

		/// <summary>
		/// Simply the name of the equality conditions class, to be used in class declarations. Not to be used to refer to the class, as when declaring a variable of its type.
		/// </summary>
		internal string GetTableEqualityConditionsClassDeclaration( DBConnection cn ) =>
			Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "TableEqualityConditions" );
	}
}