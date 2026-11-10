/**
 * F1 2020 Persistent Grid & Driver Modding CLI
 * Safe, reversible command-line interface for the patch engine.
 */

const fs = require('fs');
const path = require('path');
const patchEngine = fs.existsSync(path.join(__dirname, 'patch_engine.js'))
    ? require(path.join(__dirname, 'patch_engine.js'))
    : require(path.join(__dirname, '..', 'patch_engine.js'));

const BASELINE_CHECKSUMS = {
    dx11: 'c3b62b2f256645a3db7e4b1e5c7f384df2f6c86619aa2d1705ba9d4ea9760e97',
    dx12: '5fd6ac64158a5ffd2d001a4238365e4ac3749a496f150e42feea236dad1ee89d',
    db:   '3c140ef3bad0e20d537df7fe0bbb3f740693bd33acc89b06f2071d4560c39a7f'
};

function parseArgs() {
    const args = process.argv.slice(2);
    const parsed = { command: args[0] || 'help' };
    for (let i = 1; i < args.length; i++) {
        if (args[i].startsWith('--')) {
            const key = args[i].substring(2);
            const val = args[i + 1] && !args[i + 1].startsWith('--') ? args[++i] : true;
            parsed[key] = val;
        }
    }
    return parsed;
}

function copyFolderRecursive(src, dest) {
    fs.mkdirSync(dest, { recursive: true });
    for (const item of fs.readdirSync(src)) {
        const s = path.join(src, item);
        const d = path.join(dest, item);
        if (fs.statSync(s).isDirectory()) {
            copyFolderRecursive(s, d);
        } else {
            fs.copyFileSync(s, d);
        }
    }
}

function auditGameDir(gameDir) {
    const dx11Path = path.join(gameDir, 'F1_2020.exe');
    const dx12Path = path.join(gameDir, 'F1_2020_dx12.exe');
    const dbPath = path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin');
    const localisatioaPath = path.join(gameDir, 'localisatioa');
    const modModPath = path.join(gameDir, '_Modular_Mods', '_ACTIVE');

    const hasDx11 = fs.existsSync(dx11Path);
    const hasDx12 = fs.existsSync(dx12Path);
    const hasDb = fs.existsSync(dbPath);

    if (!hasDx11 && !hasDx12) {
        return {
            valid: false,
            message: 'No se encontraron ejecutables de F1 2020 (F1_2020.exe o F1_2020_dx12.exe).'
        };
    }
    if (!hasDb) {
        return {
            valid: false,
            message: 'No se encontró la base de datos (game_package/gamedatabases/gamedatabases.bin).'
        };
    }

    const dx11Hash = hasDx11 ? patchEngine.sha256(fs.readFileSync(dx11Path)) : null;
    const dx12Hash = hasDx12 ? patchEngine.sha256(fs.readFileSync(dx12Path)) : null;
    const dbHash = hasDb ? patchEngine.sha256(fs.readFileSync(dbPath)) : null;

    const dx11Patched = dx11Hash && dx11Hash !== BASELINE_CHECKSUMS.dx11;
    const dx12Patched = dx12Hash && dx12Hash !== BASELINE_CHECKSUMS.dx12;
    const dbPatched = dbHash && dbHash !== BASELINE_CHECKSUMS.db;
    const hasLocalisatioa = fs.existsSync(localisatioaPath);
    const hasModularMods = fs.existsSync(modModPath);

    const isPatched = dx11Patched || dx12Patched || dbPatched || hasLocalisatioa;

    return {
        valid: true,
        gameDir,
        isPatched,
        dx11Patched,
        dx12Patched,
        dbPatched,
        hasLocalisatioa,
        hasModularMods,
        dx11Hash,
        dx12Hash,
        dbHash
    };
}

function ensureBackups(gameDir, backupDir) {
    fs.mkdirSync(backupDir, { recursive: true });

    const dx11Dest = path.join(backupDir, 'F1_2020.exe');
    const dx12Dest = path.join(backupDir, 'F1_2020_dx12.exe');
    const dbDest = path.join(backupDir, 'gamedatabases.bin');

    const dx11Src = path.join(gameDir, 'F1_2020.exe');
    const dx12Src = path.join(gameDir, 'F1_2020_dx12.exe');
    const dbSrc = path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin');

    // Only copy if backup doesn't exist AND source is clean baseline OR backup is already known
    if (!fs.existsSync(dx11Dest) && fs.existsSync(dx11Src)) {
        fs.copyFileSync(dx11Src, dx11Dest);
        console.log(`[BACKUP] Creado respaldo de F1_2020.exe en ${dx11Dest}`);
    }
    if (!fs.existsSync(dx12Dest) && fs.existsSync(dx12Src)) {
        fs.copyFileSync(dx12Src, dx12Dest);
        console.log(`[BACKUP] Creado respaldo de F1_2020_dx12.exe en ${dx12Dest}`);
    }
    if (!fs.existsSync(dbDest) && fs.existsSync(dbSrc)) {
        fs.copyFileSync(dbSrc, dbDest);
        console.log(`[BACKUP] Creado respaldo de gamedatabases.bin en ${dbDest}`);
    }
}

