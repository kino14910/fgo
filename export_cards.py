import json
import re
import glob
import os

CARDS_DIR = "D:/projects/fgo/Scripts/Cards"
ZHS_PATH = "D:/projects/fgo/Fgo/localization/zhs/cards.json"
ENG_PATH = "D:/projects/fgo/Fgo/localization/eng/cards.json"
OUT_PATH = "D:/projects/fgo/cards_export.xlsx"

SKIP_CLASSES = {"FgoCardModel", "FgoCooldownCardModel", "FgoBaseCardModel", "ModCardTemplate"}

RARITY_ZH = {
    "Basic": "初始",
    "Common": "普通",
    "Uncommon": "罕见",
    "Rare": "稀有",
    "Ancient": "远古",
}

PLACEHOLDER_FEEDBACK = "看到这个请向作者反馈此卡牌缺少本地化"


def extract_balanced(text, start):
    """From text[start] which must be '(' or '[', return the balanced substring including the bracket."""
    open_ch = text[start]
    close_ch = ")" if open_ch == "(" else "]"
    depth = 0
    i = start
    n = len(text)
    while i < n:
        c = text[i]
        if c == open_ch:
            depth += 1
        elif c == close_ch:
            depth -= 1
            if depth == 0:
                return text[start : i + 1]
        elif c == '"' or c == "'":
            # skip string literals to avoid bracket confusion
            quote = c
            i += 1
            while i < n and text[i] != quote:
                if text[i] == "\\":
                    i += 1
                i += 1
        i += 1
    return text[start:]


def parse_card_file(path):
    src = open(path, encoding="utf-8").read()
    m = re.search(r"public\s+(?:abstract\s+|sealed\s+)?class\s+(\w+)", src)
    if not m:
        return None
    cls = m.group(1)
    # base constructor call
    mb = re.search(r":\s*\w+\s*\(", src)
    cost = None
    rarity = None
    if mb:
        args_block = extract_balanced(src, mb.end() - 1)
        mi = re.search(r"-?\d+", args_block)
        if mi:
            cost = int(mi.group(0))
        mr = re.search(r"CardRarity\.(\w+)", args_block)
        if mr:
            rarity = mr.group(1)
    vars_dict = parse_vars(src)
    upgrades = parse_upgrades(src)
    return {"class": cls, "cost": cost, "rarity": rarity, "vars": vars_dict, "upgrades": upgrades}


def parse_vars(src):
    """Extract all ModCardVars.Xxx(...) calls from CanonicalVars / AdditionalCanonicalVars blocks."""
    vars_dict = {}
    for kw in ("CanonicalVars", "AdditionalCanonicalVars"):
        idx = src.find(kw)
        while idx != -1:
            # find first bracket after the member name
            j = src.find("[", idx)
            k = src.find("(", idx)
            if j == -1 and k == -1:
                break
            if j != -1 and (k == -1 or j < k):
                block = extract_balanced(src, j)
            else:
                block = extract_balanced(src, k)
            for call in find_modcardvars_calls(block):
                name, value = call
                if name:
                    vars_dict[name] = value
            # continue searching for more occurrences (e.g., multiple member blocks)
            idx = src.find(kw, idx + len(kw))
    return vars_dict


def parse_upgrades(src):
    """Extract upgrade deltas from OnUpgrade: DynamicVars.X / ["X"] / [nameof(X)].UpgradeValueBy(N).

    A `m` decimal suffix on the literal is tolerated. A call is "power-prefixed" when its
    `DynamicVars` is accessed through a member/power object (e.g. `power.DynamicVars[...]`
    or `_fifthForcePower?.DynamicVars[...]`); that upgrades the power's hover stat, not the
    card description. Resolution prefers the card-level delta; falls back to the power-prefixed
    one only when no card-level upgrade of that name exists.
    """
    card_deltas = {}
    power_deltas = {}
    pats = [
        r"DynamicVars\s*\.\s*(\w+)\s*\.\s*UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)m?\s*\)",
        r'DynamicVars\s*\[\s*"([^"]+)"\s*\]\s*\.\s*UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)m?\s*\)',
        r"DynamicVars\s*\[\s*nameof\(\s*(\w+)\s*\)\s*\]\s*\.\s*UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)m?\s*\)",
    ]
    for pat in pats:
        for m in re.finditer(pat, src):
            name = m.group(1)
            n = float(m.group(2))
            n = int(n) if n.is_integer() else n
            ctx = src[max(0, m.start() - 12) : m.start()]
            power = bool(re.search(r"[\w)\]](?:\?)?\.\s*$", ctx))
            target = power_deltas if power else card_deltas
            target[name] = target.get(name, 0) + n
    deltas = {}
    deltas.update(power_deltas)  # fallback: power-prefixed only
    deltas.update(card_deltas)   # card-level takes priority
    return deltas


