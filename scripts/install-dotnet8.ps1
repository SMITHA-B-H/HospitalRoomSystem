<#
PowerShell script to install .NET 8 SDK for the current user using the official dotnet-install script.
Run in an elevated or normal PowerShell session:
  .\scripts\install-dotnet8.ps1
This installs into $env:USERPROFILE\.dotnet (no admin required). After install, restart your terminal/Visual Studio.
#>

param(
    [ValidateSet('sdk','runtime')]
    [string]$InstallType = 'sdk'
)

$installDir = Join-Path $env:USERPROFILE '.dotnet'
$tempDir = [System.IO.Path]::GetTempPath()
$installer = Join-Path $tempDir 'dotnet-install.ps1'

Write-Host "Checking for existing .NET 8 installation..." -ForegroundColor Cyan
$has8 = $false
try {
    $sdks = & dotnet --list-sdks 2>$null
    if ($sdks) {
        if ($sdks -match '^8\.') { $has8 = $true }
    }
} catch {}

try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes) {
        if ($runtimes -match '^Microsoft.NETCore.App\s+8\.') { $has8 = $true }
    }
} catch {}

if ($has8) {
    Write-Host "A .NET 8 SDK/runtime already appears to be installed on this machine." -ForegroundColor Green
    Write-Host "Use 'dotnet --list-sdks' and 'dotnet --list-runtimes' to inspect." -ForegroundColor Yellow
    return
}

Write-Host "Downloading dotnet-install script..." -ForegroundColor Cyan
$uri = 'https://dot.net/v1/dotnet-install.ps1'
Invoke-WebRequest -Uri $uri -OutFile $installer -UseBasicParsing

Write-Host "Installing .NET 8 ($InstallType) to: $installDir" -ForegroundColor Cyan
if ($InstallType -eq 'sdk') {
    & powershell -NoProfile -ExecutionPolicy Bypass -File $installer -Channel 8.0 -InstallDir $installDir
} else {
    # runtime (default core runtime)
    & powershell -NoProfile -ExecutionPolicy Bypass -File $installer -Channel 8.0 -Runtime dotnet -InstallDir $installDir
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet-install script failed (exit $LASTEXITCODE)."; exit $LASTEXITCODE
}

# Add to current session PATH
$env:PATH = "$installDir;$installDir\tools;" + $env:PATH

Write-Host "Verifying installation..." -ForegroundColor Cyan
try {
    & dotnet --info
} catch {
    Write-Warning "dotnet command not found in current session. You may need to restart your terminal or add $installDir to your PATH.";
}

Write-Host "Installation finished. Restart your terminal and Visual Studio to pick up the new .NET SDK." -ForegroundColor Green
Write-Host "If you use Visual Studio, open Tools -> Options -> Projects and Solutions -> .NET Core SDKs and check detection after restart." -ForegroundColor Yellow
