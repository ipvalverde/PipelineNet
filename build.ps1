function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished" -ForegroundColor Yellow
    Write-Host ""
}

$mainProjectPath = "src/PipelineNet/PipelineNet.csproj"
$testProjectPath = "src/PipelineNet.Tests/PipelineNet.Tests.csproj"
$solutionPath = "src/PipelineNet.sln"

$commitMessage = $null
$packageVersion = $null

if ($env:APPVEYOR_REPO_TAG -eq "true") {

    $packageVersion = $env:APPVEYOR_REPO_TAG_NAME
    Write-Host "Git version tag detected: '$packageVersion'"

    $commitMessage = $env:APPVEYOR_REPO_COMMIT_MESSAGE
    if (-not [string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED)) {
        $commitMessage = $commitMessage + "`n" + $env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED
    }
}


Invoke-CommandWithLog -Command "dotnet build $solutionPath -p:Version=$packageVersion -c Release" -CommandName "build"

Invoke-CommandWithLog -Command "dotnet test $testProjectPath -c Release --no-build" -CommandName "test"


if ($null -ne $packageVersion) {
    Invoke-CommandWithLog -Command "dotnet pack $mainProjectPath --no-build -p:Version=$packageVersion -c Release --include-symbols -o artifacts -p:PackageReleaseNotes=`"$commitMessage`"" -CommandName "pack"
}