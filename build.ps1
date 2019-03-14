function Invoke-CommandWithLog {
    param([string] $Command, [string] $CommandName)
    Write-Host "----------" -ForegroundColor Yellow
    Write-Host "Starting $CommandName process" -ForegroundColor Yellow
    Invoke-Expression $Command
    Write-Host "$CommandName finished" -ForegroundColor Yellow
}

Invoke-CommandWithLog -Command "dotnet build src/PipelineNet.sln -c Release" -CommandName "build"

Invoke-CommandWithLog -Command "dotnet test src/PipelineNet.sln -c Release" -CommandName "test"