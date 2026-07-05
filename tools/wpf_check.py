#!/usr/bin/env python3
"""Stub-compile check for the WPF project on Linux.

The Ubuntu dotnet SDK cannot build net8.0-windows/WPF projects, so this
script simulates the XAML code generation step: for every compiled XAML
page it emits a partial class with InitializeComponent(), typed fields for
each x:Name, and method-group assignments for each wired event handler
(so missing/misnamed handlers fail the compile). It then compiles all the
project's .cs files against the real Core/Data/Services assemblies and the
official .NET + WindowsDesktop reference packs.

Prerequisites (see PROGRESS.md):
  1. apt-get install -y dotnet-sdk-8.0
  2. dotnet build FleetManagementSystem_CSharp_WPF/FleetManagementSystem.Tests
     (fills the Tests bin dir used as the backend reference closure)
  3. Reference packs are downloaded automatically from nuget.org on first run.

Usage: python3 tools/wpf_check.py
"""
import re
import subprocess
import sys
import tempfile
import urllib.request
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
ROOT = REPO / "FleetManagementSystem_CSharp_WPF"
WPF = ROOT / "FleetManagementSystem.WPF"
WORK = Path(tempfile.gettempdir()) / "wpf-stub-check"
STUBS = WORK / "stubs"
REFS = WORK / "refs"
TESTS_BIN = ROOT / "FleetManagementSystem.Tests/bin/Debug/net8.0"
NUGET = Path.home() / ".nuget/packages"

REF_PACKS = {
    "netcore": ("microsoft.netcore.app.ref", "8.0.11"),
    "wdesk": ("microsoft.windowsdesktop.app.ref", "8.0.11"),
}

TYPE_MAP = {
    "Border": "System.Windows.Controls.Border",
    "Button": "System.Windows.Controls.Button",
    "CheckBox": "System.Windows.Controls.CheckBox",
    "ComboBox": "System.Windows.Controls.ComboBox",
    "ContentPresenter": "System.Windows.Controls.ContentPresenter",
    "DataGrid": "System.Windows.Controls.DataGrid",
    "DatePicker": "System.Windows.Controls.DatePicker",
    "FlowDocumentReader": "System.Windows.Controls.FlowDocumentReader",
    "Image": "System.Windows.Controls.Image",
    "ItemsControl": "System.Windows.Controls.ItemsControl",
    "ListBox": "System.Windows.Controls.ListBox",
    "PasswordBox": "System.Windows.Controls.PasswordBox",
    "ScrollViewer": "System.Windows.Controls.ScrollViewer",
    "StackPanel": "System.Windows.Controls.StackPanel",
    "TabControl": "System.Windows.Controls.TabControl",
    "TabItem": "System.Windows.Controls.TabItem",
    "TextBlock": "System.Windows.Controls.TextBlock",
    "TextBox": "System.Windows.Controls.TextBox",
    "Grid": "System.Windows.Controls.Grid",
    "WrapPanel": "System.Windows.Controls.WrapPanel",
    "DockPanel": "System.Windows.Controls.DockPanel",
    "GroupBox": "System.Windows.Controls.GroupBox",
    "Label": "System.Windows.Controls.Label",
    "ListView": "System.Windows.Controls.ListView",
    "RadioButton": "System.Windows.Controls.RadioButton",
    "RichTextBox": "System.Windows.Controls.RichTextBox",
    "Window": "System.Windows.Window",
}

EVENT_MAP = {
    "Click": "System.Windows.RoutedEventHandler",
    "Loaded": "System.Windows.RoutedEventHandler",
    "Checked": "System.Windows.RoutedEventHandler",
    "Unchecked": "System.Windows.RoutedEventHandler",
    "PasswordChanged": "System.Windows.RoutedEventHandler",
    "TextChanged": "System.Windows.Controls.TextChangedEventHandler",
    "SelectionChanged": "System.Windows.Controls.SelectionChangedEventHandler",
    "SelectedDateChanged": "System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>",
    "SelectedCellsChanged": "System.Windows.Controls.SelectedCellsChangedEventHandler",
    "CellEditEnding": "System.EventHandler<System.Windows.Controls.DataGridCellEditEndingEventArgs>",
    "CurrentCellChanged": "System.EventHandler<System.EventArgs>",
    "AutoGeneratingColumn": "System.EventHandler<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>",
}

XNS = "{http://schemas.microsoft.com/winfx/2006/xaml}"


def ensure_ref_packs():
    for key, (pkg, ver) in REF_PACKS.items():
        target = REFS / key
        if (target / "ref/net8.0").is_dir():
            continue
        target.mkdir(parents=True, exist_ok=True)
        url = f"https://api.nuget.org/v3-flatcontainer/{pkg}/{ver}/{pkg}.{ver}.nupkg"
        nupkg = REFS / f"{key}.nupkg"
        print(f"downloading {pkg} {ver} ...")
        urllib.request.urlretrieve(url, nupkg)
        with zipfile.ZipFile(nupkg) as zf:
            zf.extractall(target)


def parse_csproj():
    tree = ET.parse(WPF / "FleetManagementSystem.WPF.csproj")
    pages, compiles = [], []
    for item in tree.iter():
        tag = item.tag.split("}")[-1]
        if tag in ("Page", "ApplicationDefinition") and item.get("Include"):
            pages.append(item.get("Include").replace("\\", "/"))
        elif tag == "Compile" and item.get("Include"):
            compiles.append(item.get("Include").replace("\\", "/"))
    return pages, compiles


