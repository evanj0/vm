$vmDir = "src/vm";
$asmDir = "src/asm";
$installDir = [IO.Path]::Combine("$Env:UserProfile", "vm");
$vmInstallDir = [IO.Path]::Combine($installDir, "vm.exe");
$asmInstallDir = [IO.Path]::Combine($installDir, "asm.exe");

try
{
    Set-Location -Path $PSScriptRoot;
    Write-Output "Publishing vm";
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $vmDir, "-c Release", "-r win10-x64", "-o ./publish/vm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait -NoNewWindow;
    Write-Output "Publishing asm";
    Start-Process -FilePath "dotnet" -ArgumentList @("publish", $asmDir, "-c Release", "-r win10-x64", "-o ./publish/asm", "-p:PublishSingleFile=true", "-p:PublishTrimmed=true") -Wait -NoNewWindow;
    Copy-Item "./publish/vm/vm.exe" -Destination $vmInstallDir;
    Write-Output "Installed to $vmInstallDir";
    Copy-Item "./publish/asm/asm.exe" -Destination $asmInstallDir;
    Write-Output "Installed to $asmInstallDir";
}
catch 
{
    throw;
}
finally
{
    Set-Location -Path $PSScriptRoot;
}