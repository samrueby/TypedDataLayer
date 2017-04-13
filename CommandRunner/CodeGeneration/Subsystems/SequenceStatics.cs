using System.IO;
using CommandRunner.DatabaseAbstraction;
using TypedDataLayer.DataAccess;

namespace CommandRunner.CodeGeneration.Subsystems {
	internal static class SequenceStatics {
		internal static void Generate( DBConnection cn, TextWriter writer, string baseNamespace, IDatabase database, int? commandTimeout ) {
			writer.WriteLine( "namespace " + baseNamespace + ".Sequences {" );

			var cmd = cn.DatabaseInfo.CreateCommand( null );
			cmd.CommandText = "SELECT * FROM USER_SEQUENCES";
			cn.ExecuteReaderCommand(
				cmd,
				reader => {
					while( reader.Read() ) {
						var sequenceName = reader[ "SEQUENCE_NAME" ].ToString();
						writer.WriteLine();
						writer.WriteLine( "public class " + sequenceName + " {" );
						writer.WriteLine( "public static decimal GetNextValue() {" );
						writer.WriteLine( "DbCommand cmd = " + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionCreateCommandExpression( commandTimeout ) + ";" );
						writer.WriteLine( $@"cmd.CommandText = ""SELECT {sequenceName}.NEXTVAL FROM DUAL"";" );
						writer.WriteLine( "return (decimal)" + DataAccessStatics.DataAccessStateCurrentDatabaseConnectionExpression + ".ExecuteScalarCommand( cmd );" );
						writer.WriteLine( "}" );
						writer.WriteLine( "}" );
					}
				} );

			writer.WriteLine();
			writer.WriteLine( "}" );
		}
	}
}