def find_modcardvars_calls(block):
    calls = []
    i = block.find("ModCardVars.")
    while i != -1:
        p = block.find("(", i)
        if p == -1:
            break
        args = extract_balanced(block, p)
        method = block[i + len("ModCardVars.") : p]
        name, value = interpret_call(method, args)
        if name:
            calls.append((name, value))
        i = block.find("ModCardVars.", p)
    return calls


def interpret_call(method, args):
    # args includes parentheses
    inner = args[1:-1]
    base_method = re.sub(r"<[^>]*>", "", method)
    # Power<XxxPower>
    if base_method == "Power":
        mg = re.search(r"<(\w+)>", method)
        name = mg.group(1) if mg else None
        value = first_int(inner)
        return name, value
    if base_method in ("Int", "Computed") or base_method.startswith("Computed"):
        sm = re.search(r'"([^"]+)"', inner)
        name = sm.group(1) if sm else None
        value = first_int(inner)
        return name, value
    # typed vars: Damage, Block, Heal, Cards, Energy, HpLoss, Strength, ...
    name = base_method
    value = first_int(inner)
    return name, value


def first_int(s):
    m = re.search(r"-?\d+", s)
    return int(m.group(0)) if m else None


def resolve(text, vars_dict, cost, upgrades=None):
    if upgrades is None:
        upgrades = {}
    if text is None:
        return None
    # strip style tags
    text = re.sub(r"\[gold\](.*?)\[/gold\]", r"\1", text, flags=re.S)
    text = re.sub(r"\[purple\](.*?)\[/purple\]", r"\1", text, flags=re.S)

    def fmt(name):
        base = vars_dict.get(name)
        if base is None:
            return "{" + name + "}"
        delta = upgrades.get(name, 0)
        if delta:
            return f"{base}[{base + delta}]"
        return str(base)

    # energy icons token: {energyPrefix:energyIcons(3)} or {Energy:energyIcons()}
    def repl_energy(m):
        var = m.group(1)
        num = m.group(2)
        if num:
            return f"{int(num)}能量"
        return fmt(var) + "能量"

    text = re.sub(r"\{(\w+):[^{}]*energyIcons\((\d*)\)[^{}]*\}", repl_energy, text)

    # conditionals: repeat until stable (branches may nest)
    prev = None
    while prev != text:
        prev = text
        # IfUpgraded:show:X|Y -> base Y, plus upgraded-only X in 〔〕
        def repl_ifup(m):
            x = m.group(1)
            y = m.group(2)
            return y + ("〔" + x + "〕" if x.strip() else "")

        text = re.sub(r"\{IfUpgraded:show:(.*?)\|(.*?)\}", repl_ifup, text, flags=re.S)
        # InCombat:X|Y -> default (non-combat) = Y
        text = re.sub(
            r"\{InCombat:(.*?)\|(.*?)\}",
            lambda m: m.group(2),
            text,
            flags=re.S,
        )
        # plural: {Var:plural:singular|plural} -> noun by count (base==1 -> singular)
        def repl_plural(m):
            var = m.group(1)
            sing = m.group(2)
            plu = m.group(3)
            return sing if vars_dict.get(var) == 1 else plu

        text = re.sub(
            r"\{(\w+):plural:([^}|]*)\|([^}]*)\}",
            repl_plural,
            text,
            flags=re.S,
        )
        # cond: {Var:cond:=1?TRUE|FALSE}  (FALSE may be {:diff()} with a nested brace)
        def repl_cond_diff(m):
            var = m.group(1)
            op = m.group(2)
            tru = m.group(3)
            val = vars_dict.get(var)
            if op.startswith("="):
                try:
                    if val == int(op[1:]):
                        return tru
                except ValueError:
                    pass
            return fmt(var)

        def repl_cond_lit(m):
            var = m.group(1)
            op = m.group(2)
            tru = m.group(3)
            fal = m.group(4)
            val = vars_dict.get(var)
            if op.startswith("="):
                try:
                    if val == int(op[1:]):
                        return tru
                except ValueError:
                    pass
            return fal

        text = re.sub(
            r"\{(\w+):cond:(=[0-9]+)\?([^{}|]*)\|\{:diff\(\)\}\}",
            repl_cond_diff,
            text,
            flags=re.S,
        )
        text = re.sub(
            r"\{(\w+):cond:(=[0-9]+)\?([^{}|]*)\|([^{}]*)\}",
            repl_cond_lit,
            text,
            flags=re.S,
        )

    # diff / plain var resolution
    def repl_var(m):
        var = m.group(1)
        return fmt(var)

    prev = None
    while prev != text:
        prev = text
        text = re.sub(r"\{(\w+):diff\(\)\}", repl_var, text)
        text = re.sub(r"\{(\w+):inverseDiff\(\)\}", repl_var, text)
        text = re.sub(r"\{(\w+)\}", repl_var, text)

    # cleanup leftover empty braces and spaces
    text = text.replace("{}", "")
    return text


