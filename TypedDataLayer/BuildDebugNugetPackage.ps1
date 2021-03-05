$now = Get-Date
$version = [string]::Format("{0}.{1}.{2}.{3}$OctoVersionSuffix",$now.Year, $now.Month, $now.Day,$now.Hour*10000+$now.Minute*100+$now.Second)

nuget.exe pack TypedDataLayer.nuspec -Prop Configuration=Debug -Version $version -Suffix alpha