def local(tag):
    return tag.split("}")[-1]


def gen_stub(xaml_path: Path):
    tree = ET.parse(xaml_path)
    root = tree.getroot()
    cls = root.get(f"{XNS}Class")
    if not cls:
        return None
    ns, _, name = cls.rpartition(".")

    fields = {}
    handlers = []
    unmapped = set()

    def walk(el, in_template):
        t = local(el.tag)
        is_template = t.endswith("Template") or t == "Style" or in_template
        xname = el.get(f"{XNS}Name") or (el.get("Name") if not t[0].islower() else None)
        if xname and not in_template:
            typ = TYPE_MAP.get(t)
            if typ is None:
                unmapped.add(t)
                typ = "System.Windows.FrameworkElement"
            fields.setdefault(xname, typ)
        for attr, val in el.attrib.items():
            a = local(attr)
            if a in EVENT_MAP and not attr.startswith("{") and re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", val or ""):
                handlers.append((EVENT_MAP[a], val))
        for child in el:
            walk(child, is_template)

    walk(root, False)

    lines = [f"namespace {ns}", "{", f"    public partial class {name}", "    {"]
    for fname, ftype in fields.items():
        lines.append(f"        internal {ftype} {fname};")
    lines.append("        public void InitializeComponent() { }")
    if handlers:
        lines.append("        private void __CheckXamlHandlers()")
        lines.append("        {")
        seen = set()
        for i, (dtype, method) in enumerate(handlers):
            key = (dtype, method)
            if key in seen:
                continue
            seen.add(key)
            lines.append(f"            {dtype} __h{i} = {method};")
            lines.append(f"            _ = __h{i};")
        lines.append("        }")
    lines += ["    }", "}"]
    if unmapped:
        print(f"  [warn] unmapped element types in {xaml_path.name}: {sorted(unmapped)}")
    return "\n".join(lines)


def main():
    ensure_ref_packs()
    STUBS.mkdir(parents=True, exist_ok=True)
    for old in STUBS.glob("*.cs"):
        old.unlink()

    pages, compiles = parse_csproj()
    for page in pages:
        stub = gen_stub(WPF / page)
        if stub:
            out = STUBS / (Path(page).stem + ".g.cs")
            out.write_text(stub)

    # SDK implicit usings equivalent
    (STUBS / "GlobalUsings.g.cs").write_text(
        "\n".join(f"global using {u};" for u in [
            "System", "System.Collections.Generic", "System.IO", "System.Linq",
            "System.Net.Http", "System.Threading", "System.Threading.Tasks",
        ])
    )

    refs = []
    seen_names = set()
    # wdesk first: the netcore pack ships facade WindowsBase/System.Xaml that must not win
    for pack in ("wdesk", "netcore"):
        for dll in sorted((REFS / pack / "ref/net8.0").glob("*.dll")):
            if dll.name not in seen_names:
                seen_names.add(dll.name)
                refs.append(dll)
    for dll in sorted(TESTS_BIN.glob("*.dll")):
        if dll.name.startswith(("xunit", "testhost", "Microsoft.VisualStudio", "Microsoft.TestPlatform")):
            continue
        if dll.name in seen_names or dll.name == "FleetManagementSystem.Tests.dll":
            continue
        seen_names.add(dll.name)
        refs.append(dll)
    for pkg, ver in [
        ("microsoft.extensions.configuration", "8.0.0"),
        ("microsoft.extensions.configuration.fileextensions", "8.0.0"),
        ("microsoft.extensions.configuration.json", "8.0.0"),
        ("microsoft.extensions.fileproviders.abstractions", "8.0.0"),
        ("microsoft.extensions.logging.console", "8.0.0"),
        ("system.security.cryptography.protecteddata", "8.0.0"),
    ]:
        for cand in [NUGET / pkg / ver / "lib/net8.0", NUGET / pkg / ver / "lib/netstandard2.0"]:
            hits = sorted(cand.glob("*.dll"))
            if hits and hits[0].name not in seen_names:
                seen_names.add(hits[0].name)
                refs.append(hits[0])
                break

    sources = [str(WPF / c) for c in compiles] + [str(p) for p in STUBS.glob("*.cs")]

    csc = sorted(Path("/usr/lib/dotnet/sdk").glob("*/Roslyn/bincore/csc.dll"))[-1]
    cmd = ["dotnet", str(csc), "-nologo", "-target:library", "-langversion:latest",
           "-nowarn:CS0649,CS8618,CS0169,CS1998,CS4014,CS0414",
           f"-out:{WORK}/wpf_stub_check.dll", "-noconfig"]
    cmd += [f"-r:{r}" for r in refs]
    cmd += sources

    proc = subprocess.run(cmd, capture_output=True, text=True)
    errors = [l for l in proc.stdout.splitlines() if ": error" in l]
    warnings = [l for l in proc.stdout.splitlines() if ": warning" in l]
    if proc.returncode == 0:
        print(f"STUB COMPILE PASSED ({len(sources)} sources, {len(warnings)} warnings suppressed/ignored)")
        return 0
    print("STUB COMPILE FAILED")
    for line in errors[:60]:
        print(line)
    if not errors:
        print(proc.stdout[-4000:])
        print(proc.stderr[-2000:])
    return 1


if __name__ == "__main__":
    sys.exit(main())
