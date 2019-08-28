How to get started with TypedDataLayer
- Create a 'TypedDataLayer' folder in the root of your project .
- Create a Configuration.xml in that folder.
- Copy the XML from the bottom of this file into Configuration.xml and replace the connection string and database type if neccesary.

Add this to your appsettings:
<add key="TypedDataLayer.SupportedDatabaseType" value="SqlServer" />
		<add key="TypedDataLayer.ConnectionString" value="Server=(local);Database=[database name];Trusted_Connection=True;" />
		these are used at runtime. the other configuration is only used at code-generation time.

In your Package Manager Console, you now have the Update-DataLayer command. This command does two things:
- If Database Updates.sql file is in the root of your project your project, Update-DataLayer will query the GlobalInts table in your database for 'LineMarker'. All of the lines after 
	LineMarker in the Database Updates.sql file will be executed against your database.
- Code will be generated and written to the GeneratedCode\TypedDataLayer.cs file.

After you have generated code successfully, [TableName]TableRetrieval classes will be available to you to retrieve data. [TableName]Modification classes are available to modify data.

To open a database connection and query data, use the Database static class.

Example Configuration.xml:
<?xml version="1.0" encoding="utf-8"  ?>
<systemDevelopmentConfiguration xmlns="http://samrueby.com" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                   xmlns:rs="http://samrueby.com" xsi:schemaLocation="http://samrueby.com ..\TypedDataLayerConfig.xsd">
<LibraryNamespaceAndAssemblyName>ExampleNamespace</LibraryNamespaceAndAssemblyName>
  <databaseConfiguration xsi:type="SqlServerDatabase">
    <ConnectionString>data source=(local);Integrated Security=SSPI;initial catalog=YourDatabase</ConnectionString>
  </databaseConfiguration>
  <database>
    <CommandTimeoutSeconds>30</CommandTimeoutSeconds>
  </database>
</systemDevelopmentConfiguration>