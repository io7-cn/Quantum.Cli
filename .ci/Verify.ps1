$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-Location (Split-Path $PSScriptRoot -Parent)
dotnet restore Quantum.Cli.slnx --locked-mode
dotnet format Quantum.Cli.slnx --severity info --no-restore --verify-no-changes
dotnet test Quantum.Cli.slnx -c Release --no-restore
dotnet pack src/Quantum.Cli/Quantum.Cli.csproj -c Release --no-restore -p:DebugType=none -p:DebugSymbols=false -o artifacts/packages
python .ci/check-package.py
[xml]$project = Get-Content src/Quantum.Cli/Quantum.Cli.csproj
$version = $project.Project.PropertyGroup.Version
$source = [Security.SecurityElement]::Escape((Resolve-Path artifacts/packages).Path)
"<configuration><packageSources><clear/><add key='local' value='$source'/></packageSources></configuration>" | Set-Content artifacts/nuget-local.config
$toolPath = Join-Path $PWD 'artifacts/tool'
$commandName = if ($IsWindows) { 'quantum-cli.exe' } else { 'quantum-cli' }
$command = Join-Path $toolPath $commandName
if (Test-Path -LiteralPath $command) { dotnet tool uninstall Quantum.Platform.Cli --tool-path $toolPath }
dotnet tool install Quantum.Platform.Cli --version $version --tool-path $toolPath --configfile artifacts/nuget-local.config
& $command --help
$actualVersion = & $command --version
if ($actualVersion.Trim() -ne $version) { throw 'The installed package version differs from the source version.' }
Write-Output "Verified installed quantum-cli $actualVersion"
