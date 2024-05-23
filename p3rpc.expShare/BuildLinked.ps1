# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p3rpc.expShare/*" -Force -Recurse
dotnet publish "./p3rpc.expShare.csproj" -c Release -o "$env:RELOADEDIIMODS/p3rpc.expShare" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location