using System;
using System.Linq;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.Tools;

namespace CommandRunner.DatabaseAbstraction {
	/// <summary>
	/// Internal and Development Utility use only.
	/// </summary>
	public class ValueContainer {
		private readonly Type unconvertedDataType;
		private readonly Func<string, string> incomingValueConversionExpressionGetter;
		private readonly Func<object, object> incomingValueConverter;
		private readonly Func<string, string> outgoingValueConversionExpressionGetter;
		private readonly string dbTypeString;

		// We'll remove this when we're ready to migrate Oracle systems to Pascal-cased column names.

		public ValueContainer( string name, Type dataType, string dbTypeString, int size, bool allowsNull, DatabaseInfo databaseInfo ) {
			Name = name;
			PascalCasedName = databaseInfo is OracleInfo ? name.OracleToEnglish().EnglishToPascal() : name;
			PascalCasedNameExceptForOracle = databaseInfo is OracleInfo ? name : PascalCasedName;
			unconvertedDataType = dataType;
			this.dbTypeString = dbTypeString;
			NullValueExpression = databaseInfo is OracleInfo && new[] { "Clob", "NClob" }.Contains( dbTypeString ) ? "\"\"" : "";
			Size = size;

			// MySQL LONGTEXT returns zero for size.
			if( databaseInfo is MySqlInfo && dbTypeString == "Text" && size == 0 )
				Size = int.MaxValue;

			if( databaseInfo is MySqlInfo && dbTypeString == "Bit" && Size == 1 ) {
				if( unconvertedDataType != typeof( ulong ) )
					throw new ApplicationException( "The unconverted data type was not ulong." );

				DataType = typeof( bool );
				incomingValueConversionExpressionGetter = valueExpression => "System.Convert.ToBoolean( {0} )".FormatWith( valueExpression );
				incomingValueConverter = value => Convert.ToBoolean( value );
				outgoingValueConversionExpressionGetter = valueExpression => "System.Convert.ToUInt64( {0} )".FormatWith( valueExpression );
			}
			else {
				DataType = unconvertedDataType;
				incomingValueConversionExpressionGetter = valueExpression => "({0}){1}".FormatWith( PrimitiveNameToFriendlyName( dataType ), valueExpression );
				incomingValueConverter = value => value;
				outgoingValueConversionExpressionGetter = valueExpression => valueExpression;
			}

			AllowsNull = allowsNull;
		}

		public string Name { get; }

		public string PascalCasedName { get; }

		public string PascalCasedNameExceptForOracle { get; }

		public string CamelCasedName => PascalCasedName.LowercaseString();

		public Type DataType { get; }

		/// <summary>
		/// Gets the name of the data type for this container, or the nullable data type if the container allows null.
		/// </summary>
		public string DataTypeName => AllowsNull ? NullableDataTypeName : PrimitiveNameToFriendlyName( DataType );

		public static string PrimitiveNameToFriendlyName( Type t ) {
			var str = t.ToString();
			switch( str ) {
				case "System.Boolean":
					return "bool";
				case "System.Boolean?":
					return "bool?";
				case "System.Int32":
					return "int";
				case "System.Int32?":
					return "int?";
				case "System.String":
					return "string";
				case "System.Decimal":
					return "decimal";
				case "System.Decimal?":
					return "decimal?";
				case "System.Double":
					return "double";
				case "System.Double?":
					return "double?";
				case "System.DateTime":
					return "DateTime";
				case "System.DateTime?":
					return "DateTime?";
				default:
					return str;
			}
		}

		/// <summary>
		/// Gets the name of the nullable data type for this container, regardless of whether the container allows null. The
		/// nullable data type is equivalent to the
		/// data type if the latter is a reference type or if the null value is represented with an expression other than "null".
		/// </summary>
		public string NullableDataTypeName => DataType.IsValueType && !NullValueExpression.Any() ? DataType + "?" : DataType.ToString();

		public string NullValueExpression { get; }

		public string UnconvertedDataTypeName => unconvertedDataType.ToString();

		public string GetIncomingValueConversionExpression( string valueExpression ) => incomingValueConversionExpressionGetter( valueExpression );

		public object ConvertIncomingValue( object value ) => incomingValueConverter( value );

		public int Size { get; }

		public bool AllowsNull { get; }

		public string GetParameterValueExpression( string valueExpression ) {
			var conversionExpression = outgoingValueConversionExpressionGetter( valueExpression );
			var parameterValueExpression = valueExpression == "null" ? valueExpression :
			                               conversionExpression == valueExpression || DataType.IsValueType && ( NullValueExpression.Any() || !AllowsNull ) ? conversionExpression :
			                               "{0} != null ? {1} : null".FormatWith(
				                               valueExpression,
				                               DataType.IsValueType ? "({0}?){1}".FormatWith( UnconvertedDataTypeName, conversionExpression ) : conversionExpression );
			return "new DbParameterValue( {0}, \"{1}\" )".FormatWith( parameterValueExpression, dbTypeString );
		}
	}
}