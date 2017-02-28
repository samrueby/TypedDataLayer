#$installPath path to where the project is installed
#$toolsPath path to the extracted tools directory
#$package information about the currently installing package
#$project reference to the EnvDTE project the package is being installed into

param($installPath, $toolsPath, $package, $project)

New-Module -ScriptBlock {
$installPath = $args[0]

function Update-DataLayer {
	[CmdletBinding()]
	Param()
	Process {
		& "$installPath\CommandRunner\TypedDataLayer.exe" $installPath\..\.. UpdateAllDependentLogic
	}
}

function Update-DataLayerDebug {
	[CmdletBinding()]
	Param()
	Process {
		& "$installPath\CommandRunner\TypedDataLayer.exe" $installPath\..\.. UpdateAllDependentLogic -debug
	}
}

function Update-DataLayerAttach {
	[CmdletBinding()]
	Param()
	Process {
		& "$installPath\CommandRunner\TypedDataLayer.exe" $installPath\..\.. UpdateAllDependentLogic -attachDebugger
	}
}

} -ArgumentList $installPath