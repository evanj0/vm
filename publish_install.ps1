$vmDir = "src/vm";
$asmDir = "src/asm";
$installDir = [IO.Path]::Combine("$Env:UserProfile", "vm");

try
{
    Set-Location -Path $PSScriptRoot;
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $vmDir, "-c Release", "-r win10-x64", "-o ./publish/vm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait;
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $asmDir, "-c Release", "-r win10-x64", "-o ./publish/asm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait;
    Copy-Item -Path "./publish/vm/vm.exe" -Destination $installDir;
    Copy-Item -Path "./publish/asm/asm.exe" -Destination $installDir;
}
catch 
{
    throw;
}
finally
{
    Set-Location -Path $PSScriptRoot;
}