async function main() {
    const opts = parseArgs();
    const gameDir = opts['game-dir'] ? path.resolve(opts['game-dir']) : path.resolve(__dirname, '..');
    const configPath = opts['config'] ? path.resolve(opts['config']) : path.join(__dirname, 'grid_2026.json');
    const rawLngDir = opts['raw-lng'] ? path.resolve(opts['raw-lng']) : path.join(__dirname, 'raw_languages');
    const backupDir = opts['backup-dir'] ? path.resolve(opts['backup-dir']) : path.join(gameDir, 'f1_2020_grid_changer_backups');

    if (opts.command === 'audit') {
        const audit = auditGameDir(gameDir);
        console.log(JSON.stringify(audit, null, 2));
        return;
    }

    if (opts.command === 'apply') {
        console.log(`=== APLICANDO MOD DE PARRILLA Y PILOTOS ===`);
        console.log(`Carpeta de destino: ${gameDir}`);
        console.log(`Configuración: ${configPath}`);

        const audit = auditGameDir(gameDir);
        if (!audit.valid) {
            console.error(`ERROR: ${audit.message}`);
            process.exit(1);
        }

        // 1. Backups
        ensureBackups(gameDir, backupDir);

        // 2. Stage patch
        const tempStage = path.join(gameDir, 'f1_2020_grid_changer_temp_stage');
        console.log(`[PARCHE] Generando archivos parcheados en directorio temporal...`);
        const gridConfig = JSON.parse(fs.readFileSync(configPath, 'utf8'));

        patchEngine.buildGridStagedPatch(gameDir, gridConfig, tempStage, rawLngDir);

        // 3. Apply files
        console.log(`[PARCHE] Instalando ejecutables redirigidos...`);
        if (fs.existsSync(path.join(tempStage, 'F1_2020.exe'))) {
            fs.copyFileSync(path.join(tempStage, 'F1_2020.exe'), path.join(gameDir, 'F1_2020.exe'));
        }
        if (fs.existsSync(path.join(tempStage, 'F1_2020_dx12.exe'))) {
            fs.copyFileSync(path.join(tempStage, 'F1_2020_dx12.exe'), path.join(gameDir, 'F1_2020_dx12.exe'));
        }

        console.log(`[PARCHE] Instalando base de datos modificada...`);
        const dbDest = path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin');
        fs.copyFileSync(path.join(tempStage, 'game_package', 'gamedatabases', 'gamedatabases.bin'), dbDest);

        console.log(`[PARCHE] Instalando archivos de localización modificados (localisatioa)...`);
        const locDest = path.join(gameDir, 'localisatioa');
        if (fs.existsSync(locDest)) {
            fs.rmSync(locDest, { recursive: true, force: true });
        }
        copyFolderRecursive(path.join(tempStage, 'localisatioa'), locDest);

        // Clean up temp stage
        fs.rmSync(tempStage, { recursive: true, force: true });

        console.log(`[ÉXITO] ¡Modificación de parrilla y pilotos aplicada exitosamente a ${gameDir}!`);
        return;
    }

    if (opts.command === 'rollback') {
        console.log(`=== RESTAURANDO JUEGO ORIGINAL (ROLLBACK) ===`);
        console.log(`Carpeta de destino: ${gameDir}`);

        const dx11Backup = path.join(backupDir, 'F1_2020.exe');
        const dx12Backup = path.join(backupDir, 'F1_2020_dx12.exe');
        const dbBackup = path.join(backupDir, 'gamedatabases.bin');

        if (!fs.existsSync(dx11Backup) && !fs.existsSync(dbBackup)) {
            // Also check default mod backups folder
            const defaultModBackup = path.join(__dirname, 'backups');
            if (fs.existsSync(path.join(defaultModBackup, 'F1_2020.exe'))) {
                console.log(`[ROLLBACK] Usando respaldos base de ${defaultModBackup}`);
                fs.copyFileSync(path.join(defaultModBackup, 'F1_2020.exe'), path.join(gameDir, 'F1_2020.exe'));
                fs.copyFileSync(path.join(defaultModBackup, 'F1_2020_dx12.exe'), path.join(gameDir, 'F1_2020_dx12.exe'));
                fs.copyFileSync(
                    path.join(defaultModBackup, 'gamedatabases.bin'),
                    path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin')
                );
            } else {
                console.error(`ERROR: No se encontraron archivos de respaldo en ${backupDir}`);
                process.exit(1);
            }
        } else {
            if (fs.existsSync(dx11Backup)) {
                fs.copyFileSync(dx11Backup, path.join(gameDir, 'F1_2020.exe'));
                console.log(`[ROLLBACK] Restaurado F1_2020.exe original.`);
            }
            if (fs.existsSync(dx12Backup)) {
                fs.copyFileSync(dx12Backup, path.join(gameDir, 'F1_2020_dx12.exe'));
                console.log(`[ROLLBACK] Restaurado F1_2020_dx12.exe original.`);
            }
            if (fs.existsSync(dbBackup)) {
                fs.copyFileSync(dbBackup, path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin'));
                console.log(`[ROLLBACK] Restaurado gamedatabases.bin original.`);
            }
        }

        const locDest = path.join(gameDir, 'localisatioa');
        if (fs.existsSync(locDest)) {
            fs.rmSync(locDest, { recursive: true, force: true });
            console.log(`[ROLLBACK] Eliminada carpeta temporal localisatioa.`);
        }

        console.log(`[ÉXITO] ¡Juego restaurado a su estado original limpio!`);
        return;
    }

    console.log(`Uso: node patch_cli.js <apply|rollback|audit> --game-dir <path> [--config <json>]`);
}

main().catch(err => {
    console.error(`ERROR FATAL:`, err);
    process.exit(1);
});
