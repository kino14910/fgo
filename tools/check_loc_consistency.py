import json, re, sys
from pathlib import Path
from collections import Counter

BASE = Path("D:/projects/fgo/Fgo/localization")
# 条件语法占位符；选项段允许嵌一层有名占位符（如 {IfUpgraded:show:a|最多{ExhaustCount:diff()}}）
NEST = r"(?:[^{}|]|\{(?:[^{}|]|\{[^{}]*\})*\})*"
COND = re.compile(r"\{(IfUpgraded|IfDowngraded|IfUpgradedA|IfUpgradedB)(?::(show|choose|plural|cond|diff))?(?::(" + NEST + r"(?:\|" + NEST + r")*))?\}")
VAR = re.compile(r"\{([A-Za-z_][A-Za-z0-9_]*)((?::(?:diff|cond|plural|show|choose)[^{}|]*)*)\}")

def load(p):
    raw = p.read_bytes()
    return json.loads(raw.decode("utf-8-sig")), raw.startswith(b"\xef\xbb\xbf")

def flat(d, prefix=""):
    out = {}
    for k, v in d.items():
        key = f"{prefix}.{k}" if prefix else k
        if isinstance(v, dict):
            out.update(flat(v, key))
        else:
            out[key] = v
    return out

def sig(s):
    """提取结构签名：条件语法 + 普通变量名(多重集) + 每变量的修饰符"""
    conds = Counter()
    for m in COND.finditer(s):
        opts = m.group(3)
        conds[(m.group(1), m.group(2), (opts.count("|") if opts else 0))] += 1
    vars_ = Counter()
    for m in VAR.finditer(s):
        name, mods = m.group(1), m.group(2) or ""
        # plural/cond/show/choose 是语言相关修饰，diff 是数值修饰
        core = tuple(sorted(x for x in mods.split(":") if x and x != "diff"))
        vars_[(name, core)] += 1
    # 空选择子 {:diff()}：官方教程合法写法（sts2modding 教程 choose 示例），
    # SmartFormat 中继承外层/最近的有名变量，归属后与显式写法等价
    for (owner, fmt), cnt in empty_selectors(s):
        vars_[(owner, (fmt,))] += cnt
    return conds, vars_

NAME_RE = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")

def empty_selectors(s):
    """扫描字符串中所有 {:formatter} 空选择子，返回 [(归属变量名, formatter, 次数)]。
    归属规则：外层占位符名 > 最近的有名占位符；都没有则记 <none>。"""
    out = []
    stack = []
    i, n = 0, len(s)
    while i < n:
        if s[i] == "{":
            j, depth = i + 1, 1
            while j < n and depth:
                if s[j] == "{": depth += 1
                elif s[j] == "}": depth -= 1
                j += 1
            if depth != 0:
                break
            inner = s[i + 1:j - 1]
            if inner.startswith(":"):
                fmt = inner[1:].split(":", 1)[0]
                owner = stack[-1] if stack else "<none>"
                out.append((owner, fmt))
                i = j
                continue
            head = inner.split(":", 1)[0]
            stack.append(head if NAME_RE.fullmatch(head or "") else "<unnamed>")
            i += 1
            continue
        if s[i] == "}":
            if stack: stack.pop()
        i += 1
    return Counter(out).items()

problems = []
files = sorted(x.name for x in (BASE/"eng").glob("*.json"))
def check_pair(name, e, eflat, other, oflat, tag):
    """比对 eng 与另一语言（zhs/jpn）的键集合、空值、占位符签名"""
    probs = []
    if other.get("__bom__"): probs.append(f"[{name}] {tag} 带 BOM")
    for k in sorted(set(eflat) - set(oflat)):
        probs.append(f"[{name}] eng-only key: {k}")
    for k in sorted(set(oflat) - set(eflat)):
        probs.append(f"[{name}] {tag}-only key: {k}")
    for k in sorted(set(eflat) & set(oflat)):
        ev, ov = eflat[k], oflat[k]
        if not isinstance(ev, str) or not isinstance(ov, str):
            continue
        if not ev.strip() and ov.strip():
            probs.append(f"[{name}] {k}: ENG 空值 ({tag}: {ov[:40]!r})")
        if not ov.strip() and ev.strip():
            probs.append(f"[{name}] {k}: {tag} 空值 (eng: {ev[:40]!r})")
        if not ev.strip() or not ov.strip():
            continue
        ec, evv = sig(ev)
        oc, ovv = sig(ov)
        if ec != oc:
            probs.append(f"[{name}] {k}: 条件语法不同\n    eng: {dict(ec)}\n    {tag}: {dict(oc)}")
        for item, cnt in (evv - ovv).items():
            probs.append(f"[{name}] {k}: eng 有 {tag} 无 变量 {item[0]} (eng x{cnt})")
        for item, cnt in (ovv - evv).items():
            probs.append(f"[{name}] {k}: {tag} 有 eng 无 变量 {item[0]} ({tag} x{cnt})")
    return probs

for name in files:
    ef, zf = BASE/"eng"/name, BASE/"zhs"/name
    e, ebom = load(ef)
    z, zbom = load(zf)
    if ebom: problems.append(f"[{name}] eng 带 BOM")
    if zbom: problems.append(f"[{name}] zhs 带 BOM")
    problems += check_pair(name, e, flat(e), {"__bom__": zbom, **z}, flat(z), "zhs")
    jf = BASE/"jpn"/name
    if jf.exists():
        j, jbom = load(jf)
        problems += check_pair(name, e, flat(e), {"__bom__": jbom, **j}, flat(j), "jpn")
    elif (BASE/"jpn").is_dir():
        problems.append(f"[{name}] jpn 文件缺失")

langs = ["eng", "zhs"] + (["jpn"] if (BASE/"jpn").is_dir() else [])
print(f"比对 {len(files)} 个文件（{'/'.join(langs)}）\n")
if problems:
    print(f"发现 {len(problems)} 处问题:\n")
    for p in problems:
        print(" -", p)
else:
    print("全部一致")
