#!/usr/bin/env python3
"""Lightweight XAML linter for the WPF project.

wpf_check.py verifies the C# side (handlers, x:Name fields) but does NOT
compile XAML markup, so it misses XAML-only errors that only the real
Windows XAML compiler (MC****) would catch. This linter covers the most
common one that has bitten us: a property set twice on the same element,
once as an attribute (Style="...") and once as a property element
(<Button.Style>...), which fails on Windows with MC3024.

It also runs an xmllint well-formedness check on every .xaml file.

Usage: python3 tools/xaml_lint.py
Exit code 0 = clean, 1 = problems found.
"""
import glob
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

WPF = Path(__file__).resolve().parent.parent / "FleetManagementSystem_CSharp_WPF/FleetManagementSystem.WPF"


def compiled_xaml_files():
    """Only the XAML that the csproj actually compiles (Page/ApplicationDefinition).

    Files under Views/ and Resources/ResourceDictionaries.xaml are legacy,
    excluded from the build (EnableDefaultItems=false), and use markup
    (System: prefix, etc.) that xmllint can't parse — linting them is noise.
    """
    tree = ET.parse(WPF / "FleetManagementSystem.WPF.csproj")
    files = []
    for item in tree.iter():
        tag = item.tag.split("}")[-1]
        if tag in ("Page", "ApplicationDefinition") and item.get("Include"):
            files.append(str(WPF / item.get("Include").replace("\\", "/")))
    return sorted(files)

# Properties commonly set both ways by mistake.
PROPS = [
    "Style", "CellStyle", "Template", "ContentTemplate", "Background",
    "Header", "Content", "ItemsSource", "ItemContainerStyle", "Foreground",
]


def find_duplicate_properties(text: str):
    problems = []
    for m in re.finditer(r"<(\w[\w.]*)\b", text):
        tag = m.group(1)
        if "." in tag:
            continue
        start = m.start()
        gt = text.find(">", start)
        if gt == -1:
            continue
        open_tag = text[start:gt]
        window = text[gt:gt + 1000]
        for p in PROPS:
            if re.search(rf"\b{p}\s*=", open_tag) and f"<{tag}.{p}>" in window:
                line = text[:start].count("\n") + 1
                problems.append((line, f"<{tag}> sets {p} as both attribute and element (MC3024)"))
    return problems


def main():
    problems = []
    xaml_files = compiled_xaml_files()
    for f in xaml_files:
        text = Path(f).read_text(encoding="utf-8")
        for line, msg in find_duplicate_properties(text):
            problems.append(f"{f}:{line}  {msg}")

        result = subprocess.run(["xmllint", "--noout", f], capture_output=True, text=True)
        if result.returncode != 0:
            problems.append(f"{f}: not well-formed\n{result.stderr.strip()}")

    if problems:
        print("XAML LINT FAILED")
        for p in problems:
            print(p)
        return 1

    print(f"XAML LINT PASSED ({len(xaml_files)} files)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
