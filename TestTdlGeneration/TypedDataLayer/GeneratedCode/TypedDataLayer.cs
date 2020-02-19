using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using TypedDataLayer;
using TypedDataLayer.Tools;
using TypedDataLayer.Exceptions;
using TypedDataLayer.DatabaseSpecification;
using TypedDataLayer.DatabaseSpecification.Databases;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DataAccess.StandardModification;
using TypedDataLayer.DataAccess.RetrievalCaching;
using TypedDataLayer.DataAccess.CommandWriting;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction;
using TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction.Conditions;
using TypedDataLayer.DataAccess.CommandWriting.Commands;
using TypedDataLayer.Containers;
using TypedDataLayer.Collections;

namespace Tdl.TestGeneration.DataAccess {
public static class Configuration {
public const int CommandTimeoutSeconds = 5;
}
}

namespace Tdl.TestGeneration.DataAccess.TableConstants {
namespace dbo {
/// <summary>
/// This object represents the constants of the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
public class @GlobalIntsTable {
/// <summary>
/// The name of this table.
/// </summary>
public const string Name = "CommandRunner.DatabaseAbstraction.Table";
/// <summary>
/// Contains schema information about this column.
/// </summary>
public class @ParameterNameColumn {
/// <summary>
/// The name of this column.
/// </summary>
public const string Name = "ParameterName";
/// <summary>
/// The size of this column. For varchars, this is the length of the biggest string that can be stored in this column.
/// </summary>
public const int Size = 50;
}
/// <summary>
/// Contains schema information about this column.
/// </summary>
public class @ParameterValueColumn {
/// <summary>
/// The name of this column.
/// </summary>
public const string Name = "ParameterValue";
/// <summary>
/// The size of this column. For varchars, this is the length of the biggest string that can be stored in this column.
/// </summary>
public const int Size = 4;
}
}
}
namespace dbo {
/// <summary>
/// This object represents the constants of the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
public class @StatesTable {
/// <summary>
/// The name of this table.
/// </summary>
public const string Name = "CommandRunner.DatabaseAbstraction.Table";
/// <summary>
/// Contains schema information about this column.
/// </summary>
public class @StateIdColumn {
/// <summary>
/// The name of this column.
/// </summary>
public const string Name = "StateId";
/// <summary>
/// The size of this column. For varchars, this is the length of the biggest string that can be stored in this column.
/// </summary>
public const int Size = 4;
}
/// <summary>
/// Contains schema information about this column.
/// </summary>
public class @StateNameColumn {
/// <summary>
/// The name of this column.
/// </summary>
public const string Name = "StateName";
/// <summary>
/// The size of this column. For varchars, this is the length of the biggest string that can be stored in this column.
/// </summary>
public const int Size = 50;
}
/// <summary>
/// Contains schema information about this column.
/// </summary>
public class @AbbreviationColumn {
/// <summary>
/// The name of this column.
/// </summary>
public const string Name = "Abbreviation";
/// <summary>
/// The size of this column. For varchars, this is the length of the biggest string that can be stored in this column.
/// </summary>
public const int Size = 2;
}
}
}
}

namespace Tdl.TestGeneration.DataAccess.RowConstants {
namespace dbo {
/// <summary>
/// Provides constants copied from the dbo.States table.
/// </summary>
public class StatesRows {
/// <summary>
/// Constant generated from row in database table.
/// </summary>
public const int @Ak = 2;
/// <summary>
/// Constant generated from row in database table.
/// </summary>
public const int @Al = 1;
/// <summary>
/// Constant generated from row in database table.
/// </summary>
public const int @Ar = 4;
/// <summary>
/// Constant generated from row in database table.
/// </summary>
public const int @Az = 3;
/// <summary>
/// Constant generated from row in database table.
/// </summary>
public const int @Ny = 4000;
private static readonly OneToOneMap<int, string> valuesAndNames = new OneToOneMap<int, string>();
static StatesRows() {
valuesAndNames.Add( (int)(2), "AK" );
valuesAndNames.Add( (int)(1), "AL" );
valuesAndNames.Add( (int)(4), "AR" );
valuesAndNames.Add( (int)(3), "AZ" );
valuesAndNames.Add( (int)(4000), "NY" );
}
/// <summary>
/// Returns the name of the constant given the constant's value.
/// </summary>
public static string GetNameFromValue( int constantValue ) {
return valuesAndNames.GetRightFromLeft( constantValue );
}
/// <summary>
/// Returns the value of the constant given the constant's name.
/// </summary>
public static int GetValueFromName( string constantName ) {
return valuesAndNames.GetLeftFromRight( constantName );
}
}
}
}

