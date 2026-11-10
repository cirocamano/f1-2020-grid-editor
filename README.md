# F1 2020 Grid & Driver Customizer

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10 / 8 WPF](https://img.shields.io/badge/.NET-Desktop_WPF-purple.svg)](https://dotnet.microsoft.com/)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows_10%2F11-0078D6.svg)](https://www.microsoft.com/)

A standalone, persistent, file-based modding suite for **Codemasters F1 2020**. Customizes driver names, surnames, 3-letter abbreviations, nationalities with in-game flags, team names, and constructor UI timing tower colors with **zero runtime memory injection, zero Cheat Engine, and zero background processes**.

---

## 🏎️ Key Features

- **100% Persistent & Standalone**: Changes persist across game launches, full PC restarts, and career sessions without needing any background trainer or memory hook.
- **Compact & Modern Dark UI**: Sleek, Formula 1-themed interface that fits comfortably on any screen resolution without taking over your desktop.
- **Multi-Language Support**: Fully localized in **English** (default), **Spanish (Español)**, and **Russian (Русский)** with instant one-click toggle.
- **Full 10-Team & 20-Driver Grid Customization**:
  - Edit driver first name, surname, and 3-letter initials.
  - 75 supported nationalities with accurate in-game flag mapping (Argentina, Brazil, France, Spain, Germany, UK, etc.).
  - Constructor display names, official titles, and short titles.
- **Accurate Team UI Colors**:
  - Set modern hex colors (`#RRGGBB`) with built-in color swatch preview and native Windows color picker dialog.
  - Updates timing towers, deltas, HUD accents, minimap, and team selection menus.
- **Preset Management**:
  - Includes ready-to-use **2026 Season Grid** (Hamilton @ Ferrari, Colapinto @ Alpine, Bortoleto @ Sauber, Hadjar @ Racing Bulls, Russell & Antonelli @ Mercedes, etc.).
  - Includes **Stock 2020 Grid** for instant restoration.
  - Export and import custom grids as JSON presets.
- **Safe & Fully Reversible**:
  - Creates automatic SHA-256-verified backups before applying modifications.
  - One-click rollback button to restore 100% clean original game files.
- **Modular Mods Compatible**: Works seamlessly with Modular Mods, custom liveries, helmets, and season visual overhauls.

---

## 📦 Directory Structure

```
F1-2020-Grid-Customizer/
├── src/                    # C# WPF Desktop GUI and Node.js Patch Engine source code
│   ├── F1GridEditorApp.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / MainWindow.xaml.cs
│   ├── Models.cs
│   ├── Localization.cs
│   ├── patch_engine.js
│   └── patch_cli.js
├── presets/                # Season roster configurations (JSON)
│   ├── grid_2026.json
│   └── grid_stock_2020.json
├── release/                # Ready-to-run standalone desktop binaries
│   ├── Iniciar_Editor.bat
│   ├── F1_2020_Grid_Editor.exe
│   ├── F1_2020_Grid_Editor.dll
│   ├── F1_2020_Grid_Editor.runtimeconfig.json
│   ├── F1_2020_Grid_Editor.deps.json
│   ├── patch_engine.js
│   ├── patch_cli.js
│   └── presets/
├── CONTRIBUTING.md         # Contribution and preset guidelines
├── LICENSE                 # MIT License
└── README.md               # Project documentation
```

> [!NOTE]
> **No Copyrighted Game Files**: This repository does not contain proprietary game files (`F1_2020.exe`, `game.dat`, `gamedatabases.bin`, or game archives). The tool operates directly on the user's legally acquired installation of F1 2020.

---

## 🚀 Quick Start (Pre-Built Binaries)

1. Navigate to the `release/` folder.
2. Launch `Iniciar_Editor.bat` or run `F1_2020_Grid_Editor.exe`.
3. Click **Browse...** to select your F1 2020 root folder (or click **Downloads / Clean** if testing on a clean copy).
4. Select a team from the left sidebar to edit names, initials, nationalities, and colors, or click **⭐ 2026 Grid** to load the modern preset.
5. Click **🚀 APPLY TO GAME**. The tool creates automatic backups and applies the patch.
6. Launch F1 2020 normally through Steam or your standard executable.

To revert to stock at any time, simply click **↩️ RESTORE STOCK (Rollback)**.

---

## 🛠️ Building from Source

### Prerequisites
- Windows 10 (1809+) or Windows 11
- [.NET 8.0 SDK or .NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+) for CLI engine script execution

### Build Instructions
```powershell
# Clone the repository
git clone https://github.com/your-username/F1-2020-Grid-Customizer.git
cd F1-2020-Grid-Customizer

# Build the WPF Desktop App
dotnet build src/F1GridEditorApp.csproj -c Release

# Publish to release folder
dotnet publish src/F1GridEditorApp.csproj -c Release -o release/
```

---

## 🔬 How It Works (Technical Overview)

1. **Persistent Executable Redirection**:
   - `F1_2020.exe` (DX11, offset `0x1bfb096`) and `F1_2020_dx12.exe` (DX12, offset `0x1b9e022`): A single-byte patch changes the hardcoded folder string from `localisation/` to `localisatioa/`.
   - This redirects the engine to load modified localization `.lng` files as loose files on disk without touching the read-only encrypted `game.dat` archive.
2. **Database Record Patching (`gamedatabases.bin`)**:
   - Decrypted and re-encrypted using the static Codemasters XOR stream key:
     `PxA6v23AL0INY7sA1Ab701AA2A44c7`
   - Maps 64-bit FNV-1a hashes for driver nationalities (`entrants.drivers.driver_{id}.nationality`), abbreviations (`initials`), and constructor timing tower colors (`entrants.teams.team_{id}.ui_team_colour` & `primary_team_colour`).
3. **Binary Language Compilation (`LNGT`)**:
   - Reconstructs binary LNG headers, bucket counts, and string offset tables (`HSHS`, `HSHT`, `SIDA`, `SIDB`, `LNGB`).
   - Computes DJB2 hashes (`seed = 5381`, `multiplier = 33`, `buckets = 17528`) ensuring 100% crash-free dictionary lookup by the game engine.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
F1 2020 is a trademark of The Formula One Licensing B.V. and Electronic Arts Inc. / Codemasters. This open-source utility is intended for personal and educational modding purposes only.

