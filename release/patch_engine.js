/**
 * F1 2020 Persistent Grid Patch Engine
 * Reversible, data-driven file modifier for driver identities and nationalities.
 */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const DB_KEY = Buffer.from('PxA6v23AL0INY7sA1Ab701AA2A44c7', 'ascii');

const TEAM_COLOR_KEYS = {
    forceindia: [
        0x5cd4c59136a1e9c7n, // entrants.teams.team_4.ui_team_colour
        0x4ddda7e10aa91083n, // entrants.teams.team_4.primary_team_colour
        0x401f90ff60cd08e7n, // entrants.teams.team_4.secondary_team_colour
        0x0374e1c2c9a82be2n  // legacy key
    ],
    renault: [
        0x5371775b150bcdden, // entrants.teams.team_5.ui_team_colour
        0x4fa6592c0a265c44n, // entrants.teams.team_5.primary_team_colour
        0xe275484c99669944n, // entrants.teams.team_5.secondary_team_colour
        0x01cc562a3ab61f13n, // legacy key 1
        0x66b294c2765bc79an, // legacy key 2
        0xdc19153d8a1f8a0en  // legacy key 3
    ],
    tororosso: [
        0x2fbdead0143241a1n, // entrants.teams.team_6.ui_team_colour
        0xc236fe74a82ca569n, // entrants.teams.team_6.primary_team_colour
        0x339c4d5dd07d12b9n, // entrants.teams.team_6.secondary_team_colour
        0x03abe835cf460136n, // legacy key 1
        0x4ac7cf194cf27d47n  // legacy key 2
    ],
    sauber: [
        0xde173352b4c4d1ban, // entrants.teams.team_9.ui_team_colour
        0x185eafaa4f790bc8n, // entrants.teams.team_9.primary_team_colour
        0xe9c02c9356423520n, // entrants.teams.team_9.secondary_team_colour
        0x0484ed9c74fe55a0n, // legacy key 1
        0x7d9d4de784c29dc3n, // legacy key 2
        0xbc18e039e18d92b8n, // legacy key 3
        0xc1c264c0147c6b5an, // legacy key 4
        0xe7fabffc7e78b895n  // legacy key 5
    ]
};

function hexToColorInt32Str(hex) {
    let clean = (hex || '').replace('#', '').trim();
    if (!clean) return null;
    if (clean.length === 6) clean = 'ff' + clean;
    return ((parseInt(clean, 16) | 0)).toString(10);
}

function colorInt32StrToHex(str) {
    const valI32 = parseInt(str, 10);
    const valU32 = (valI32 >>> 0);
    return '#' + valU32.toString(16).padStart(8, '0').toUpperCase();
}

// FNV-1a 64-bit hash
function fnv1a64(str) {
    let hash = 0xCBF29CE484222325n;
    const prime = 0x100000001B3n;
    const buf = Buffer.from(str, 'utf8');
    for (let i = 0; i < buf.length; i++) {
        hash ^= BigInt(buf[i]);
        hash = (hash * prime) & 0xFFFFFFFFFFFFFFFFn;
    }
    return hash;
}

// DJB2 hash for LNG files
function djb2(key, seed = 5381, mult = 33, buckets = 17528) {
    let h = seed;
    for (let i = 0; i < key.length; i++) {
        h = (h * mult + key.charCodeAt(i)) >>> 0;
    }
    return (h % buckets) >>> 0;
}

function sha256(buf) {
    return crypto.createHash('sha256').update(buf).digest('hex').toLowerCase();
}

// -------------------------------------------------------------
// GAMEDATABASES.BIN PARSER & SERIALIZER
// -------------------------------------------------------------
function loadDatabase(input) {
    const buf = Buffer.isBuffer(input) ? input : fs.readFileSync(input);
    const count = buf.readUInt32LE(0);
    const low0 = buf.readUInt32LE(4);

    let pos = 8;
    let counter = 0;
    const records = [];
    let prevLow = low0;

    for (let i = 0; i < count; i++) {
        const high = buf.readUInt32LE(pos); pos += 4;
        const tag = buf[pos]; pos += 1;
        const length = buf.readUInt32LE(pos); pos += 4;

        const payload = buf.subarray(pos, pos + length);
        const decoded = Buffer.alloc(length);
        for (let j = 0; j < length; j++) {
            decoded[j] = (payload[j] - DB_KEY[(counter + j) % DB_KEY.length]) & 0xFF;
        }
        counter = (counter + length) % DB_KEY.length;
        pos += length;
        const term = buf[pos]; pos += 1;

        const keyU64 = (BigInt(high) << 32n) | BigInt(prevLow);
        records.push({
            key: keyU64,
            tag,
            data: decoded,
            term
        });

        if (i < count - 1) {
            prevLow = buf.readUInt32LE(pos); pos += 4;
        }
    }
    return records;
}

