$vmDir = "src/vm";
$asmDir = "src/asm";

try
{
    Set-Location -Path $PSScriptRoot;
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $vmDir, "-c Release", "-r win10-x64", "-o ./publish/vm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait;
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $vmDir, "-c Release", "-r win10-x64", "-o ./publish/vm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait;
}
catch 
{
    throw;
}
finally
{
    Set-Location -Path $PSScriptRoot;
}