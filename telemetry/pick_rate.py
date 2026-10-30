import collections, csv, json, math, os, sys

from posthog import run_query

# 单次最多 500 行，分页拉取
BATCH = 500

# 楼层门槛已移到模组端（FgoTelemetry.cs），此处全量采集
cnt_out = run_query(
    "SELECT count() AS c FROM events WHERE event = 'run_history.completed' "
    "AND timestamp >= now() - INTERVAL 7 DAY"
)
total_runs = int(cnt_out.split("\n")[1].strip())
total_batches = math.ceil(total_runs / BATCH)

def progress_bar(done, total, width=30):
    frac = done / total if total else 0
    filled = int(width * frac)
    bar = "=" * filled + ">" + "." * (width - filled - 1)
    sys.stdout.write("\r拉取中 [%s] %d/%d 局 (%d/%d 批)" % (bar, done, total, math.ceil(done / BATCH), total_batches))
    sys.stdout.flush()

# 分页拉近 7 天所有 run_history.completed 的 map_point_history（全量，楼层门槛见模组端）
all_lines = []
offset = 0
while True:
    out = run_query(
        "SELECT timestamp, properties.is_victory AS vic, properties.is_abandoned AS abn, "
        "properties.payload.applicant_payload.run_history.map_point_history AS mph "
        "FROM events WHERE event = 'run_history.completed' "
        "AND timestamp >= now() - INTERVAL 7 DAY "
        "ORDER BY timestamp DESC LIMIT %d OFFSET %d" % (BATCH, offset),
        truncate=False,
    )
    rows = [r for r in out.split("\n")[1:] if r.strip()]
    if not rows:
        break
    all_lines.extend(rows)
    progress_bar(len(all_lines), total_runs)
    if len(rows) < BATCH:
        break
    offset += BATCH
sys.stdout.write("\n")
sys.stdout.flush()

# 加载中文卡名映射: "FGO_CARD_XXX.title" -> 中文名
cards_path = r"D:\projects\fgo\Fgo\localization\zhs\cards.json"
cn_map = {}
if os.path.exists(cards_path):
    with open(cards_path, encoding="utf-8") as f:
        cn_map = json.load(f)

def cn_name(cid):
    key = cid[len("CARD."):] + ".title" if cid.startswith("CARD.") else cid + ".title"
    return cn_map.get(key, "")

rows = all_lines
stats = collections.defaultdict(lambda: {"offered": 0, "picked": 0,
                                         "runs_picked_all": 0, "runs_picked_all_won": 0,
                                         "runs_picked_active": 0, "runs_picked_active_won": 0})
run_count = 0
for line in rows:
    if not line.strip():
        continue
    parts = line.split("|", 3)
    if len(parts) != 4:
        continue
    ts, vic, abn, mph_json = parts
    won = str(vic).strip().lower() in ("true", "1")
    abandoned = str(abn).strip().lower() in ("true", "1")
    try:
        mph = json.loads(mph_json)
    except Exception:
        continue
    run_count += 1
    run_picked = set()
    for player_mps in mph:
        for mp in player_mps:
            for ps in mp.get("player_stats", []):
                for cc in ps.get("card_choices", []):
                    cid = cc.get("card", {}).get("id", "?")
                    if not cid.startswith("CARD.FGO_"):
                        continue
                    stats[cid]["offered"] += 1
                    if cc.get("was_picked"):
                        stats[cid]["picked"] += 1
                        run_picked.add(cid)
    for cid in run_picked:
        stats[cid]["runs_picked_all"] += 1  # 含弃局
        if won:
            stats[cid]["runs_picked_all_won"] += 1
        if not abandoned:
            stats[cid]["runs_picked_active"] += 1  # 不含弃局
            if won:
                stats[cid]["runs_picked_active_won"] += 1

print("解析局数:", run_count)
print("涉及卡牌数:", len(stats))
print("")

# 导出 CSV（写入失败不换文件名，直接报错提示）
csv_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "pick_rate.csv")
try:
    with open(csv_path, "w", newline="", encoding="utf-8-sig") as f:
        w = csv.writer(f)
        w.writerow(["card_id", "cn_name", "offered", "picked", "抓率", "胜率", "胜率（含弃局）"])
        for cid, s in sorted(stats.items(), key=lambda kv: -(kv[1]["picked"] * 100.0 / kv[1]["offered"] if kv[1]["offered"] else 0)):
            pick_rate = s["picked"] * 100.0 / s["offered"] if s["offered"] else 0
            wr_excl = s["runs_picked_active_won"] * 100.0 / s["runs_picked_active"] if s["runs_picked_active"] else 0
            wr_incl = s["runs_picked_all_won"] * 100.0 / s["runs_picked_all"] if s["runs_picked_all"] else 0
            w.writerow([cid, cn_name(cid), s["offered"], s["picked"], round(pick_rate, 1),
                        round(wr_excl, 1), round(wr_incl, 1)])
except PermissionError:
    print("写入失败: %s 被占用（可能正被 Excel 打开），请关闭后重试" % csv_path)
    raise SystemExit(1)
print("已导出:", csv_path)