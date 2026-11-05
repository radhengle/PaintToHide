# Unity Setup

Target: Unity 2022.3.50f1 LTS.

1. Install Unity Hub and Unity 2022.3.50f1.
2. Include Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open the `UnityProject` directory in Unity Hub.
4. Allow Package Manager to resolve `Packages/manifest.json`.
5. Ensure URP is selected in Project Settings > Graphics and Quality.
6. Switch Build Settings to Android; use landscape orientation and package ID `com.radhengle.paintblend`.
7. Open Tools → Paint & Blend → Build Grand Hall Blockout.
8. Save the generated scene as `Assets/PaintBlend/Scenes/GrandHallComplex.unity`.
9. Confirm no compile errors before proceeding.
10. Run EditMode/PlayMode tests and inspect the world at character scale.

The procedural world builder establishes spatial layout and a first-pass prop blockout. It is not a replacement for bespoke Blender assets and final visual polish shown in the references.
