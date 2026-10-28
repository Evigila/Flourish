"""Generate the common variable-icon WOFF using Windows and Python's standard library.

The bundled original WOFF2 is never changed. Windows computes the glyf/cmap
subset without renumbering glyphs; this script removes unused gvar blocks and
wraps the resulting SFNT in WOFF 1.0. No font converter package is installed.
"""
from __future__ import annotations

import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import struct
import subprocess
import tempfile
import zlib

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/icons/material"
CODEPOINTS = ROOT / "src/Flourish.Blazor/Flourish.Blazor.Framework/Icons/MaterialSymbolsOutlined.codepoints"
COMMON = "MaterialSymbolsOutlined.common.woff"


def aligned(data: bytes) -> bytes:
    return data + bytes(-len(data) % 4)


def checksum(data: bytes) -> int:
    padded = aligned(data)
    return sum(struct.unpack(">" + "I" * (len(padded) // 4), padded)) & 0xFFFFFFFF


def tables(data: bytes) -> dict[bytes, bytes]:
    assert data[:4] == b"\0\1\0\0", "Expected TrueType outlines."
    count = struct.unpack_from(">H", data, 4)[0]
    result = {}
    for index in range(count):
        tag, _, offset, size = struct.unpack_from(">4sIII", data, 12 + index * 16)
        assert offset + size <= len(data), f"Truncated {tag!r} table."
        result[tag] = data[offset:offset + size]
    return result


def gvar_blocks(data: bytes) -> tuple[tuple, bytes, list[bytes]]:
    header = struct.unpack_from(">HHHHIHHI", data)
    _, _, axes, shared, shared_offset, count, flags, start = header
    offsets = struct.unpack_from(">" + ("I" if flags & 1 else "H") * (count + 1), data, 20)
    scale = 1 if flags & 1 else 2
    assert all(a <= b for a, b in zip(offsets, offsets[1:]))
    assert start + offsets[-1] * scale <= len(data)
    shared_data = data[shared_offset:shared_offset + shared * axes * 2]
    assert len(shared_data) == shared * axes * 2
    return header, shared_data, [data[start + a * scale:start + b * scale] for a, b in zip(offsets, offsets[1:])]


def reduce_variations(original: dict[bytes, bytes], subset: dict[bytes, bytes]) -> bytes:
    count = struct.unpack_from(">H", subset[b"maxp"], 4)[0]
    assert count == struct.unpack_from(">H", original[b"maxp"], 4)[0], "Windows changed glyph IDs."
    long_offsets = struct.unpack_from(">h", subset[b"head"], 50)[0] == 1
    offsets = struct.unpack(">" + ("I" if long_offsets else "H") * (count + 1), subset[b"loca"])
    retained = {index for index in range(count) if offsets[index] != offsets[index + 1]}
    header, shared_data, blocks = gvar_blocks(original[b"gvar"])
    major, minor, axes, shared, _, glyph_count, flags, _ = header
    assert glyph_count == count
    positions, payload = [], bytearray()
    for index, block in enumerate(blocks):
        positions.append(len(payload))
        if index in retained:
            payload.extend(block)
    positions.append(len(payload))
    shared_offset = 20 + 4 * (count + 1)
    subset[b"gvar"] = (struct.pack(">HHHHIHHI", major, minor, axes, shared, shared_offset,
                                    count, flags | 1, shared_offset + len(shared_data))
                       + struct.pack(">" + "I" * (count + 1), *positions) + shared_data + payload)
    for tag in (b"fvar", b"avar", b"STAT", b"HVAR"):
        assert subset[tag] == original[tag], f"Variable-font contract changed: {tag!r}."
    # Preserve upstream names/license strings and timestamps, rather than a
    # platform-specific subset name. Keep Windows' chosen loca format.
    subset[b"name"] = original[b"name"]
    head = bytearray(subset[b"head"])
    head[8:12] = bytes(4)
    head[20:36] = original[b"head"][20:36]
    subset[b"head"] = bytes(head)
    tags = sorted(subset)
    power = 2 ** (len(tags).bit_length() - 1)
    result = bytearray(struct.pack(">IHHHH", 0x10000, len(tags), power * 16,
                                   power.bit_length() - 1, len(tags) * 16 - power * 16))
    offset = 12 + len(tags) * 16
    payloads = []
    head_offset = None
    for tag in tags:
        value = subset[tag]
        result.extend(struct.pack(">4sIII", tag, checksum(value), offset, len(value)))
        payloads.append(aligned(value))
        if tag == b"head":
            head_offset = offset
        offset += len(aligned(value))
    result.extend(b"".join(payloads))
    assert head_offset is not None
    struct.pack_into(">I", result, head_offset + 8, (0xB1B0AFBA - checksum(result)) & 0xFFFFFFFF)
    assert checksum(result) == 0xB1B0AFBA
    reduced = tables(result)
    _, _, reduced_blocks = gvar_blocks(reduced[b"gvar"])
    for index in retained:
        assert reduced_blocks[index] == blocks[index], f"Variation deltas changed for glyph {index}."
    return bytes(result)


def woff(font: bytes) -> bytes:
    source = tables(font)
    offset = 44 + len(source) * 20
    directory, payloads = [], []
    for tag, data in sorted(source.items()):
        compressed = zlib.compress(data, 9)
        payload = compressed if len(compressed) < len(data) else data
        check = data[:8] + bytes(4) + data[12:] if tag == b"head" else data
        directory.append(struct.pack(">4sIIII", tag, offset, len(payload), len(data), checksum(check)))
        payloads.append(aligned(payload))
        offset += len(aligned(payload))
    return (struct.pack(">4sIIHHIHHIIIII", b"wOFF", 0x10000, offset, len(source), 0,
                        len(font), 1, 0, 0, 0, 0, 0, 0) + b"".join(directory) + b"".join(payloads))


def ranges(points: set[int]) -> str:
    spans = []
    for point in sorted(points):
        if spans and point == spans[-1][1] + 1:
            spans[-1][1] = point
        else:
            spans.append([point, point])
    return ", ".join(f"U+{a:X}" if a == b else f"U+{a:X}-{b:X}" for a, b in spans)


def css(points: set[int]) -> str:
    remaining, start = [], 0
    for point in sorted(points):
        if start < point:
            remaining.append((start, point - 1))
        start = point + 1
    if start <= 0x10FFFF:
        remaining.append((start, 0x10FFFF))
    fallback = ", ".join(f"U+{a:X}" if a == b else f"U+{a:X}-{b:X}" for a, b in remaining)
    template = ("@font-face {{\n font-family:'Material Symbols Outlined'; font-style:normal; "
                "font-weight:100 700; font-display:block;\n src:url('./{file}') format('{format}');\n"
                " unicode-range:{range};\n}}\n")
    return ("/* Generated by scripts/Subset-MaterialIcons.py. Disjoint ranges avoid downloading\n"
            "   the complete font for common glyphs; every other dynamic glyph keeps its original fallback. */\n"
            + template.format(file=COMMON, format="woff", range=ranges(points))
            + template.format(file="MaterialSymbolsOutlined.woff2", format="woff2", range=fallback))


def literal(path: Path) -> str:
    return "'" + str(path).replace("'", "''") + "'"


def execute(executable: str, text: str, directory: Path, name: str) -> None:
    command = "try {\n$ErrorActionPreference = 'Stop'\n$ProgressPreference = 'SilentlyContinue'\n" + text + "\nexit 0\n} catch {\n[Console]::Error.WriteLine($_.ToString()); exit 1\n}\n\n"
    encoded = base64.b64encode(command.encode("utf-16le")).decode("ascii")
    result = subprocess.run([executable, "-NoProfile", "-NonInteractive", "-EncodedCommand", encoded],
                            capture_output=True, text=True)
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)


def generate() -> tuple[bytes, str, dict]:
    if os.name != "nt" or not shutil.which("pwsh"):
        raise RuntimeError("Regeneration needs Windows DirectWrite, Windows PowerShell/WPF and PowerShell 7; checked-in assets need none of them at runtime.")
    names = json.loads((ASSETS / "common-icons.json").read_text(encoding="utf-8"))["names"]
    mapping = dict(line.split() for line in CODEPOINTS.read_text(encoding="utf-8").splitlines())
    assert names == sorted(set(names)) and names and "help_outline" in names
    assert all(name in mapping for name in names), "Unknown common icon name."
    points = {int(mapping[name], 16) for name in names}
    native_loader = (f"$converter=Get-Content -LiteralPath {literal(ROOT / 'scripts/Convert-MaterialIcons.ps1')} -Raw\n"
                     + "$native=[regex]::Match($converter,'(?s)\\$nativeSource = @''\\r?\\n(.*?)\\r?\\n''@').Groups[1].Value\n"
                     + "Add-Type -TypeDefinition $native\n")
    windows_ps = str(Path(os.environ["SystemRoot"]) / "System32/WindowsPowerShell/v1.0/powershell.exe")
    # WPF's font URI loader rejects some 8.3 user-directory paths returned by
    # Windows TEMP. Keep transient files under the resolved workspace instead.
    working = ROOT / "artifacts/icon-subset-work"
    working.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="subset-", dir=working) as temporary:
        directory = Path(temporary)
        assert directory.resolve().is_relative_to(working.resolve())
        full, raw, reduced = (directory / name for name in ("original.ttf", "subset.ttf", "reduced.ttf"))
        selected = directory / "selected.codepoints"
        selected.write_text("".join(f"{name} {mapping[name]}\n" for name in names), encoding="utf-8")
        execute(shutil.which("pwsh"), native_loader
                + f"[NativeFontContainer]::Convert({literal(ASSETS / 'MaterialSymbolsOutlined.woff2')},{literal(full)})\n", directory, "decode")
        assert full.is_file() and full.stat().st_size > 1_000_000, "Original font decoding failed."
        execute(windows_ps, "Add-Type -AssemblyName PresentationCore\n"
                + f"$font = [Windows.Media.GlyphTypeface]::new([Uri]{literal(full)})\n"
                + "$glyphs = [Collections.Generic.List[UInt16]]::new()\n"
                + f"Get-Content {literal(selected)} | ForEach-Object {{ $p = $_.Split(' '); $glyphs.Add($font.CharacterToGlyphMap[[Convert]::ToInt32($p[1],16)]) }}\n"
                + f"[IO.File]::WriteAllBytes({literal(raw)},$font.ComputeSubset($glyphs))\n", directory, "subset")
        original_tables = tables(full.read_bytes())
        font = reduce_variations(original_tables, tables(raw.read_bytes()))
        reduced.write_bytes(font)
        # Independent Windows outline loaders verify default and FILL=1 shapes.
        execute(windows_ps, "Add-Type -AssemblyName PresentationCore\n"
                + f"$a = [Windows.Media.GlyphTypeface]::new([Uri]{literal(full)})\n$b = [Windows.Media.GlyphTypeface]::new([Uri]{literal(reduced)})\n"
                + f"Get-Content {literal(selected)} | ForEach-Object {{ $p = $_.Split(' '); $cp = [Convert]::ToInt32($p[1],16); $ga=$a.CharacterToGlyphMap[$cp]; $gb=$b.CharacterToGlyphMap[$cp]; if ($ga -ne $gb -or $a.GetGlyphOutline($ga,1,1).ToString([Globalization.CultureInfo]::InvariantCulture) -ne $b.GetGlyphOutline($gb,1,1).ToString([Globalization.CultureInfo]::InvariantCulture)) {{ throw ('Outline mismatch: '+$p[0]) }} }}\n", directory, "verify-outlines")
        execute(shutil.which("pwsh"), native_loader
                + f"$a=[NativeIconOutlines]::Extract({literal(full)},{literal(selected)})\n$b=[NativeIconOutlines]::Extract({literal(reduced)},{literal(selected)})\n"
                + "foreach ($key in $a.Keys) { if ($a[$key] -cne $b[$key]) { throw ('Filled outline mismatch: '+$key) } }\n", directory, "verify-filled")
        output = woff(font)
        encoded, decoded = directory / "common.woff", directory / "roundtrip.ttf"
        encoded.write_bytes(output)
        execute(shutil.which("pwsh"), native_loader
                + f"[NativeFontContainer]::Convert({literal(encoded)},{literal(decoded)})\n", directory, "verify-container")
        assert tables(decoded.read_bytes()) == tables(font), "Windows WOFF decoding changed the font tables."
        report = {"names": len(names), "codepoints": len(points), "originalBytes": (ASSETS / "MaterialSymbolsOutlined.woff2").stat().st_size,
                  "commonBytes": len(output), "sha256": hashlib.sha256(output).hexdigest(), "defaultAndFilledShapesMatch": True,
                  "windowsWoffRoundTripMatches": True,
                  "variationTablesPreserved": ["fvar", "avar", "STAT", "HVAR"], "commonUnicodeRange": ranges(points)}
        return output, css(points), report


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--verify", action="store_true", help="Rebuild independently and compare checked-in bytes without editing them.")
    args = parser.parse_args()
    output, stylesheet, report = generate()
    if args.verify:
        assert (ASSETS / COMMON).read_bytes() == output, "Checked-in common font differs from regeneration."
        assert (ASSETS / "subsets.css").read_text(encoding="utf-8") == stylesheet, "Checked-in ranges differ from regeneration."
    else:
        (ASSETS / COMMON).write_bytes(output)
        (ASSETS / "subsets.css").write_text(stylesheet, encoding="utf-8")
        metadata = json.loads((ASSETS / "source.json").read_text(encoding="utf-8"))
        metadata["commonSubset"] = {"generator": "scripts/Subset-MaterialIcons.py", "manifest": "common-icons.json", "path": COMMON,
                                    "format": "WOFF 1.0", **report}
        (ASSETS / "source.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
