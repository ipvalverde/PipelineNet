function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished`n" -ForegroundColor Yellow
}

$projectName = "PipelineNet"
$nugetSourceUrl = "https://www.myget.org/F/pipelinenet/api/v2/package"
$mainProjectDirectory = "src/PipelineNet"
$mainProjectPath = "$mainProjectDirectory/$projectName.csproj"
$testProjectPath = "src/PipelineNet.Tests/PipelineNet.Tests.csproj"
$solutionPath = "src/PipelineNet.sln"


$commitMessage = $null
$packageVersionCommandArgument = [string]::Empty
$packageVersion = $null

if ($env:APPVEYOR_REPO_TAG -eq "true") {

    Write-Host "`nGit version tag detected: '$env:APPVEYOR_REPO_TAG_NAME'`n"

    if ($env:APPVEYOR_REPO_TAG_NAME.StartsWith("v")) {
        $packageVersion = $env:APPVEYOR_REPO_TAG_NAME.Substring(1)
    }
    else {
        $packageVersion = $env:APPVEYOR_REPO_TAG_NAME
    }

    $packageVersionCommandArgument = " -p:Version=$packageVersion"

    $commitMessage = $env:APPVEYOR_REPO_COMMIT_MESSAGE
    if (-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED)) {
        $commitMessage = $commitMessage + "`n" + $env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED
    }
}


Invoke-CommandWithLog -Command "dotnet build $solutionPath -c Release$packageVersionCommandArgument" -CommandName "build"

Invoke-CommandWithLog -Command "dotnet test $testProjectPath -c Release --no-build" -CommandName "test"


if (-not [string]::IsNullOrWhiteSpace($packageVersionCommandArgument)) {
    Invoke-CommandWithLog -Command "dotnet pack $mainProjectPath --no-build -c Release --include-symbols -o artifacts -p:PackageReleaseNotes=`"$commitMessage`"$packageVersionCommandArgument" -CommandName "pack"

    $nugetPackageName = "$projectName.$packageVersion"
    Invoke-CommandWithLog -Command "dotnet nuget push $mainProjectDirectory/artifacts/$nugetPackageName.nupkg -s $nugetSourceUrl -k $env:MYGET_KEY" -CommandName "publish"
    Invoke-CommandWithLog -Command "dotnet nuget push $mainProjectDirectory/artifacts/$nugetPackageName.symbols.nupkg -s $nugetSourceUrl -k $env:MYGET_KEY" -CommandName "publish symbols"
}
