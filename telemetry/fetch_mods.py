import json

from posthog import run_query

# 1) 拉取 mod_inventory 的完整 loaded_mods（truncate=false 避免截断）
out = run_query(
    "SELECT timestamp, properties.payload.base_payload.loaded_mods AS mods "
    "FROM events WHERE event = 'mod_inventory' ORDER BY timestamp DESC",
    truncate=False,
)

rows = out.split("\n")
mods_by_ts = {}
for line in rows[1:]:
    if not line.strip():
        continue
    parts = line.split("|", 1)
    if len(parts) != 2:
        continue
    ts, mods_json = parts
    mods_by_ts[ts] = json.loads(mods_json)

# 2) 输出每个事件里所有模组：状态 | 名字 | 版本
for ts, mods in mods_by_ts.items():
    print("=== %s (共 %d 个模组) ===" % (ts, len(mods)))
    for m in mods:
        print("%-9s | %s | %s" % (m.get("state"), m.get("name"), m.get("version")))
