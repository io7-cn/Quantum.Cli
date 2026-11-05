"""Validate the local language pairs and repository-relative Markdown links."""

import re
from pathlib import Path
from urllib.parse import unquote, urlsplit

root = Path(__file__).resolve().parent.parent
left = {p.relative_to(root / "docs/en") for p in (root / "docs/en").rglob("*.md")}
right = {p.relative_to(root / "docs/zh-CN") for p in (root / "docs/zh-CN").rglob("*.md")}
errors = []
if left != right:
    errors.append("English and Simplified Chinese document sets differ.")
for edition, other in (("en", "zh-CN"), ("zh-CN", "en")):
    for relative in sorted(left & right):
        source = root / "docs" / edition / relative
        body = source.read_text(encoding="utf-8-sig")
        counterpart = root / "docs" / other / relative
        links = re.findall(r"\[[^\]\n]+\]\(([^)\s]+)\)", body)
        resolved = []
        for link in links:
            target = urlsplit(link)
            if target.scheme or target.netloc:
                continue
            path = (source.parent / unquote(target.path)).resolve() if target.path else source
            if not path.is_relative_to(root) or not path.exists():
                errors.append(f"{source.relative_to(root)}: unresolved link {link}")
            if target.fragment and path.exists() and path.suffix == ".md":
                target_body = path.read_text(encoding="utf-8-sig")
                anchors = set(re.findall(r'<a\s+id="([^"]+)"', target_body))
                for heading in re.findall(r"^#{1,6} (.+)$", target_body, flags=re.MULTILINE):
                    heading = re.sub(r"\[([^]]+)\]\([^)]+\)", r"\1", heading)
                    anchors.add(re.sub(r"[^\w -]", "", heading.lower()).replace(" ", "-"))
                if unquote(target.fragment) not in anchors:
                    errors.append(f"{source.relative_to(root)}: missing anchor {link}")
            resolved.append(path)
        if counterpart.resolve() not in resolved:
            errors.append(f"{source.relative_to(root)}: missing counterpart link")
if errors:
    raise SystemExit("\n".join(errors))
print(f"Checked {len(left)} document pairs and relative links.")
