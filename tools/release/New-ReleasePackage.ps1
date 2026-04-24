[CmdletBinding()]
param(
    [string]$ProjectPath = '',
    [string]$UnityPath = 'C:\Program Files\Unity\6000.4.0f1\Editor\Unity.exe',
    [switch]$SkipValidation
)

$ErrorActionPreference = 'Stop'

function Invoke-Unity {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$StepName
    )

    $process = Start-Process -FilePath $UnityPath -ArgumentList $Arguments -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        throw "$StepName failed with exit code $($process.ExitCode)."
    }
}

function Assert-TestResultsPassed {
    param([Parameter(Mandatory = $true)][string]$ResultsPath)

    if (-not (Test-Path -LiteralPath $ResultsPath)) {
        throw "Unity test results were not created: $ResultsPath"
    }

    [xml]$results = Get-Content -LiteralPath $ResultsPath -Raw
    $testRun = $results.'test-run'
    if ($testRun.result -ne 'Passed' -or [int]$testRun.failed -ne 0) {
        throw "Unity EditMode tests did not pass. result=$($testRun.result), failed=$($testRun.failed)"
    }
}

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = Join-Path $PSScriptRoot '..\..'
}

$ProjectPath = (Resolve-Path $ProjectPath).Path
if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity executable was not found: $UnityPath"
}

$packageRoot = Join-Path $ProjectPath 'Packages\com.sunmax0731.square-crop-editor'
$packageJsonPath = Join-Path $packageRoot 'package.json'
$packageJson = Get-Content -LiteralPath $packageJsonPath -Raw | ConvertFrom-Json
$version = $packageJson.version
$packageName = $packageJson.name
$artifactName = "UnitySquareCropEditor-v$version"
$releaseRoot = Join-Path $ProjectPath "ReleaseBuilds\v$version"
$stagingRoot = Join-Path $releaseRoot 'zip-staging'
$validationRoot = Join-Path $ProjectPath 'Validation'
$testResultsPath = Join-Path $validationRoot 'editmode-results.xml'
$testLogPath = Join-Path $validationRoot 'unity-editmode.log'
$unityPackagePath = Join-Path $releaseRoot "$artifactName.unitypackage"
$unityPackageLogPath = Join-Path $validationRoot 'unitypackage-export.log'
$zipPath = Join-Path $releaseRoot "$artifactName.zip"

New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
New-Item -ItemType Directory -Path $validationRoot -Force | Out-Null

if (-not $SkipValidation) {
    Remove-Item -LiteralPath $testResultsPath -ErrorAction SilentlyContinue
    Invoke-Unity -StepName 'EditMode validation' -Arguments @(
        '-batchmode',
        '-projectPath', $ProjectPath,
        '-runTests',
        '-testPlatform', 'EditMode',
        '-assemblyNames', 'Sunmax0731.SquareCropEditor.Editor.Tests',
        '-testResults', $testResultsPath,
        '-logFile', $testLogPath
    )
    Assert-TestResultsPassed -ResultsPath $testResultsPath
}

Remove-Item -LiteralPath $unityPackagePath -ErrorAction SilentlyContinue
Invoke-Unity -StepName 'Unity package export' -Arguments @(
    '-batchmode',
    '-quit',
    '-projectPath', $ProjectPath,
    '-executeMethod', 'Sunmax0731.SquareCropEditor.Editor.Release.UnityPackageExporter.ExportFromCommandLine',
    '-squareCropUnityPackageOutput', $unityPackagePath,
    '-logFile', $unityPackageLogPath
)

if (-not (Test-Path -LiteralPath $unityPackagePath)) {
    throw "Unity package was not created: $unityPackagePath"
}

Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

$packageDestination = Join-Path $stagingRoot $packageName
Copy-Item -LiteralPath $packageRoot -Destination $packageDestination -Recurse
Copy-Item -LiteralPath $unityPackagePath -Destination (Join-Path $stagingRoot (Split-Path $unityPackagePath -Leaf))
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'GitHubReleaseBody.ja.md') -Destination (Join-Path $stagingRoot 'GitHubReleaseBody.ja.md')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'BOOTHDescription.ja.md') -Destination (Join-Path $stagingRoot 'BOOTHDescription.ja.md')

Remove-Item -LiteralPath $zipPath -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $zipPath -Force

& (Join-Path $PSScriptRoot 'Test-ReleasePackage.ps1') -ZipPath $zipPath -PackageName $packageName -Version $version

Write-Host "Release artifacts created:"
Write-Host "  $zipPath"
Write-Host "  $unityPackagePath"
