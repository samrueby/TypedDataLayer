nuget.exe pack TypedDataLayer.csproj -Prop Configuration=Release
$package = Get-ChildItem TypedDataLayer.*.nupkg | sort LastWriteTime | select -last 1
nuget.exe push "$package" otmk0ypJrLjGOc^5Ln!LiD@x7!s1kviEucNx%*Y2*iUu8QBdCi*1c7wXQzS02Hn -Source https://testing.brossgroup.com/BrossNugetServer/api/v2/package