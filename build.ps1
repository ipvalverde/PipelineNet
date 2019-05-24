function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished`n" -ForegroundColor Yellow
}

$nugetSourceUrl = "https://www.myget.org/F/pipelinenet/api/v2/package"
$mainProjectPath = "src/PipelineNet/PipelineNet.csproj"
$testProjectPath = "src/PipelineNet.Tests/PipelineNet.Tests.csproj"
$solutionPath = "src/PipelineNet.sln"

$commitMessage = $null
$packageVersionCommandArgument = [string]::Empty

if ($env:APPVEYOR_REPO_TAG -eq "true") {

    Write-Host "Git version tag detected: '$env:APPVEYOR_REPO_TAG_NAME'"

    $packageVersionCommandArgument = " -p:Version=$env:APPVEYOR_REPO_TAG_NAME"

    $commitMessage = $env:APPVEYOR_REPO_COMMIT_MESSAGE
    if (-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED)) {
        $commitMessage = $commitMessage + "`n" + $env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED
    }
}


Invoke-CommandWithLog -Command "dotnet build $solutionPath -c Release$packageVersionCommandArgument" -CommandName "build"

Invoke-CommandWithLog -Command "dotnet test $testProjectPath -c Release --no-build" -CommandName "test"


if (-not [string]::IsNullOrWhiteSpace($packageVersionCommandArgument)) {
    Invoke-CommandWithLog -Command "dotnet pack $mainProjectPath --no-build -c Release --include-symbols -o artifacts -p:PackageReleaseNotes=`"$commitMessage`"$packageVersionCommandArgument" -CommandName "pack"

    Invoke-CommandWithLog -Command "dotnet nuget push artifacts/*.nupkg -s $nugetSourceUrl -k $env:MYGET_KEY" -CommandName "publish"
}
