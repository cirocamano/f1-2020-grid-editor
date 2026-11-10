# Contributing to F1 2020 Grid & Driver Customizer

Thank you for your interest in contributing to the **F1 2020 Grid & Driver Customizer**!

## Scope and Principles
1. **No Copyrighted Game Files**: Never submit pull requests that include game executables (`F1_2020.exe`), game archives (`game.dat`), raw decrypted database binaries, or proprietary sound/video/texture packages.
2. **Persistence Without Injection**: The project strictly uses data-driven file redirection and database record modifications. No RAM hooks, DLL injections, Cheat Engine tables, or pre-launch trainers.
3. **Reversibility**: Any modification must support 100% clean rollback to the original stock baseline without requiring game reinstallation.

## How to Contribute

### 1. Adding Season Presets
You can create and submit new grid presets in `presets/<season_name>.json`:
- Follow the schema in `presets/grid_2026.json`.
- Provide driver names, 3-letter abbreviations, supported nationality IDs, official constructor titles, and hex team colors.
- Verify nationality IDs against `NationalitiesData` in `src/Models.cs`.

### 2. Enhancing the Desktop Application
The GUI is built with C# and WPF on .NET 8 / 10 (`Microsoft.NET.Sdk.WindowsDesktop`):
- Run `dotnet build src/F1GridEditorApp.csproj` to compile.
- Maintain compact and responsive UI layout.
- Ensure all new user-facing strings are added to `src/Localization.cs` across all supported languages (English, Spanish, Russian).

### 3. Reporting Issues
When reporting a bug, please include:
- Windows version (e.g. Windows 10 22H2 / Windows 11).
- DirectX mode used (DX11 or DX12).
- Whether Modular Mods or other visual season mods are installed.
- Console log output from the bottom console box.

