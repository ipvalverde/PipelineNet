param (
    [Parameter(Mandatory=$true)]
    [string] $packageVersion,

    [Parameter(Mandatory=$true)]
    [string] $releaseNotes,

    [Parameter(Mandatory=$true)]
    [string] $isPreRelease
)

function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished`n" -ForegroundColor Yellow
}

$projectName = "PipelineNet"
$mainProjectDirectory = "src/PipelineNet"
$mainProjectPath = "$mainProjectDirectory/$projectName.csproj"
$testProjectPath = "src/PipelineNet.Tests/PipelineNet.Tests.csproj"
$solutionPath = "src/PipelineNet.sln"


Write-Host "`nGit version tag: '$packageVersion'`n"
if ($packageVersion.StartsWith("v")) {
    $packageVersion = $packageVersion.Substring(1)
}
if ([System.Convert]::ToBoolean($isPreRelease)) {
    $packageVersion += "-alpha"
}

Write-Host "Package version: $packageVersion" -ForegroundColor Yellow

Invoke-CommandWithLog -Command "dotnet build $solutionPath -c Release$packageVersionCommandArgument" -CommandName "build"
Invoke-CommandWithLog -Command "dotnet test $testProjectPath -c Release --no-build" -CommandName "test"


Invoke-CommandWithLog -Command "dotnet pack $mainProjectPath --no-build -c Release --include-symbols -o artifacts -p:PackageReleaseNotes=`"$releaseNotes`" -p:Version=$packageVersion" -CommandName "pack"

$nugetPackageName = "$projectName.$packageVersion"
Invoke-CommandWithLog -Command "dotnet nuget push $mainProjectDirectory/artifacts/$nugetPackageName.nupkg -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY" -CommandName "publish"
Invoke-CommandWithLog -Command "dotnet nuget push $mainProjectDirectory/artifacts/$nugetPackageName.symbols.nupkg -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY" -CommandName "publish symbols"
