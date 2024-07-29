param (
    [Parameter(Mandatory=$true)]
    [string] $packageVersion
)

function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished`n" -ForegroundColor Yellow
}

function Publish-NugetPackage {
    param([string] $TestProject, [string] $Project, [string] $PackageName)

    Invoke-CommandWithLog -Command "dotnet test $TestProject -c Release --no-build" -CommandName "test"
    Invoke-CommandWithLog -Command "dotnet pack $Project --no-build -c Release --include-symbols -o artifacts -p:Version=$packageVersion" -CommandName "pack"

    Invoke-CommandWithLog -Command "dotnet nuget push $Project/artifacts/$PackageName.nupkg -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY" -CommandName "publish"
    Invoke-CommandWithLog -Command "dotnet nuget push $Project/artifacts/$PackageName.symbols.nupkg -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY" -CommandName "publish symbols"
}


$solutionPath = "src/PipelineNet.sln"

Write-Host "`nGit version tag: '$packageVersion'`n"
if ($packageVersion.StartsWith("v")) {
    $packageVersion = $packageVersion.Substring(1)
}

cd "src"

Write-Host "Package version: $packageVersion" -ForegroundColor Yellow

Invoke-CommandWithLog -Command "dotnet build $solutionPath -c Release -p:Version=$packageVersion" -CommandName "build"


Publish-NugetPackage -TestProject "PipelineNet.Tests/PipelineNet.Tests.csproj" -Project "PipelineNet/PipelineNet.csproj" -PackageName "PipelineNet.$packageVersion"
Publish-NugetPackage -TestProject "PipelineNet.ServiceProvider.Tests/PipelineNet.ServiceProvider.Tests.csproj" -Project "PipelineNet.ServiceProvider/PipelineNet.ServiceProvider.csproj" -PackageName "PipelineNet.ServiceProvider.$packageVersion"