function saveDatabase(records) {
    let totalLen = 4 + 4;
    for (let i = 0; i < records.length; i++) {
        totalLen += 4 + 1 + 4 + records[i].data.length + 1;
        if (i < records.length - 1) totalLen += 4;
    }

    const outBuf = Buffer.alloc(totalLen);
    outBuf.writeUInt32LE(records.length, 0);
    outBuf.writeUInt32LE(Number(records[0].key & 0xFFFFFFFFn), 4);

    let pos = 8;
    let counter = 0;
    for (let i = 0; i < records.length; i++) {
        const rec = records[i];
        outBuf.writeUInt32LE(Number(rec.key >> 32n), pos); pos += 4;
        outBuf[pos] = rec.tag; pos += 1;
        outBuf.writeUInt32LE(rec.data.length, pos); pos += 4;

        for (let j = 0; j < rec.data.length; j++) {
            outBuf[pos + j] = (rec.data[j] + DB_KEY[(counter + j) % DB_KEY.length]) & 0xFF;
        }
        counter = (counter + rec.data.length) % DB_KEY.length;
        pos += rec.data.length;

        outBuf[pos] = rec.term; pos += 1;

        if (i < records.length - 1) {
            outBuf.writeUInt32LE(Number(records[i + 1].key & 0xFFFFFFFFn), pos); pos += 4;
        }
    }
    return outBuf;
}

function applyGridToDatabase(records, gridConfig) {
    // Map records by key
    const recordsMap = new Map();
    for (const r of records) {
        recordsMap.set(r.key, r);
    }

    let modifiedCount = 0;

    for (const team of gridConfig.teams) {
        // Team color modernization
        const colorHex = team.color || team.team_color;
        if (colorHex && TEAM_COLOR_KEYS[team.team_id]) {
            const colorValStr = hexToColorInt32Str(colorHex);
            if (colorValStr) {
                for (const key of TEAM_COLOR_KEYS[team.team_id]) {
                    const rec = recordsMap.get(key);
                    if (rec) {
                        const currentVal = rec.data.toString('utf8');
                        if (currentVal !== colorValStr) {
                            rec.data = Buffer.from(colorValStr, 'utf8');
                            modifiedCount++;
                        }
                    }
                }
            }
        }

        for (const driver of team.drivers) {
            const idx = driver.driver_index;
            const initKey = fnv1a64(`entrants.drivers.driver_${idx}.initials`);
            const natKey = fnv1a64(`entrants.drivers.driver_${idx}.nationality`);

            const initRec = recordsMap.get(initKey);
            if (initRec) {
                initRec.data = Buffer.from(driver.target_initials, 'utf8');
                modifiedCount++;
            }

            const natRec = recordsMap.get(natKey);
            if (natRec) {
                natRec.data = Buffer.from(driver.target_nationality, 'utf8');
                modifiedCount++;
            }

            // If driver has a custom first_name_key (e.g., driver_74 -> lng_gabriel to prevent collision)
            if (driver.first_name_key && driver.first_name_key !== 'lng_antonio' && idx === 74) {
                const fnKey = fnv1a64(`entrants.drivers.driver_${idx}.first_name`);
                const fnRec = recordsMap.get(fnKey);
                if (fnRec) {
                    fnRec.data = Buffer.from(driver.first_name_key, 'utf8');
                    modifiedCount++;
                }
            }
        }
    }
    return modifiedCount;
}

