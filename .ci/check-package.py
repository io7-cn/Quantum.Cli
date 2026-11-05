"""Validate tool identity, package contents and integrity without executing a remote package."""
import hashlib
from pathlib import Path
import xml.etree.ElementTree as ET
import zipfile

packages = sorted(Path("artifacts/packages").glob("*.nupkg"))
if len(packages) != 1:
    raise SystemExit("Expected exactly one tool package; clean artifacts/packages before changing version.")
package = packages[0]
with zipfile.ZipFile(package) as archive:
    names = archive.namelist()
    for required in ("README.md", "LICENSE", "NOTICE", "tools/net10.0/any/quantum-cli.dll", "tools/net10.0/any/DotnetToolSettings.xml"):
        assert required in names, required
    assert not any(name.endswith((".pdb", ".cs", ".pfx", ".p12")) for name in names)
    settings = ET.fromstring(archive.read("tools/net10.0/any/DotnetToolSettings.xml"))
    command = settings.find(".//Command")
    assert command is not None and command.attrib["Name"] == "quantum-cli" and command.attrib["EntryPoint"] == "quantum-cli.dll"
    nuspec = ET.fromstring(archive.read(next(name for name in names if name.endswith(".nuspec"))))
    ns = {"n": nuspec.tag.split("}")[0].strip("{")}
    metadata = nuspec.find("n:metadata", ns)
    assert metadata.findtext("n:id", namespaces=ns) == "Quantum.Platform.Cli"
    assert metadata.findtext("n:license", namespaces=ns) == "Apache-2.0"
digest = hashlib.sha256(package.read_bytes()).hexdigest()
(package.parent / "SHA256SUMS").write_text(f"{digest}  {package.name}\n", encoding="utf-8")
print("Verified package identity, license, contents and digest:", package.name)
