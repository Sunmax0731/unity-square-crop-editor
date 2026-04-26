# Validation Checklist

Use this checklist before publishing a release package.

## Automated

- [ ] Run EditMode tests.
- [ ] Confirm all tests pass.
- [ ] Confirm no compile errors in the Unity Console.

Suggested command:

```powershell
$unity = 'C:\Program Files\Unity\6000.4.0f1\Editor\Unity.exe'
& $unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Validation\editmode-results.xml -logFile Validation\unity-editmode.log
```

## Manual Smoke

- [ ] Open `Tools > Square Crop Editor > メイン画面`.
- [ ] Assign a transparent PNG source texture.
- [ ] Drag a square selection.
- [ ] Drag a non-square preset selection.
- [ ] Confirm selection outline follows the selected crop ratio.
- [ ] Change output ratio and confirm preview dimensions update.
- [ ] Check `Fit`.
- [ ] Check `Fill`.
- [ ] Check `Stretch`.
- [ ] Export to `Assets/Generated/SquareCrop`.
- [ ] Confirm exported PNG dimensions match output settings.
- [ ] Confirm alpha is preserved.
- [ ] Set source texture Read/Write disabled.
- [ ] Confirm export succeeds without permanently changing importer settings.

## Package Contents

- [ ] `README.md`
- [ ] `Manual.md`
- [ ] `Manual.ja.md`
- [ ] `TermsOfUse.md`
- [ ] `ReleaseNotes.md`
- [ ] `ValidationChecklist.md`
- [ ] `Samples~/TransparentIconSource`

## Release Packaging

- [ ] Run the release packaging script.
- [ ] Confirm the release zip name matches the package version.
- [ ] Confirm the release zip contains the UPM package folder.
- [ ] Confirm the release zip contains the unitypackage.
- [ ] Confirm the release zip contains `GitHubReleaseBody.ja.md`.
- [ ] Confirm the release zip contains `BOOTHDescription.ja.md`.

Suggested command:

```powershell
.\tools\release\New-ReleasePackage.ps1
```

## Known Limitations Confirmed

- [ ] Batch export is not listed as supported.
- [ ] Object detection is not listed as supported.
- [ ] Mask editing is not listed as supported.
- [ ] Atlas or grid slicing is not listed as supported.
- [ ] Session JSON and preset persistence are not listed as supported.
