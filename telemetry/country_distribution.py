from posthog import run_query

# 口径说明：
#   玩家 = distinct_id（每安装稳定的 32 位哈希），每人只计一次。
#   国家归属 = argMax(cc, timestamp)，即该玩家最近一次事件时的 geoip 国家。
#   直接按国家分组 uniqExact(distinct_id) 会把 IP 跨多个国家的玩家重复计数
#   （实测 73 人 / 多计 109），必须按 distinct_id 先聚合再统计。
#   聚合放在 SQL 子查询里完成：中间结果每人一行（上千行）会被 API 默认截断到
#   100 行，外层 GROUP BY 后只剩几十行，绕开截断。
#   总玩家    全时段加载过 mod            event = 'mod_inventory'
#   近30天活跃 近 30 天加载过 mod          event = 'mod_inventory' + 时间窗
#   完赛玩家   全时段完成过对局            event = 'run_history.completed'
Q_TOTAL = (
    "SELECT cc, uniqExact(distinct_id) AS players FROM ("
    "SELECT distinct_id, argMax(properties.$geoip_country_code, timestamp) AS cc "
    "FROM events WHERE event = 'mod_inventory' GROUP BY distinct_id) "
    "GROUP BY cc ORDER BY players DESC"
)
Q_ACTIVE = (
    "SELECT cc, uniqExact(distinct_id) AS players FROM ("
    "SELECT distinct_id, argMax(properties.$geoip_country_code, timestamp) AS cc "
    "FROM events WHERE event = 'mod_inventory' "
    "AND timestamp >= now() - INTERVAL 30 DAY GROUP BY distinct_id) "
    "GROUP BY cc ORDER BY players DESC"
)
Q_FINISHED = (
    "SELECT cc, uniqExact(distinct_id) AS players FROM ("
    "SELECT distinct_id, argMax(properties.$geoip_country_code, timestamp) AS cc "
    "FROM events WHERE event = 'run_history.completed' GROUP BY distinct_id) "
    "GROUP BY cc ORDER BY players DESC"
)

CN_NAMES = {
    "CN": "中国", "HK": "中国香港", "TW": "中国台湾", "MO": "中国澳门",
    "US": "美国", "DE": "德国", "JP": "日本", "KR": "韩国", "GB": "英国",
    "FR": "法国", "RU": "俄罗斯", "CA": "加拿大", "AU": "澳大利亚",
    "IT": "意大利", "ES": "西班牙", "NL": "荷兰", "PL": "波兰", "BR": "巴西",
    "IN": "印度", "SG": "新加坡", "MY": "马来西亚", "TH": "泰国", "VN": "越南",
    "ID": "印度尼西亚", "PH": "菲律宾", "TR": "土耳其", "MX": "墨西哥",
    "AR": "阿根廷", "CL": "智利", "CO": "哥伦比亚", "PE": "秘鲁", "ZA": "南非",
    "SE": "瑞典", "NO": "挪威", "FI": "芬兰", "DK": "丹麦", "CH": "瑞士",
    "AT": "奥地利", "BE": "比利时", "IE": "爱尔兰", "PT": "葡萄牙",
    "GR": "希腊", "CZ": "捷克", "HU": "匈牙利", "RO": "罗马尼亚",
    "UA": "乌克兰", "NZ": "新西兰", "IL": "以色列", "AE": "阿联酋",
    "SA": "沙特阿拉伯", "EG": "埃及", "KZ": "哈萨克斯坦", "BD": "孟加拉国",
}


def count_by_cc(sql):
    out = run_query(sql)
    result = {}
    for line in out.split("\n")[1:]:
        if not line.strip():
            continue
        cc, n = line.split("|")
        result[cc or "未知"] = int(n)
    return result


total = count_by_cc(Q_TOTAL)
active = count_by_cc(Q_ACTIVE)
finished = count_by_cc(Q_FINISHED)


def name_of(cc):
    return CN_NAMES.get(cc, cc)


sum_total = sum(total.values())
rows = sorted(total.items(), key=lambda kv: -kv[1])

print("mod 玩家国家/地区分布（全时段 %d 名玩家，每人按最近一次事件的国家归属）" % sum_total)
print("%-14s %8s %7s %10s %10s" % ("国家/地区", "总玩家", "占比", "近30天活跃", "完赛玩家"))
print("-" * 56)
for cc, n in rows[:25]:
    print("%-14s %8d %6.1f%% %10d %10d" % (
        name_of(cc), n, n * 100.0 / sum_total, active.get(cc, 0), finished.get(cc, 0)))
rest = rows[25:]
if rest:
    rest_n = sum(n for _, n in rest)
    print("%-14s %8d %6.1f%% %10s %10s" % (
        "其他(%d个)" % len(rest), rest_n, rest_n * 100.0 / sum_total, "-", "-"))
print("-" * 56)
print("%-14s %8d %6.1f%% %10d %10d" % (
    "合计", sum_total, 100.0, sum(active.values()), sum(finished.values())))
