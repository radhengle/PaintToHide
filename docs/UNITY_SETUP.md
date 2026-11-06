# Unity Setup and Build

Target editor: **Unity 2022.3.50f1 LTS**.

## Local development

1. Install Unity Hub and Unity 2022.3.50f1.
2. Include Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open the `UnityProject` directory in Unity Hub.
4. Allow Package Manager to resolve `Packages/manifest.json`.
5. Confirm URP is selected in Project Settings > Graphics and Quality.
6. Set the platform to Android, landscape orientation and package ID `com.radhengle.paintblend`.
7. Open **Tools → Paint & Blend → Build Grand Hall Blockout** to generate the editable connected-world foundation scene at `Assets/PaintBlend/Scenes/GrandHallComplex_Blockout.unity`.
8. Resolve all Unity Console errors before continuing.
9. Run EditMode and PlayMode tests and inspect the scene at the 1.75m character scale.

## Command-line Android APK

The repository includes the batch-mode entry point `PaintBlend.Editor.PaintBlendBuildPipeline.BuildAndroid`. It generates the Grand Hall blockout scene and builds an ARM64 IL2CPP APK.

Windows PowerShell example:

```powershell
$env:PAINTBLEND_BUILD_PATH = (Join-Path $PWD "Build/Android/PaintAndBlend.apk")
& "C:\Program Files\Unity\Hub\Editor\2022.3.50f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "$PWD/UnityProject" `
  -executeMethod PaintBlend.Editor.PaintBlendBuildPipeline.BuildAndroid `
  -logFile "$PWD/Build/AndroidBuild.log"
```

The machine must have a valid Unity license and Android Build Support installed. Inspect the log and confirm the APK exists before treating the build as successful.

## GitHub Actions Android build

The workflow `.github/workflows/unity-android.yml` runs Unity EditMode tests and creates an Android APK when repository configuration is enabled:

- Add repository Actions secret `UNITY_LICENSE` containing the Unity license file contents accepted by GameCI.
- Add repository Actions variable `UNITY_ANDROID_BUILD_ENABLED` with value `true`.
- Push to `feature/game-foundation`, open/update a PR to `main`, or run the workflow manually.
- Download the `paint-blend-android-apk` artifact only after the Unity build job succeeds.

If the variable is not enabled, Unity jobs are intentionally skipped; the license is not available to this environment, so a Unity compile or APK must not be claimed from static CI alone.

## Scope and quality gate

The generated world is an editable first-pass architectural/prop blockout. It is not finished reference-matched production art. Bespoke Blender assets, UV/material QA, rigging, animation, VFX, final lighting, gameplay integration, multiplayer and physical-device QA remain release requirements. A successful APK build is an engineering milestone, not proof that the full game is complete.