def main():
    zhs = json.load(open(ZHS_PATH, encoding="utf-8"))
    eng = json.load(open(ENG_PATH, encoding="utf-8"))

    rows = []
    unresolved_report = []
    for path in sorted(glob.glob(os.path.join(CARDS_DIR, "*.cs"))):
        info = parse_card_file(path)
        if not info:
            continue
        if info["class"] in SKIP_CLASSES:
            continue
        key = "FGO_CARD_" + re.sub(r"(?<!^)(?=[A-Z])", "_", info["class"]).upper()
        ztitle = zhs.get(key + ".title")
        zdesc = zhs.get(key + ".description")
        etitle = eng.get(key + ".title")
        edesc = eng.get(key + ".description")
        # skip placeholder feedback entries (abstract models / missing localization)
        if ztitle == PLACEHOLDER_FEEDBACK or (ztitle is None and zdesc is None):
            unresolved_report.append((info["class"], "no zhs localization (key=" + key + ")"))
            continue
        r_zh = resolve(zdesc, info["vars"], info["cost"], info.get("upgrades", {}))
        r_en = resolve(edesc, info["vars"], info["cost"], info.get("upgrades", {}))
        # collect unresolved tokens
        for lang, resolved in (("zh", r_zh), ("en", r_en)):
            if resolved:
                toks = re.findall(r"\{(\w+)\}", resolved)
                if toks:
                    unresolved_report.append(
                        (info["class"], f"{lang} unresolved: {sorted(set(toks))}")
                    )
        rarity_zh = RARITY_ZH.get(info["rarity"], info["rarity"] or "")
        rows.append(
            {
                "key": key,
                "name_zh": ztitle,
                "desc_zh": r_zh,
                "cost": info["cost"],
                "rarity": rarity_zh,
                "name_en": etitle,
                "desc_en": r_en,
            }
        )

    # write excel
    from openpyxl import Workbook
    from openpyxl.styles import Alignment, Font, PatternFill

    wb = Workbook()
    ws = wb.active
    ws.title = "卡牌"
    headers = ["卡名", "描述", "费用", "稀有度", "英文卡名(title)", "英文描述(description)"]
    ws.append(headers)
    head_fill = PatternFill("solid", fgColor="4472C4")
    for c in ws[1]:
        c.font = Font(bold=True, color="FFFFFF")
        c.fill = head_fill
        c.alignment = Alignment(vertical="center", horizontal="center")
    for r in rows:
        ws.append(
            [r["name_zh"], r["desc_zh"], r["cost"], r["rarity"], r["name_en"], r["desc_en"]]
        )
    # column widths & wrap
    widths = [22, 60, 8, 10, 28, 70]
    for i, w in enumerate(widths, start=1):
        ws.column_dimensions[chr(64 + i)].width = w
    for row in ws.iter_rows(min_row=2):
        for cell in row:
            cell.alignment = Alignment(vertical="top", wrap_text=True)
    ws.freeze_panes = "A2"

    wb.save(OUT_PATH)
    print(f"导出完成：{OUT_PATH}")
    print(f"卡牌行数：{len(rows)}")
    print(f"未解析项：{len(unresolved_report)}")
    for item in unresolved_report:
        print("  -", item[0], ":", item[1])


if __name__ == "__main__":
    main()
