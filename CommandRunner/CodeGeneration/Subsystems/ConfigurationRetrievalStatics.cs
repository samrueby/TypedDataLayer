using System.IO;

namespace CommandRunner.CodeGeneration.Subsystems {
	internal static class ConfigurationRetrievalStatics {
		private const string className = "Configuration";
		private const string commandTimeoutSecondsPropertyName = "CommandTimeoutSeconds";

		internal static void Generate( TextWriter writer, string baseNamespace, Database configuration ) {
			// NOTE SJR: Nothing is using this but they should be.
			writer.CodeBlock(
				$"namespace {baseNamespace} {{",
				() => {
					writer.CodeBlock(
						$"public static class {className} {{",
						() => {
							writer.WriteLine( $@"public const int {commandTimeoutSecondsPropertyName} = {configuration.CommandTimeoutSecondsTyped?.ToString() ?? "null"};" );
						} );
				} );
		}

		internal static string GetCommandTimeoutSecondsExpression = $"{className}.{commandTimeoutSecondsPropertyName}";
	}
}