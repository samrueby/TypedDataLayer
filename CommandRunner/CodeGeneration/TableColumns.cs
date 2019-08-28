using System;
using System.Collections.Generic;
using System.Linq;
using CommandRunner.Exceptions;
using TypedDataLayer.DataAccess;
using TypedDataLayer.DatabaseSpecification.Databases;

namespace CommandRunner.CodeGeneration {
	internal class TableColumns {
		internal readonly IEnumerable<Column> AllColumns;

		private readonly List<Column> keyColumns = new List<Column>();

		/// <summary>
		/// Returns either all components of the primary key, or the identity (alone).
		/// </summary>
		internal IEnumerable<Column> KeyColumns => keyColumns;

		internal Column IdentityColumn { get; }

		internal readonly Column RowVersionColumn;
		internal readonly IEnumerable<Column> AllColumnsExceptRowVersion;
		internal readonly IEnumerable<Column> AllNonIdentityColumnsExceptRowVersion;

		internal Column PrimaryKeyAndRevisionIdColumn { get; }

		/// <summary>
		/// Gets all columns that are not the identity column, the row version column, or the primary key and revision ID column.
		/// </summary>
		internal IEnumerable<Column> DataColumns { get; }

		internal TableColumns( DBConnection cn, string tableIdentifier, bool forRevisionHistoryLogic ) {
			try {
				// NOTE: Cache this result.
				AllColumns = Column.GetColumnsInQueryResults( cn, "SELECT * FROM " + tableIdentifier, true );

				foreach( var col in AllColumns ) {
					if( !( cn.DatabaseInfo is OracleInfo ) && col.DataTypeName == typeof( string ).ToString() && col.AllowsNull )
						throw new UserCorrectableException( $"String column {col.Name} allows null, which is not allowed." );
				}

				// Identify key, identity, and non identity columns.
				var nonIdentityColumns = new List<Column>();
				foreach( var col in AllColumns ) {
					if( col.IsKey )
						keyColumns.Add( col );
					if( col.IsIdentity ) {
						if( IdentityColumn != null )
							throw new ApplicationException( "Only one identity column per table is supported." );
						IdentityColumn = col;
					}
					else
						nonIdentityColumns.Add( col );
				}

				if( !keyColumns.Any() )
					throw new ApplicationException( "The table must contain a primary key or other means of uniquely identifying a row." );

				// If the table has a composite key, try to use the identity as the key instead since this will enable InsertRow to return a value.
				if( IdentityColumn != null && keyColumns.Count > 1 ) {
					keyColumns.Clear();
					keyColumns.Add( IdentityColumn );
				}

				RowVersionColumn = AllColumns.SingleOrDefault( i => i.IsRowVersion );
				AllColumnsExceptRowVersion = AllColumns.Where( i => !i.IsRowVersion ).ToArray();
				AllNonIdentityColumnsExceptRowVersion = nonIdentityColumns.Where( i => !i.IsRowVersion ).ToArray();

				if( forRevisionHistoryLogic ) {
					if( keyColumns.Count != 1 ) {
						throw new ApplicationException(
							"A revision history modification class can only be created for tables with exactly one primary key column, which is assumed to also be a foreign key to the revisions table." );
					}

					PrimaryKeyAndRevisionIdColumn = keyColumns.Single();
					if( PrimaryKeyAndRevisionIdColumn.IsIdentity )
						throw new ApplicationException( "The revision ID column of a revision history table must not be an identity." );
				}

				DataColumns = AllColumns.Where( col => !col.IsIdentity && !col.IsRowVersion && col != PrimaryKeyAndRevisionIdColumn ).ToArray();
			}
			catch( UserCorrectableException e ) {
				throw new UserCorrectableException( $"There was a problem getting columns for table {tableIdentifier}.", e );
			}
			catch( Exception e ) {
				throw new ApplicationException( $"An exception occurred while getting columns for table {tableIdentifier}.", e );
			}
		}
	}
}