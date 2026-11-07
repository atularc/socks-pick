"""Verify the gameplay-only change against the saved pre-change source."""
import hashlib
import json
import difflib
from pathlib import Path

root = Path(__file__).resolve().parents[1]
report_dir = root / 'Documentation/Opening-Progression'
baseline = json.loads((report_dir / 'gui-baseline.json').read_text(encoding='utf-8-sig'))
changed_gui = [name for name, sha in baseline.items()
               if hashlib.sha256((root / name).read_bytes()).hexdigest().lower() != sha.lower()]
assert not changed_gui, f'GUI files changed: {changed_gui}'
before = json.loads((report_dir / 'Before/levels.json').read_text(encoding='utf-8-sig'))
after = json.loads((root / 'Assets/Resources/Data/levels.json').read_text(encoding='utf-8-sig'))
assert before['levels'][20:] == after['levels'][20:], 'Later campaign changed'
old = (report_dir / 'Before/SockGame.cs').read_text(encoding='utf-8-sig')
new = (root / 'Assets/Scripts/Runtime/SockGame.cs').read_text(encoding='utf-8-sig')
(report_dir / 'runtime-changes.diff').write_text(''.join(difflib.unified_diff(old.splitlines(True), new.splitlines(True), 'before/SockGame.cs', 'after/SockGame.cs')), encoding='utf-8')
report = {'result': 'PASS', 'unchangedGuiFiles': len(baseline), 'unchangedLevels': '21–100',
          'runtimeDiff': 'runtime-changes.diff'}
(report_dir / 'scope-report.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report))
