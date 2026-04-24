[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ZipPath,
    [Parameter(Mandatory = $true)]
    [string]$PackageName,
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ZipPath)) {
    throw "Release zip was not found: $ZipPath"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $ZipPath).Path)
try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    $artifactName = "UnitySquareCropEditor-v$Version"
    $requiredEntries = @(
        "$artifactName.unitypackage",
        "$PackageName/package.json",
        "$PackageName/README.md",
        "$PackageName/Manual.md",
        "$PackageName/Manual.ja.md",
        "$PackageName/TermsOfUse.md",
        "$PackageName/ReleaseNotes.md",
        "$PackageName/ValidationChecklist.md",
        "$PackageName/Samples~/TransparentIconSource/README.md",
        "$PackageName/Samples~/TransparentIconSource/square-crop-sample.png",
        'GitHubReleaseBody.ja.md',
        'BOOTHDescription.ja.md'
    )

    foreach ($entry in $requiredEntries) {
        if ($entries -notcontains $entry) {
            throw "Release zip is missing required entry: $entry"
        }
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Release zip contents validated: $ZipPath"