namespace Tdl.TestGeneration.DataAccess.CommandConditions {
namespace dbo {
public interface GlobalIntsTableCondition: TableCondition {}
public static class @GlobalIntsTableEqualityConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterName: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterName( string value ) {
this.value = value;
}
internal string Value { get { return value; } }
InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( value, "VarChar" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterValue: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly int value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterValue( int value ) {
this.value = value;
}
internal int Value { get { return value; } }
InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value, "Int" ) ) ); } }
}
}
public static class @GlobalIntsTableInequalityConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterName: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly InequalityCondition.Operator op; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'.
/// </summary>
public @ParameterName( InequalityCondition.Operator op, string value ) {
this.op = op;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( value, "VarChar" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterValue: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly InequalityCondition.Operator op; 
private readonly int value;
/// <summary>
/// Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'.
/// </summary>
public @ParameterValue( InequalityCondition.Operator op, int value ) {
this.op = op;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( value, "Int" ) ) ); } }
}
}
public static class @GlobalIntsTableInConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterName: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly string subQuery;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterName( string subQuery ) {
this.subQuery = subQuery;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( "ParameterName", subQuery ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterValue: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly string subQuery;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterValue( string subQuery ) {
this.subQuery = subQuery;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( "ParameterValue", subQuery ); } }
}
}
public static class @GlobalIntsTableLikeConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterName: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly LikeCondition.Behavior behavior; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterName( LikeCondition.Behavior behavior, string value ) {
this.behavior = behavior;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, "ParameterName", value ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @ParameterValue: CommandConditions.dbo.GlobalIntsTableCondition {
private readonly LikeCondition.Behavior behavior; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @ParameterValue( LikeCondition.Behavior behavior, string value ) {
this.behavior = behavior;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, "ParameterValue", value ); } }
}
}
}
namespace dbo {
public interface StatesTableCondition: TableCondition {}
public static class @StatesTableEqualityConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateId: CommandConditions.dbo.StatesTableCondition {
private readonly int value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateId( int value ) {
this.value = value;
}
internal int Value { get { return value; } }
InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( "StateId", new DbParameterValue( value, "Int" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateName: CommandConditions.dbo.StatesTableCondition {
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateName( string value ) {
this.value = value;
}
internal string Value { get { return value; } }
InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( "StateName", new DbParameterValue( value, "NVarChar" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @Abbreviation: CommandConditions.dbo.StatesTableCondition {
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @Abbreviation( string value ) {
this.value = value;
}
internal string Value { get { return value; } }
InlineDbCommandCondition TableCondition.CommandCondition { get { return new EqualityCondition( new InlineDbCommandColumnValue( "Abbreviation", new DbParameterValue( value, "NVarChar" ) ) ); } }
}
}
public static class @StatesTableInequalityConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateId: CommandConditions.dbo.StatesTableCondition {
private readonly InequalityCondition.Operator op; 
private readonly int value;
/// <summary>
/// Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'.
/// </summary>
public @StateId( InequalityCondition.Operator op, int value ) {
this.op = op;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( "StateId", new DbParameterValue( value, "Int" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateName: CommandConditions.dbo.StatesTableCondition {
private readonly InequalityCondition.Operator op; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'.
/// </summary>
public @StateName( InequalityCondition.Operator op, string value ) {
this.op = op;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( "StateName", new DbParameterValue( value, "NVarChar" ) ) ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @Abbreviation: CommandConditions.dbo.StatesTableCondition {
private readonly InequalityCondition.Operator op; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command. Expression will read 'valueInDatabase op yourValue'. So new InequalityCondition( Operator.GreaterThan, value ) will turn into 'columnName > @value'.
/// </summary>
public @Abbreviation( InequalityCondition.Operator op, string value ) {
this.op = op;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InequalityCondition( op, new InlineDbCommandColumnValue( "Abbreviation", new DbParameterValue( value, "NVarChar" ) ) ); } }
}
}
public static class @StatesTableInConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateId: CommandConditions.dbo.StatesTableCondition {
private readonly string subQuery;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateId( string subQuery ) {
this.subQuery = subQuery;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( "StateId", subQuery ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateName: CommandConditions.dbo.StatesTableCondition {
private readonly string subQuery;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateName( string subQuery ) {
this.subQuery = subQuery;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( "StateName", subQuery ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @Abbreviation: CommandConditions.dbo.StatesTableCondition {
private readonly string subQuery;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @Abbreviation( string subQuery ) {
this.subQuery = subQuery;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new InCondition( "Abbreviation", subQuery ); } }
}
}
public static class @StatesTableLikeConditions {
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateId: CommandConditions.dbo.StatesTableCondition {
private readonly LikeCondition.Behavior behavior; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateId( LikeCondition.Behavior behavior, string value ) {
this.behavior = behavior;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, "StateId", value ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @StateName: CommandConditions.dbo.StatesTableCondition {
private readonly LikeCondition.Behavior behavior; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @StateName( LikeCondition.Behavior behavior, string value ) {
this.behavior = behavior;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, "StateName", value ); } }
}
/// <summary>
/// A condition that narrows the scope of a command.
/// </summary>
public class @Abbreviation: CommandConditions.dbo.StatesTableCondition {
private readonly LikeCondition.Behavior behavior; 
private readonly string value;
/// <summary>
/// Creates a condition to narrow the scope of a command.
/// </summary>
public @Abbreviation( LikeCondition.Behavior behavior, string value ) {
this.behavior = behavior;
this.value = value;
}
InlineDbCommandCondition TableCondition.CommandCondition { get { return new LikeCondition( behavior, "Abbreviation", value ); } }
}
}
}
}

namespace Tdl.TestGeneration.DataAccess.TableRetrieval {
/// <summary>
/// Contains logic that retrieves rows from the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
namespace dbo {
public static partial class @GlobalIntsTableRetrieval {
internal class BasicRow {
private readonly string @__parameterName;
private readonly int @__parameterValue;
internal BasicRow( DbDataReader reader ) {
@__parameterName = (string)reader.GetValue( 0 );
@__parameterValue = (int)reader.GetValue( 1 );
}
internal string @ParameterName { get { return @__parameterName; } }
internal int @ParameterValue { get { return @__parameterValue; } }
}
/// <summary>
/// Holds data for a row of this result.
/// </summary>
public partial class Row: IEquatable<Row> {
private readonly BasicRow __basicRow;
internal Row( BasicRow basicRow ) {
__basicRow = basicRow;
}
/// <summary>
/// This object will never be null.
/// </summary>
public string @ParameterName { get { return __basicRow.@ParameterName; } }
/// <summary>
/// This object will never be null.
/// </summary>
public int @ParameterValue { get { return __basicRow.@ParameterValue; } }
public override int GetHashCode() { 
return @ParameterName.GetHashCode();
}
public static bool operator == ( Row row1, Row row2 ) => Equals( row1, row2 );
			public static bool operator !=( Row row1, Row row2 ) => !Equals( row1, row2 );
public override bool Equals( object obj ) {
return Equals( obj as Row );
}
public bool Equals( Row other ) {
if( other == null ) return false;
return @ParameterName == other.@ParameterName && @ParameterValue == other.@ParameterValue;
}
public Modification.dbo.@GlobalIntsModification ToModification() {
return Modification.dbo.@GlobalIntsModification.CreateForSingleRowUpdate( @ParameterName, @ParameterValue );
}
}
private partial class Cache {
internal struct Key {
internal string ParameterName;
}
internal static Cache Current { get { return DataAccessState.Current.GetCacheValue( "GlobalIntsTableRetrieval", () => new Cache() ); } }
private readonly TableRetrievalQueryCache<Row> queries = new TableRetrievalQueryCache<Row>();
private readonly Dictionary<Key, Row> rowsByPk = new Dictionary<Key, Row>();
private Cache() {}
internal TableRetrievalQueryCache<Row> Queries => queries; 
internal Dictionary<Key, Row> RowsByPk => rowsByPk;
}
/// <summary>
/// Retrieves the rows from the table that match the specified conditions, ordered in a stable way.
/// </summary>
public static IEnumerable<Row> GetRows( params CommandConditions.dbo.GlobalIntsTableCondition[] conditions ) {
var parameterNameCondition = conditions.OfType<CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName>().FirstOrDefault();
var cache = Cache.Current;
var isPkQuery = parameterNameCondition != null && conditions.Count() == 1;
if( isPkQuery ) {
Row row;
if( cache.RowsByPk.TryGetValue( new Cache.Key {ParameterName = parameterNameCondition.Value}, out row ) )
return new [] {row};
}
return cache.Queries.GetResultSet( conditions.Select( i => i.CommandCondition ), commandConditions => {
var command = new InlineSelect( new[] { "*" }, "FROM dbo.GlobalInts", !isPkQuery, 5, orderByClause: "ORDER BY ParameterName" );
foreach( var i in commandConditions ) command.AddCondition( i );
var results = new List<Row>();
command.Execute( DataAccessState.Current.DatabaseConnection, r => { while( r.Read() ) results.Add( new Row( new BasicRow( r ) ) ); } );
foreach( var i in results ) {
cache.RowsByPk[ new Cache.Key {ParameterName = i.ParameterName} ] = i;
}
return results;
} );
}
public static Row GetRowMatchingPk( string parameterName, bool returnNullIfNoMatch = false ) {
return GetRows( new CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName( parameterName ) ).PrimaryKeySingle(returnNullIfNoMatch);
}
}
}
/// <summary>
/// Contains logic that retrieves rows from the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
namespace dbo {
public static partial class @StatesTableRetrieval {
internal class BasicRow {
private readonly int @__stateId;
private readonly string @__stateName;
private readonly string @__abbreviation;
internal BasicRow( DbDataReader reader ) {
@__stateId = (int)reader.GetValue( 0 );
@__stateName = (string)reader.GetValue( 1 );
@__abbreviation = (string)reader.GetValue( 2 );
}
internal int @StateId { get { return @__stateId; } }
internal string @StateName { get { return @__stateName; } }
internal string @Abbreviation { get { return @__abbreviation; } }
}
/// <summary>
/// Holds data for a row of this result.
/// </summary>
public partial class Row: IEquatable<Row> {
private readonly BasicRow __basicRow;
internal Row( BasicRow basicRow ) {
__basicRow = basicRow;
}
/// <summary>
/// This object will never be null.
/// </summary>
public int @StateId { get { return __basicRow.@StateId; } }
/// <summary>
/// This object will never be null.
/// </summary>
public string @StateName { get { return __basicRow.@StateName; } }
/// <summary>
/// This object will never be null.
/// </summary>
public string @Abbreviation { get { return __basicRow.@Abbreviation; } }
public override int GetHashCode() { 
return @StateId.GetHashCode();
}
public static bool operator == ( Row row1, Row row2 ) => Equals( row1, row2 );
			public static bool operator !=( Row row1, Row row2 ) => !Equals( row1, row2 );
public override bool Equals( object obj ) {
return Equals( obj as Row );
}
public bool Equals( Row other ) {
if( other == null ) return false;
return @StateId == other.@StateId && @StateName == other.@StateName && @Abbreviation == other.@Abbreviation;
}
public Modification.dbo.@StatesModification ToModification() {
return Modification.dbo.@StatesModification.CreateForSingleRowUpdate( @StateId, @StateName, @Abbreviation );
}
}
private partial class Cache {
internal struct Key {
internal int StateId;
}
internal static Cache Current { get { return DataAccessState.Current.GetCacheValue( "StatesTableRetrieval", () => new Cache() ); } }
private readonly TableRetrievalQueryCache<Row> queries = new TableRetrievalQueryCache<Row>();
private readonly Dictionary<Key, Row> rowsByPk = new Dictionary<Key, Row>();
private Cache() {}
internal TableRetrievalQueryCache<Row> Queries => queries; 
internal Dictionary<Key, Row> RowsByPk => rowsByPk;
}
/// <summary>
/// Retrieves the rows from the table, ordered in a stable way.
/// </summary>
public static IEnumerable<Row> GetAllRows() {
return GetRowsMatchingConditions();
}
/// <summary>
/// Retrieves the rows from the table that match the specified conditions, ordered in a stable way. Since the table is specified as small, you should only use this method if you cannot filter the rows in code.
/// </summary>
public static IEnumerable<Row> GetRowsMatchingConditions( params CommandConditions.dbo.StatesTableCondition[] conditions ) {
var stateIdCondition = conditions.OfType<CommandConditions.dbo.@StatesTableEqualityConditions.@StateId>().FirstOrDefault();
var cache = Cache.Current;
var isPkQuery = stateIdCondition != null && conditions.Count() == 1;
if( isPkQuery ) {
Row row;
if( cache.RowsByPk.TryGetValue( new Cache.Key {StateId = stateIdCondition.Value}, out row ) )
return new [] {row};
}
return cache.Queries.GetResultSet( conditions.Select( i => i.CommandCondition ), commandConditions => {
var command = new InlineSelect( new[] { "*" }, "FROM dbo.States", !isPkQuery, 5, orderByClause: "ORDER BY StateId" );
foreach( var i in commandConditions ) command.AddCondition( i );
var results = new List<Row>();
command.Execute( DataAccessState.Current.DatabaseConnection, r => { while( r.Read() ) results.Add( new Row( new BasicRow( r ) ) ); } );
foreach( var i in results ) {
cache.RowsByPk[ new Cache.Key {StateId = i.StateId} ] = i;
}
return results;
} );
}
public static Row GetRowMatchingId( int id, bool returnNullIfNoMatch = false ) {
var cache = Cache.Current;
cache.Queries.GetResultSet( new InlineDbCommandCondition[ 0 ], commandConditions => {
var command = new InlineSelect( new[] { "*" }, "FROM dbo.States", true, 5, orderByClause: "ORDER BY StateId" );
foreach( var i in commandConditions ) command.AddCondition( i );
var results = new List<Row>();
command.Execute( DataAccessState.Current.DatabaseConnection, r => { while( r.Read() ) results.Add( new Row( new BasicRow( r ) ) ); } );
foreach( var i in results ) {
cache.RowsByPk[ new Cache.Key {StateId = i.StateId} ] = i;
}
return results;
} );
if( !returnNullIfNoMatch )
return cache.RowsByPk[ new Cache.Key {StateId = id} ];
Row row;
return cache.RowsByPk.TryGetValue( new Cache.Key {StateId = id}, out row ) ? row : null;
}
public static Dictionary<int, Row> ToIdDictionary( this IEnumerable<Row> rows ) {
return rows.ToDictionary( i => i.@StateId );
}
}
}
}

namespace Tdl.TestGeneration.DataAccess.Modification {
namespace dbo {
public partial class @GlobalIntsModification {
/// <summary>
/// Inserts a row into the dbo.GlobalInts table.
/// </summary>
/// <param name="parameterName">Object does not allow null.</param>
/// <param name="parameterValue">Object does not allow null.</param>
public static void InsertRow( string @parameterName, int @parameterValue ) { 
var mod = CreateForInsert();
mod.@parameterNameColumnValue.Value = @parameterName;
mod.@parameterValueColumnValue.Value = @parameterValue;
mod.Execute();
}
/// <summary>
/// Inserts a row into the dbo.GlobalInts table.
/// </summary>
/// <param name="parameterName">Object does not allow null.</param>
/// <param name="parameterValue">Object does not allow null.</param>
public static void InsertRowWithoutAdditionalLogic( string @parameterName, int @parameterValue ) { 
var mod = CreateForInsert();
mod.@parameterNameColumnValue.Value = @parameterName;
mod.@parameterValueColumnValue.Value = @parameterValue;
mod.ExecuteWithoutAdditionalLogic();
}
/// <summary>
/// Updates rows in the dbo.GlobalInts table that match the specified conditions with the specified data.
/// </summary>
/// <param name="parameterName">Object does not allow null.</param>
/// <param name="parameterValue">Object does not allow null.</param>
/// <param name="requiredCondition">A condition.</param>
/// <param name="additionalConditions">Additional conditions.</param>
public static void UpdateRows( string @parameterName, int @parameterValue, CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
var mod = CreateForUpdate( requiredCondition, additionalConditions );
mod.@parameterNameColumnValue.Value = @parameterName;
mod.@parameterValueColumnValue.Value = @parameterValue;
mod.Execute();
}
/// <summary>
/// Updates rows in the dbo.GlobalInts table that match the specified conditions with the specified data.
/// </summary>
/// <param name="parameterName">Object does not allow null.</param>
/// <param name="parameterValue">Object does not allow null.</param>
/// <param name="requiredCondition">A condition.</param>
/// <param name="additionalConditions">Additional conditions.</param>
public static void UpdateRowsWithoutAdditionalLogic( string @parameterName, int @parameterValue, CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
var mod = CreateForUpdate( requiredCondition, additionalConditions );
mod.@parameterNameColumnValue.Value = @parameterName;
mod.@parameterValueColumnValue.Value = @parameterValue;
mod.ExecuteWithoutAdditionalLogic();
}
/// <summary>
/// <para>Deletes the rows that match the specified conditions and returns the number of rows deleted.</para><para>WARNING: After calling this method, delete referenced rows in other tables that are no longer needed.</para>
/// </summary>
public static int DeleteRows( CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
return DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => {
var conditions = getConditionList( requiredCondition, additionalConditions );
PostDeleteCall<IEnumerable<TableRetrieval.dbo.@GlobalIntsTableRetrieval.Row>> postDeleteCall = null;
preDelete( conditions, ref postDeleteCall );
var rowsDeleted = deleteRows( conditions );
if( postDeleteCall != null )
postDeleteCall.Execute();
return rowsDeleted;
} );
}
/// <summary>
/// <para>Deletes the rows that match the specified conditions and returns the number of rows deleted.</para><para>WARNING: After calling this method, delete referenced rows in other tables that are no longer needed.</para>
/// </summary>
public static int DeleteRowsWithoutAdditionalLogic( CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
var conditions = getConditionList( requiredCondition, additionalConditions );
var rowsDeleted = deleteRows( conditions );
return rowsDeleted;
}
private static int deleteRows( List<CommandConditions.dbo.GlobalIntsTableCondition> conditions ) {
var delete = new InlineDelete( "dbo.GlobalInts", 5 );
conditions.ForEach( condition => delete.AddCondition( condition.CommandCondition ) );
try {
return delete.Execute( DataAccessState.Current.DatabaseConnection );
}
catch(Exception e) {
rethrowAsDataModificationExceptionIfNecessary( e );
throw;
}
}
static partial void preDelete( List<CommandConditions.dbo.GlobalIntsTableCondition> conditions, ref PostDeleteCall<IEnumerable<TableRetrieval.dbo.@GlobalIntsTableRetrieval.Row>> postDeleteCall );
private ModificationType modType;
private List<CommandConditions.dbo.GlobalIntsTableCondition> conditions;
private readonly DataValue<string> @parameterNameColumnValue = new DataValue<string>();
/// <summary>
/// Gets or sets the value for the ParameterName column. Throws an exception if the value has not been initialized. Object does not allow null.
/// </summary>
public string @ParameterName { get { return @parameterNameColumnValue.Value; } set { @parameterNameColumnValue.Value = value; } }
/// <summary>
/// Indicates whether or not the value for the ParameterName has been set since object creation or the last call to Execute, whichever was latest.
/// </summary>
public bool @ParameterNameHasChanged { get { return @parameterNameColumnValue.Changed; } }
private readonly DataValue<int> @parameterValueColumnValue = new DataValue<int>();
/// <summary>
/// Gets or sets the value for the ParameterValue column. Throws an exception if the value has not been initialized. Object does not allow null.
/// </summary>
public int @ParameterValue { get { return @parameterValueColumnValue.Value; } set { @parameterValueColumnValue.Value = value; } }
/// <summary>
/// Indicates whether or not the value for the ParameterValue has been set since object creation or the last call to Execute, whichever was latest.
/// </summary>
public bool @ParameterValueHasChanged { get { return @parameterValueColumnValue.Changed; } }
/// <summary>
/// Creates a modification object in insert mode, which can be used to do a piecemeal insert of a new row in the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
public static dbo.@GlobalIntsModification CreateForInsert() {
return new dbo.@GlobalIntsModification { modType = ModificationType.Insert };
}
/// <summary>
/// Creates a modification object in update mode with the specified conditions, which can be used to do a piecemeal update of the dbo.GlobalInts table.
/// </summary>
public static dbo.@GlobalIntsModification CreateForUpdate( CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
var mod = new dbo.@GlobalIntsModification { modType = ModificationType.Update, conditions = getConditionList( requiredCondition, additionalConditions ) };
foreach( var condition in mod.conditions ) {
if( condition is CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName )
mod.@parameterNameColumnValue.Value = ( condition as CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName ).Value;
else if( condition is CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterValue )
mod.@parameterValueColumnValue.Value = ( condition as CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterValue ).Value;
}

mod.markColumnValuesUnchanged();
return mod;
}
/// <summary>
/// Creates a modification object in single-row update mode with the specified current data. All column values in this object will have HasChanged = false, despite being initialized. This object can then be used to do a piecemeal update of the dbo.GlobalInts table.
/// </summary>
public static dbo.@GlobalIntsModification CreateForSingleRowUpdate( string @parameterName, int @parameterValue ) {
var mod = new dbo.@GlobalIntsModification { modType = ModificationType.Update };
mod.conditions = new List<CommandConditions.dbo.GlobalIntsTableCondition>();
mod.conditions.Add( new CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName( @parameterName ) );
mod.@parameterNameColumnValue.Value = @parameterName;
mod.@parameterValueColumnValue.Value = @parameterValue;
mod.markColumnValuesUnchanged();
return mod;
}
private static List<CommandConditions.dbo.GlobalIntsTableCondition> getConditionList( CommandConditions.dbo.GlobalIntsTableCondition requiredCondition, params CommandConditions.dbo.GlobalIntsTableCondition[] additionalConditions ) {
var conditions = new List<CommandConditions.dbo.GlobalIntsTableCondition>();
conditions.Add( requiredCondition );
foreach( var condition in additionalConditions )
conditions.Add( condition );
return conditions;
}
private @GlobalIntsModification() {}
/// <summary>
/// Sets all column values. This is useful for enforcing the number of arguments when deferred execution is needed.
/// </summary>
/// <param name="parameterName">Object does not allow null.</param>
/// <param name="parameterValue">Object does not allow null.</param>
public void SetAllData( string @parameterName, int @parameterValue ) {
this.@parameterNameColumnValue.Value = @parameterName;
this.@parameterValueColumnValue.Value = @parameterValue;
}
/// <summary>
/// Executes this dbo.GlobalInts modification, persisting all changes. Executes any pre-insert, pre-update, post-insert, or post-update logic that may exist in the class.
/// </summary>
public void Execute() {
DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => {
var frozenModType = modType;
if( frozenModType == ModificationType.Insert )
preInsert();
else if( frozenModType == ModificationType.Update )
preUpdate();
executeInsertOrUpdate();
if( frozenModType == ModificationType.Insert )
postInsert();
else if( frozenModType == ModificationType.Update )
postUpdate();
markColumnValuesUnchanged();
} );
}
partial void preInsert();
partial void preUpdate();
/// <summary>
/// Executes this dbo.GlobalInts modification, persisting all changes. Does not execute pre-insert, pre-update, post-insert, or post-update logic that may exist in the class.
/// </summary>
public void ExecuteWithoutAdditionalLogic() {
executeInsertOrUpdate();
markColumnValuesUnchanged();
}
private void executeInsertOrUpdate() {
try {
if( modType == ModificationType.Insert ) {
var insert = new InlineInsert( "dbo.GlobalInts", false, 5 );
addColumnModifications( insert );
insert.Execute( DataAccessState.Current.DatabaseConnection );
if( !@ParameterNameHasChanged ) return;
modType = ModificationType.Update;
conditions = new List<CommandConditions.dbo.GlobalIntsTableCondition>();
conditions.Add( new CommandConditions.dbo.@GlobalIntsTableEqualityConditions.@ParameterName( @ParameterName ) );
}
else {
var update = new InlineUpdate( "dbo.GlobalInts", 5 );
addColumnModifications( update );
conditions.ForEach( condition => update.AddCondition( condition.CommandCondition ) );
update.Execute( DataAccessState.Current.DatabaseConnection );
}
}
catch(Exception e) {
rethrowAsDataModificationExceptionIfNecessary( e );
throw;
}
}
private void addColumnModifications( InlineDbModificationCommand cmd ) {
if( @parameterNameColumnValue.Changed )
cmd.AddColumnModification( new InlineDbCommandColumnValue( "ParameterName", new DbParameterValue( @ParameterName, "VarChar" ) ) );
if( @parameterValueColumnValue.Changed )
cmd.AddColumnModification( new InlineDbCommandColumnValue( "ParameterValue", new DbParameterValue( @ParameterValue, "Int" ) ) );
}
private static void rethrowAsDataModificationExceptionIfNecessary( System.Exception e ) {
var constraintNamesToViolationErrorMessages = new Dictionary<string,string>();
populateConstraintNamesToViolationErrorMessages( constraintNamesToViolationErrorMessages );
foreach( var pair in constraintNamesToViolationErrorMessages )
if( e.GetBaseException().Message.ToLower().Contains( pair.Key.ToLower() ) ) throw new DataModificationException( pair.Value );
}
static partial void populateConstraintNamesToViolationErrorMessages( Dictionary<string,string> constraintNamesToViolationErrorMessages );
partial void postInsert();
partial void postUpdate();
private void markColumnValuesUnchanged() {
@parameterNameColumnValue.ClearChanged();
@parameterValueColumnValue.ClearChanged();
}
}
}
namespace dbo {
public partial class @StatesModification {
/// <summary>
/// Inserts a row into the dbo.States table.
/// </summary>
/// <param name="stateId">Object does not allow null.</param>
/// <param name="stateName">Object does not allow null.</param>
/// <param name="abbreviation">Object does not allow null.</param>
public static void InsertRow( int @stateId, string @stateName, string @abbreviation ) { 
var mod = CreateForInsert();
mod.@stateIdColumnValue.Value = @stateId;
mod.@stateNameColumnValue.Value = @stateName;
mod.@abbreviationColumnValue.Value = @abbreviation;
mod.Execute();
}
/// <summary>
/// Inserts a row into the dbo.States table.
/// </summary>
/// <param name="stateId">Object does not allow null.</param>
/// <param name="stateName">Object does not allow null.</param>
/// <param name="abbreviation">Object does not allow null.</param>
public static void InsertRowWithoutAdditionalLogic( int @stateId, string @stateName, string @abbreviation ) { 
var mod = CreateForInsert();
mod.@stateIdColumnValue.Value = @stateId;
mod.@stateNameColumnValue.Value = @stateName;
mod.@abbreviationColumnValue.Value = @abbreviation;
mod.ExecuteWithoutAdditionalLogic();
}
/// <summary>
/// Updates rows in the dbo.States table that match the specified conditions with the specified data.
/// </summary>
/// <param name="stateId">Object does not allow null.</param>
/// <param name="stateName">Object does not allow null.</param>
/// <param name="abbreviation">Object does not allow null.</param>
/// <param name="requiredCondition">A condition.</param>
/// <param name="additionalConditions">Additional conditions.</param>
public static void UpdateRows( int @stateId, string @stateName, string @abbreviation, CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
var mod = CreateForUpdate( requiredCondition, additionalConditions );
mod.@stateIdColumnValue.Value = @stateId;
mod.@stateNameColumnValue.Value = @stateName;
mod.@abbreviationColumnValue.Value = @abbreviation;
mod.Execute();
}
/// <summary>
/// Updates rows in the dbo.States table that match the specified conditions with the specified data.
/// </summary>
/// <param name="stateId">Object does not allow null.</param>
/// <param name="stateName">Object does not allow null.</param>
/// <param name="abbreviation">Object does not allow null.</param>
/// <param name="requiredCondition">A condition.</param>
/// <param name="additionalConditions">Additional conditions.</param>
public static void UpdateRowsWithoutAdditionalLogic( int @stateId, string @stateName, string @abbreviation, CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
var mod = CreateForUpdate( requiredCondition, additionalConditions );
mod.@stateIdColumnValue.Value = @stateId;
mod.@stateNameColumnValue.Value = @stateName;
mod.@abbreviationColumnValue.Value = @abbreviation;
mod.ExecuteWithoutAdditionalLogic();
}
/// <summary>
/// <para>Deletes the rows that match the specified conditions and returns the number of rows deleted.</para><para>WARNING: After calling this method, delete referenced rows in other tables that are no longer needed.</para>
/// </summary>
public static int DeleteRows( CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
return DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => {
var conditions = getConditionList( requiredCondition, additionalConditions );
PostDeleteCall<IEnumerable<TableRetrieval.dbo.@StatesTableRetrieval.Row>> postDeleteCall = null;
preDelete( conditions, ref postDeleteCall );
var rowsDeleted = deleteRows( conditions );
if( postDeleteCall != null )
postDeleteCall.Execute();
return rowsDeleted;
} );
}
/// <summary>
/// <para>Deletes the rows that match the specified conditions and returns the number of rows deleted.</para><para>WARNING: After calling this method, delete referenced rows in other tables that are no longer needed.</para>
/// </summary>
public static int DeleteRowsWithoutAdditionalLogic( CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
var conditions = getConditionList( requiredCondition, additionalConditions );
var rowsDeleted = deleteRows( conditions );
return rowsDeleted;
}
private static int deleteRows( List<CommandConditions.dbo.StatesTableCondition> conditions ) {
var delete = new InlineDelete( "dbo.States", 5 );
conditions.ForEach( condition => delete.AddCondition( condition.CommandCondition ) );
try {
return delete.Execute( DataAccessState.Current.DatabaseConnection );
}
catch(Exception e) {
rethrowAsDataModificationExceptionIfNecessary( e );
throw;
}
}
static partial void preDelete( List<CommandConditions.dbo.StatesTableCondition> conditions, ref PostDeleteCall<IEnumerable<TableRetrieval.dbo.@StatesTableRetrieval.Row>> postDeleteCall );
private ModificationType modType;
private List<CommandConditions.dbo.StatesTableCondition> conditions;
private readonly DataValue<int> @stateIdColumnValue = new DataValue<int>();
/// <summary>
/// Gets or sets the value for the StateId column. Throws an exception if the value has not been initialized. Object does not allow null.
/// </summary>
public int @StateId { get { return @stateIdColumnValue.Value; } set { @stateIdColumnValue.Value = value; } }
/// <summary>
/// Indicates whether or not the value for the StateId has been set since object creation or the last call to Execute, whichever was latest.
/// </summary>
public bool @StateIdHasChanged { get { return @stateIdColumnValue.Changed; } }
private readonly DataValue<string> @stateNameColumnValue = new DataValue<string>();
/// <summary>
/// Gets or sets the value for the StateName column. Throws an exception if the value has not been initialized. Object does not allow null.
/// </summary>
public string @StateName { get { return @stateNameColumnValue.Value; } set { @stateNameColumnValue.Value = value; } }
/// <summary>
/// Indicates whether or not the value for the StateName has been set since object creation or the last call to Execute, whichever was latest.
/// </summary>
public bool @StateNameHasChanged { get { return @stateNameColumnValue.Changed; } }
private readonly DataValue<string> @abbreviationColumnValue = new DataValue<string>();
/// <summary>
/// Gets or sets the value for the Abbreviation column. Throws an exception if the value has not been initialized. Object does not allow null.
/// </summary>
public string @Abbreviation { get { return @abbreviationColumnValue.Value; } set { @abbreviationColumnValue.Value = value; } }
/// <summary>
/// Indicates whether or not the value for the Abbreviation has been set since object creation or the last call to Execute, whichever was latest.
/// </summary>
public bool @AbbreviationHasChanged { get { return @abbreviationColumnValue.Changed; } }
/// <summary>
/// Creates a modification object in insert mode, which can be used to do a piecemeal insert of a new row in the CommandRunner.DatabaseAbstraction.Table table.
/// </summary>
public static dbo.@StatesModification CreateForInsert() {
return new dbo.@StatesModification { modType = ModificationType.Insert };
}
/// <summary>
/// Creates a modification object in update mode with the specified conditions, which can be used to do a piecemeal update of the dbo.States table.
/// </summary>
public static dbo.@StatesModification CreateForUpdate( CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
var mod = new dbo.@StatesModification { modType = ModificationType.Update, conditions = getConditionList( requiredCondition, additionalConditions ) };
foreach( var condition in mod.conditions ) {
if( condition is CommandConditions.dbo.@StatesTableEqualityConditions.@StateId )
mod.@stateIdColumnValue.Value = ( condition as CommandConditions.dbo.@StatesTableEqualityConditions.@StateId ).Value;
else if( condition is CommandConditions.dbo.@StatesTableEqualityConditions.@StateName )
mod.@stateNameColumnValue.Value = ( condition as CommandConditions.dbo.@StatesTableEqualityConditions.@StateName ).Value;
else if( condition is CommandConditions.dbo.@StatesTableEqualityConditions.@Abbreviation )
mod.@abbreviationColumnValue.Value = ( condition as CommandConditions.dbo.@StatesTableEqualityConditions.@Abbreviation ).Value;
}

mod.markColumnValuesUnchanged();
return mod;
}
/// <summary>
/// Creates a modification object in single-row update mode with the specified current data. All column values in this object will have HasChanged = false, despite being initialized. This object can then be used to do a piecemeal update of the dbo.States table.
/// </summary>
public static dbo.@StatesModification CreateForSingleRowUpdate( int @stateId, string @stateName, string @abbreviation ) {
var mod = new dbo.@StatesModification { modType = ModificationType.Update };
mod.conditions = new List<CommandConditions.dbo.StatesTableCondition>();
mod.conditions.Add( new CommandConditions.dbo.@StatesTableEqualityConditions.@StateId( @stateId ) );
mod.@stateIdColumnValue.Value = @stateId;
mod.@stateNameColumnValue.Value = @stateName;
mod.@abbreviationColumnValue.Value = @abbreviation;
mod.markColumnValuesUnchanged();
return mod;
}
private static List<CommandConditions.dbo.StatesTableCondition> getConditionList( CommandConditions.dbo.StatesTableCondition requiredCondition, params CommandConditions.dbo.StatesTableCondition[] additionalConditions ) {
var conditions = new List<CommandConditions.dbo.StatesTableCondition>();
conditions.Add( requiredCondition );
foreach( var condition in additionalConditions )
conditions.Add( condition );
return conditions;
}
private @StatesModification() {}
/// <summary>
/// Sets all column values. This is useful for enforcing the number of arguments when deferred execution is needed.
/// </summary>
/// <param name="stateId">Object does not allow null.</param>
/// <param name="stateName">Object does not allow null.</param>
/// <param name="abbreviation">Object does not allow null.</param>
public void SetAllData( int @stateId, string @stateName, string @abbreviation ) {
this.@stateIdColumnValue.Value = @stateId;
this.@stateNameColumnValue.Value = @stateName;
this.@abbreviationColumnValue.Value = @abbreviation;
}
/// <summary>
/// Executes this dbo.States modification, persisting all changes. Executes any pre-insert, pre-update, post-insert, or post-update logic that may exist in the class.
/// </summary>
public void Execute() {
DataAccessState.Current.DatabaseConnection.ExecuteInTransaction( () => {
var frozenModType = modType;
if( frozenModType == ModificationType.Insert )
preInsert();
else if( frozenModType == ModificationType.Update )
preUpdate();
executeInsertOrUpdate();
if( frozenModType == ModificationType.Insert )
postInsert();
else if( frozenModType == ModificationType.Update )
postUpdate();
markColumnValuesUnchanged();
} );
}
partial void preInsert();
partial void preUpdate();
/// <summary>
/// Executes this dbo.States modification, persisting all changes. Does not execute pre-insert, pre-update, post-insert, or post-update logic that may exist in the class.
/// </summary>
public void ExecuteWithoutAdditionalLogic() {
executeInsertOrUpdate();
markColumnValuesUnchanged();
}
private void executeInsertOrUpdate() {
try {
if( modType == ModificationType.Insert ) {
var insert = new InlineInsert( "dbo.States", false, 5 );
addColumnModifications( insert );
insert.Execute( DataAccessState.Current.DatabaseConnection );
if( !@StateIdHasChanged ) return;
modType = ModificationType.Update;
conditions = new List<CommandConditions.dbo.StatesTableCondition>();
conditions.Add( new CommandConditions.dbo.@StatesTableEqualityConditions.@StateId( @StateId ) );
}
else {
var update = new InlineUpdate( "dbo.States", 5 );
addColumnModifications( update );
conditions.ForEach( condition => update.AddCondition( condition.CommandCondition ) );
update.Execute( DataAccessState.Current.DatabaseConnection );
}
}
catch(Exception e) {
rethrowAsDataModificationExceptionIfNecessary( e );
throw;
}
}
private void addColumnModifications( InlineDbModificationCommand cmd ) {
if( @stateIdColumnValue.Changed )
cmd.AddColumnModification( new InlineDbCommandColumnValue( "StateId", new DbParameterValue( @StateId, "Int" ) ) );
if( @stateNameColumnValue.Changed )
cmd.AddColumnModification( new InlineDbCommandColumnValue( "StateName", new DbParameterValue( @StateName, "NVarChar" ) ) );
if( @abbreviationColumnValue.Changed )
cmd.AddColumnModification( new InlineDbCommandColumnValue( "Abbreviation", new DbParameterValue( @Abbreviation, "NVarChar" ) ) );
}
private static void rethrowAsDataModificationExceptionIfNecessary( System.Exception e ) {
var constraintNamesToViolationErrorMessages = new Dictionary<string,string>();
populateConstraintNamesToViolationErrorMessages( constraintNamesToViolationErrorMessages );
foreach( var pair in constraintNamesToViolationErrorMessages )
if( e.GetBaseException().Message.ToLower().Contains( pair.Key.ToLower() ) ) throw new DataModificationException( pair.Value );
}
static partial void populateConstraintNamesToViolationErrorMessages( Dictionary<string,string> constraintNamesToViolationErrorMessages );
partial void postInsert();
partial void postUpdate();
private void markColumnValuesUnchanged() {
@stateIdColumnValue.ClearChanged();
@stateNameColumnValue.ClearChanged();
@abbreviationColumnValue.ClearChanged();
}
}
}
}

namespace Tdl.TestGeneration.DataAccess.Retrieval {
/// <summary>
/// This object holds the values returned from a PrimarySequence query.
/// </summary>
public static partial class PrimarySequenceRetrieval {
internal class BasicRow {
private readonly int @__id;
internal BasicRow( DbDataReader reader ) {
@__id = (int)reader.GetValue( 0 );
}
internal int @Id { get { return @__id; } }
}
/// <summary>
/// Holds data for a row of this result.
/// </summary>
public partial class Row: IEquatable<Row> {
private readonly BasicRow __basicRow;
internal Row( BasicRow basicRow ) {
__basicRow = basicRow;
}
/// <summary>
/// This object will never be null.
/// </summary>
public int @Id { get { return __basicRow.@Id; } }
public override int GetHashCode() { 
return @Id.GetHashCode();
}
public static bool operator == ( Row row1, Row row2 ) => Equals( row1, row2 );
			public static bool operator !=( Row row1, Row row2 ) => !Equals( row1, row2 );
public override bool Equals( object obj ) {
return Equals( obj as Row );
}
public bool Equals( Row other ) {
if( other == null ) return false;
return @Id == other.@Id;
}
}
private partial class Cache {
internal static Cache Current { get { return DataAccessState.Current.GetCacheValue( "PrimarySequenceQueryRetrieval", () => new Cache() ); } }
private readonly ParameterlessQueryCache<Row> rowsNextQuery = new ParameterlessQueryCache<Row>();
private Cache() {}
internal ParameterlessQueryCache<Row> RowsNextQuery { get { return rowsNextQuery; } }
}
private const string selectFromClause = @"SELECT NEXT VALUE FOR PrimarySequence as Id ";
/// <summary>
/// Queries the database and returns the full results collection immediately.
/// </summary>
public static IEnumerable<Row> GetRowsNext(  ) {
return Cache.Current.RowsNextQuery.GetResultSet( () => {
var cmd = DataAccessState.Current.DatabaseConnection.DatabaseInfo.CreateCommand(5);
cmd.CommandText = selectFromClause
;var results = new List<Row>();
DataAccessState.Current.DatabaseConnection.ExecuteReaderCommand( cmd, r => { while( r.Read() ) results.Add( new Row( new BasicRow( r ) ) ); } );
foreach( var i in results )
updateSingleRowCaches( i );
return results;
} );
}
static partial void updateSingleRowCaches( Row row );
}
}

