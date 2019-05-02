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
		private readonly DBConnection cn;

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
		public Table( DBConnection cn, string schema, string name ) {
			this.cn = cn;
			Schema = schema;
			Name = name;
		}

		/// <summary>
		/// Simply the name of the standard modification class, to be used in class declarations. Not to be used to refer to the class, as when declaring a variable of its type.
		/// </summary>
		public string GetStandardModificationClassDeclaration() {
			return Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "Modification" );
		}

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		public string GetStandardModificationClassReference() {
			return getSchemaNamespacePrefix() + GetStandardModificationClassDeclaration();
		}

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		internal string GetTableConditionInterfaceReference() =>
			$"{CommandConditionStatics.CommandConditionsNamespace}.{getSchemaNamespacePrefix()}{GetTableConditionInterfaceDeclaration()}";

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		internal string GetEqualityConditionClassReference( DBConnection cn, Column column ) =>
			$"{CommandConditionStatics.CommandConditionsNamespace}.{getSchemaNamespacePrefix()}{GetTableEqualityConditionsClassDeclaration()}.{CommandConditionStatics.GetConditionClassName( column )}";

		private string getSchemaNamespacePrefix() {
			return Schema.Any() ? Schema.TableNameToPascal( cn ) + "." : "";
		}

		/// <summary>
		/// Simply the name of the table condition interface, to be used in interface declaration. Not to be used to refer to the interface, as when declaring a variable of its type.
		/// </summary>
		internal string GetTableConditionInterfaceDeclaration() => Name.TableNameToPascal( cn ) + "TableCondition";

		/// <summary>
		/// Simply the name of the equality conditions class, to be used in class declarations. Not to be used to refer to the class, as when declaring a variable of its type.
		/// </summary>
		internal string GetTableEqualityConditionsClassDeclaration() =>
			Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "TableEqualityConditions" );

		/// <summary>
		/// A qualified reference to the class, including necessary namespaces prefixes. To be used when declaring a variable of this type.
		/// </summary>
		internal string GetTableRetrievalClassReference() => getSchemaNamespacePrefix() + Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "TableRetrieval" );

		/// <summary>
		/// Simply the name of the table retrieval class, to be used in class declaration. Not to be used to refer to the class, as when declaring a variable of its type.
		/// </summary>
		internal string GetTableRetrievalClassDeclaration() => Utility.GetCSharpIdentifier( Name.TableNameToPascal( cn ) + "TableRetrieval" );
	}
}