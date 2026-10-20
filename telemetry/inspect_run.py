import json, sys

from posthog import run_query

out = run_query(
    "SELECT timestamp, properties FROM events "
    "WHERE event = 'run_history.completed' ORDER BY timestamp DESC LIMIT 1",
    truncate=False,
)
lines = [l for l in out.split("\n")[1:] if l.strip()]
print("行数:", len(lines))
if not lines:
    sys.exit(0)
parts = lines[0].split("|", 1)
ts, props = parts
props = json.loads(props)

def walk(obj, prefix="", depth=0, max_depth=3):
    if depth > max_depth:
        return
    if isinstance(obj, dict):
        for k, v in obj.items():
            if isinstance(v, (dict, list)):
                kind = "dict" if isinstance(v, dict) else "list[%d]" % len(v)
                print("%s%s : %s" % (prefix, k, kind))
                if isinstance(v, dict) and len(v) <= 200:
                    walk(v, prefix + "  ", depth + 1, max_depth)
                elif isinstance(v, list) and v and isinstance(v[0], dict) and len(v) <= 3:
                    walk(v[0], prefix + "  [0] ", depth + 1, max_depth)
            else:
                s = str(v)
                if len(s) > 60:
                    s = s[:60] + "..."
                print("%s%s = %r" % (prefix, k, s))

print("=== 顶层 properties ===")
walk(props, "", 0, 1)
print("=== applicant_payload ===")
walk(props.get("payload", {}).get("applicant_payload", {}), "", 0, 2)