// -------------------------------------------------------------
// LNG LOCALIZATION PARSER & COMPILER
// -------------------------------------------------------------
function parseLng(input) {
    const buf = Buffer.isBuffer(input) ? input : fs.readFileSync(input);
    let offset = 0;
    const magic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    if (magic !== 'LNGT') throw new Error(`Invalid LNG magic: ${magic}`);

    const totalSize = buf.readUInt32BE(offset); offset += 4;
    const hshsMagic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    const hshsSize = buf.readUInt32BE(offset); offset += 4;
    const buckets = buf.readUInt32BE(offset); offset += 4;
    const seed = buf.readUInt32BE(offset); offset += 4;
    const multiplier = buf.readUInt32BE(offset); offset += 4;

    const hshtMagic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    const hshtSize = buf.readUInt32BE(offset); offset += 4;
    offset += hshtSize;

    const sidaMagic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    const sidaSize = buf.readUInt32BE(offset); offset += 4;
    const numEntries = buf.readUInt32BE(offset); offset += 4;

    const sidaEntries = [];
    for (let i = 0; i < numEntries; i++) {
        const keyOffset = buf.readUInt32BE(offset); offset += 4;
        const valOffset = buf.readUInt32BE(offset); offset += 4;
        sidaEntries.push({ keyOffset, valOffset });
    }

    const sidbMagic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    const sidbSize = buf.readUInt32BE(offset); offset += 4;
    const keysBuf = buf.subarray(offset, offset + sidbSize);
    offset += sidbSize;

    const lngbMagic = buf.subarray(offset, offset + 4).toString('ascii'); offset += 4;
    const lngbSize = buf.readUInt32BE(offset); offset += 4;
    const valsBuf = buf.subarray(offset, offset + lngbSize);

    function readTerminatedString(b, off) {
        let end = off;
        while (end < b.length && b[end] !== 0) end++;
        return b.subarray(off, end).toString('utf8');
    }

    const entries = [];
    for (let i = 0; i < numEntries; i++) {
        const k = readTerminatedString(keysBuf, sidaEntries[i].keyOffset);
        const v = readTerminatedString(valsBuf, sidaEntries[i].valOffset);
        entries.push({ key: k, val: v });
    }

    return {
        magic, totalSize,
        hshs: { magic: hshsMagic, size: hshsSize, buckets, seed, multiplier },
        entries
    };
}

