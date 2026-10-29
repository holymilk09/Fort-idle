# Compile check (no Unity needed)

Compiles `Assets/FortIdle/Scripts` with the .NET 8 SDK against Unity's 2021.3 reference
assemblies (NuGet `UnityEngine.Modules`) plus small stubs for uGUI and URP in `Stubs/`.

```bash
Tools/CompileCheck/check.sh
```

Prints `COMPILE CHECK PASSED` or the errors and exits non-zero.

- It catches syntax, type and API-shape mistakes. It does not run the game and does not replace
  Play Mode QA (`docs/QA.md`).
- Editor scripts (`Assets/FortIdle/Editor`) are not checked.
- A stub missing a real uGUI/URP member: add it to `Stubs/` with the real signature.
- A Unity 6 API missing from the 2021.3 references: add the error text to `unity6-only.txt`
  after confirming the API exists in Unity 6000.3.
- Install the SDK on Ubuntu with `apt-get install dotnet-sdk-8.0`.
