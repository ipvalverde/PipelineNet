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

    $TestProjectPath = "$TestProject/$TestProject.csproj"
    $ProjectPath = "$Project/$Project.csproj"

    Invoke-CommandWithLog -Command "dotnet test $TestProjectPath -c Release --no-build" -CommandName "test"
    Invoke-CommandWithLog -Command "dotnet pack $ProjectPath --no-build -c Release --include-symbols -o artifacts -p:Version=$packageVersion" -CommandName "pack"


    Invoke-CommandWithLog -Command "dotnet nuget push artifacts/$PackageName.nupkg -s $($Env:NUGET_SOURCE) -k $($Env:NUGET_API_KEY)" -CommandName "publish"
    Invoke-CommandWithLog -Command "dotnet nuget push artifacts/$PackageName.symbols.nupkg -s $($Env:NUGET_SOURCE) -k $($Env:NUGET_API_KEY)" -CommandName "publish symbols"
}

$solutionPath = "PipelineNet.sln"

Write-Host "`nGit version tag: '$packageVersion'`n"
if ($packageVersion.StartsWith("v")) {
    $packageVersion = $packageVersion.Substring(1)
}

Set-Location -Path "src" -PassThru

Write-Host "Package version: $packageVersion" -ForegroundColor Yellow

Invoke-CommandWithLog -Command "dotnet build $solutionPath -c Release -p:Version=$packageVersion" -CommandName "build"


Publish-NugetPackage -TestProject "PipelineNet.Tests" -Project "PipelineNet" -PackageName "PipelineNet.$packageVersion"
Publish-NugetPackage -TestProject "PipelineNet.ServiceProvider.Tests" -Project "PipelineNet.ServiceProvider" -PackageName "PipelineNet.ServiceProvider.$packageVersion"