function writeLng(entries, seed = 5381, mult = 33, buckets = 17528) {
    const bucketLists = Array.from({ length: buckets }, () => []);
    for (const entry of entries) {
        const h = djb2(entry.key, seed, mult, buckets);
        bucketLists[h].push(entry);
    }

    const orderedEntries = [];
    const hshtTable = [];
    let currBucketStart = 0;
    for (let h = 0; h < buckets; h++) {
        const list = bucketLists[h];
        if (list.length === 0) {
            hshtTable.push({ start: 0, count: 0 });
        } else {
            hshtTable.push({ start: currBucketStart, count: list.length });
            for (const item of list) {
                orderedEntries.push(item);
            }
            currBucketStart += list.length;
        }
    }

    const keyBufs = [];
    const valBufs = [];
    const sidaEntries = [];
    let keyOffset = 0;
    let valOffset = 0;

    for (const item of orderedEntries) {
        sidaEntries.push({ keyOffset, valOffset });
        const kBuf = Buffer.from(item.key + '\0', 'utf8');
        const vBuf = Buffer.from(item.val + '\0', 'utf8');
        keyBufs.push(kBuf);
        valBufs.push(vBuf);
        keyOffset += kBuf.length;
        valOffset += vBuf.length;
    }

    const sidbData = Buffer.concat(keyBufs);
    const lngbData = Buffer.concat(valBufs);

    const hshsSize = 12;
    const hshtSize = buckets * 8;
    const sidaSize = orderedEntries.length * 8;
    const sidbSize = sidbData.length;
    const lngbSize = lngbData.length;

    const totalFileSize = 4 + 4 + (4 + 4 + hshsSize) + (4 + 4 + hshtSize) + (4 + 4 + 4 + sidaSize) + (4 + 4 + sidbSize) + (4 + 4 + lngbSize);

    const outBuf = Buffer.alloc(totalFileSize);
    let pos = 0;

    outBuf.write('LNGT', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(totalFileSize, pos); pos += 4;

    outBuf.write('HSHS', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(hshsSize, pos); pos += 4;
    outBuf.writeUInt32BE(buckets, pos); pos += 4;
    outBuf.writeUInt32BE(seed, pos); pos += 4;
    outBuf.writeUInt32BE(mult, pos); pos += 4;

    outBuf.write('HSHT', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(hshtSize, pos); pos += 4;
    for (const b of hshtTable) {
        outBuf.writeUInt32BE(b.start, pos); pos += 4;
        outBuf.writeUInt32BE(b.count, pos); pos += 4;
    }

    outBuf.write('SIDA', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(sidaSize, pos); pos += 4;
    outBuf.writeUInt32BE(orderedEntries.length, pos); pos += 4;
    for (const s of sidaEntries) {
        outBuf.writeUInt32BE(s.keyOffset, pos); pos += 4;
        outBuf.writeUInt32BE(s.valOffset, pos); pos += 4;
    }

    outBuf.write('SIDB', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(sidbSize, pos); pos += 4;
    sidbData.copy(outBuf, pos); pos += sidbSize;

    outBuf.write('LNGB', pos, 4, 'ascii'); pos += 4;
    outBuf.writeUInt32BE(lngbSize, pos); pos += 4;
    lngbData.copy(outBuf, pos); pos += lngbSize;

    return outBuf;
}

function applyGridToLng(parsedLng, gridConfig) {
    const stringMap = new Map();
    for (const e of parsedLng.entries) {
        stringMap.set(e.key, e);
    }

    const replacements = {};

    for (const team of gridConfig.teams) {
        if (team.showroom_key && team.showroom_lineup) {
            replacements[team.showroom_key] = team.showroom_lineup;
        }
        if (team.team_title_key && team.team_title_val) {
            replacements[team.team_title_key] = team.team_title_val;
        }
        if (team.team_short_key && team.team_short_val) {
            replacements[team.team_short_key] = team.team_short_val;
        }

        for (const driver of team.drivers) {
            if (driver.first_name_key && driver.target_first_name) {
                replacements[driver.first_name_key] = driver.target_first_name;
            }
            if (driver.last_name_key && driver.target_last_name) {
                replacements[driver.last_name_key] = driver.target_last_name;
            }
            if (driver.full_name_key && driver.target_full_name) {
                replacements[driver.full_name_key] = driver.target_full_name;
            }
            if (driver.short_name_key && driver.target_short_name) {
                replacements[driver.short_name_key] = driver.target_short_name;
            }
        }
    }

    let modifiedCount = 0;
    for (const [k, v] of Object.entries(replacements)) {
        if (stringMap.has(k)) {
            stringMap.get(k).val = v;
            modifiedCount++;
        } else {
            // New key (e.g. lng_gabriel)
            const newEntry = { key: k, val: v };
            parsedLng.entries.push(newEntry);
            stringMap.set(k, newEntry);
            modifiedCount++;
        }
    }

    return modifiedCount;
}

// -------------------------------------------------------------
// EXECUTABLE PATCHING (1-BYTE REDIRECT)
// -------------------------------------------------------------
const EXE_SPECS = {
    dx11: {
        offset: 0x2282718,
        byteOffset: 0x2282723,
        stockSha256: 'c3b62b2f256645a3db7e4b1e5c7f384df2f6c86619aa2d1705ba9d4ea9760e97',
        filename: 'F1_2020.exe'
    },
    dx12: {
        offset: 0x2261898,
        byteOffset: 0x22618a3,
        stockSha256: '5fd6ac64158a5ffd2d001a4238365e4ac3749a496f150e42feea236dad1ee89d',
        filename: 'F1_2020_dx12.exe'
    }
};

function checkExecutableStatus(filePath, isDx12 = false) {
    if (!fs.existsSync(filePath)) return { exists: false, status: 'MISSING' };
    const spec = isDx12 ? EXE_SPECS.dx12 : EXE_SPECS.dx11;
    const buf = fs.readFileSync(filePath);
    const hash = sha256(buf);
    const s = buf.subarray(spec.offset, spec.offset + 13).toString('ascii');
    const isStock = s === 'localisation/';
    const isPatched = s === 'localisatioa/';
    return {
        exists: true,
        size: buf.length,
        sha256: hash,
        redirectString: s,
        status: isPatched ? 'PATCHED' : isStock ? 'STOCK' : 'UNKNOWN'
    };
}

function patchExecutableBuffer(buf, isDx12 = false) {
    const spec = isDx12 ? EXE_SPECS.dx12 : EXE_SPECS.dx11;
    const curByte = buf[spec.byteOffset];
    if (curByte === 0x6e) { // 'n' -> 'a'
        buf[spec.byteOffset] = 0x61;
        return true;
    } else if (curByte === 0x61) {
        return false; // already patched
    } else {
        throw new Error(`Unexpected byte at 0x${spec.byteOffset.toString(16)}: 0x${curByte.toString(16)}`);
    }
}

function rollbackExecutableBuffer(buf, isDx12 = false) {
    const spec = isDx12 ? EXE_SPECS.dx12 : EXE_SPECS.dx11;
    const curByte = buf[spec.byteOffset];
    if (curByte === 0x61) { // 'a' -> 'n'
        buf[spec.byteOffset] = 0x6e;
        return true;
    } else if (curByte === 0x6e) {
        return false; // already stock
    } else {
        throw new Error(`Unexpected byte at 0x${spec.byteOffset.toString(16)}: 0x${curByte.toString(16)}`);
    }
}

// -------------------------------------------------------------
// HIGH-LEVEL GRID ORCHESTRATOR
// -------------------------------------------------------------
function buildGridStagedPatch(gameDir, arg2, arg3, rawLngDir) {
    let gridConfig = arg2;
    let stageDir = arg3;
    if (typeof arg2 === 'string' && typeof arg3 === 'object') {
        gridConfig = arg3;
        stageDir = arg2;
    }

    if (!stageDir || typeof stageDir !== 'string') {
        stageDir = path.join(gameDir, 'f1_2020_grid_changer', 'staged_patch');
    }

    fs.mkdirSync(stageDir, { recursive: true });
    fs.mkdirSync(path.join(stageDir, 'game_package', 'gamedatabases'), { recursive: true });
    fs.mkdirSync(path.join(stageDir, 'localisatioa', '2020_release'), { recursive: true });

    // Locate clean raw languages
    if (!rawLngDir || !fs.existsSync(path.join(rawLngDir, 'language_eng.lng'))) {
        const candidates = [
            path.join(stageDir, '..', 'raw_languages'),
            path.join(gameDir, 'f1_2020_grid_changer', 'raw_languages'),
            path.join(gameDir, 'f1_2020_colapinto_poc')
        ];
        for (const c of candidates) {
            if (fs.existsSync(path.join(c, 'language_eng.lng'))) {
                rawLngDir = c;
                break;
            }
        }
    }

    const backupDir = path.join(stageDir, '..', 'backups');

    // 1. Patch Executables
    for (const isDx12 of [false, true]) {
        const spec = isDx12 ? EXE_SPECS.dx12 : EXE_SPECS.dx11;
        const backupPath = path.join(backupDir, spec.filename);
        const livePath = path.join(gameDir, spec.filename);
        const srcPath = fs.existsSync(backupPath) ? backupPath : livePath;

        if (fs.existsSync(srcPath)) {
            const exeBuf = Buffer.from(fs.readFileSync(srcPath));
            patchExecutableBuffer(exeBuf, isDx12);
            fs.writeFileSync(path.join(stageDir, spec.filename), exeBuf);
        }
    }

    // 2. Patch Database
    const dbBackupPath = path.join(backupDir, 'gamedatabases.bin');
    const dbLivePath = path.join(gameDir, 'game_package', 'gamedatabases', 'gamedatabases.bin');
    const dbSrc = fs.existsSync(dbBackupPath) ? dbBackupPath : dbLivePath;

    const dbRecords = loadDatabase(dbSrc);
    applyGridToDatabase(dbRecords, gridConfig);
    const patchedDbBuf = saveDatabase(dbRecords);
    fs.writeFileSync(path.join(stageDir, 'game_package', 'gamedatabases', 'gamedatabases.bin'), patchedDbBuf);

    // 3. Patch English LNG
    const engSrc = path.join(rawLngDir, 'language_eng.lng');
    const parsedEng = parseLng(engSrc);
    applyGridToLng(parsedEng, gridConfig);
    const engRebuilt = writeLng(parsedEng.entries, parsedEng.hshs.seed, parsedEng.hshs.multiplier, parsedEng.hshs.buckets);
    fs.writeFileSync(path.join(stageDir, 'localisatioa', '2020_release', 'language_eng.lng'), engRebuilt);

    // 4. Patch Spanish LNG
    const spaSrc = path.join(rawLngDir, 'language_spa.lng');
    const parsedSpa = parseLng(spaSrc);
    applyGridToLng(parsedSpa, gridConfig);
    const spaRebuilt = writeLng(parsedSpa.entries, parsedSpa.hshs.seed, parsedSpa.hshs.multiplier, parsedSpa.hshs.buckets);
    fs.writeFileSync(path.join(stageDir, 'localisatioa', '2020_release', 'language_spa.lng'), spaRebuilt);

    return {
        dbBytes: patchedDbBuf.length,
        engBytes: engRebuilt.length,
        spaBytes: spaRebuilt.length
    };
}

module.exports = {
    fnv1a64,
    djb2,
    sha256,
    loadDatabase,
    saveDatabase,
    applyGridToDatabase,
    TEAM_COLOR_KEYS,
    hexToColorInt32Str,
    colorInt32StrToHex,
    parseLng,
    writeLng,
    applyGridToLng,
    EXE_SPECS,
    checkExecutableStatus,
    patchExecutableBuffer,
    rollbackExecutableBuffer,
    buildGridStagedPatch
};

