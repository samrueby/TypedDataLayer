How to get started with TypedDataLayer
- You will find an example configuration file in the Examples folder. Rename 'Examples' to 'TypedDataLayer' and set the values in this file to use TDL.
- In your Package Manager Console, you now have the Update-DataLayer command. This command does two things:
	- If Database Updates.sql file is in the root of your project your project, Update-DataLayer will query the GlobalInts table in your database for 'LineMarker'. 
	  All of the lines after LineMarker in the Database Updates.sql file will be executed against your database.
	- Code will be generated and written to the GeneratedCode\TypedDataLayer.cs file.

After you have generated code successfully, [TableName]TableRetrieval classes will be available to you to retrieve data. [TableName]Modification classes are available to modify data.

To open a database connection and query data, use:
new DataAccessState().ExecuteWithThis( () => DataAccessState.Current.PrimaryDatabaseConnection.ExecuteWithConnectionOpen( method ) )