"""检查本地化描述里引用的 {Var} 是否都在对应卡/能力的 CanonicalVars 里声明过。

背景：卡面字符串里的 `{Xxx}` 由 SmartFormat 用卡牌自己的 DynamicVarSet 求值。
只要描述里引用了没声明的变量，运行时就会抛
    [ERROR] Localization formatting error! ... No source extension could handle the selector named "Xxx"
这是**运行期**错误、编译期发现不了，很容易漏到游戏里。

用法：
    python tools/check_card_vars.py                # 全量检查
    python tools/check_card_vars.py LAST_RESORT    # 只查名字里含这些片段的键
    python tools/check_card_vars.py KURAKURAS YAKISOBA

退出码：有问题为 1，干净为 0。
"""

import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 游戏/CardModel 自带、不需要自己声明的变量与格式化器。
# 报错信息里的 variables={...} 会列出全部可用名，遇到新的内置名补到这里。
BUILTIN = {
    # 通用上下文
    "IfUpgraded", "OnTable", "InCombat", "IsTargeting", "TargetType", "GainsBlock",
    "IsOstyAlive", "energyPrefix", "singleStarIcon", "energyIcons",
    # 常见默认变量名（DynamicVarSet 上的强类型属性）
    "Damage", "Block", "Heal", "HpLoss", "MaxHp", "Cards", "Energy", "Stars",
    "Gold", "Repeat", "Forge", "Summon", "OstyDamage", "Dexterity", "Doom",
    "Poison", "Target", "Target2", "Target3",
    # 本项目冷却卡基类注入的
    "Cooldown", "CooldownMax",
    # PowerModel 自带（能力的 {Amount} 不需要自己声明）
    "Amount",
}

# ModCardVars.Xxx("Name", ...) 里显式命名的类型
NAMED_VAR_TYPES = (
    "Int", "String", "Bool", "Cards", "Gold", "Heal", "HpLoss", "MaxHp",
    "Repeat", "Forge", "Summon", "Energy", "Stars", "Damage", "Block",
    "OstyDamage", "Computed",
)
# ModCardVars.Xxx(值) 里不命名、名字固定的类型
UNNAMED_VAR_TYPES = (
    "Heal", "Damage", "Block", "Stars", "Energy", "Cards", "Gold",
    "HpLoss", "MaxHp", "Repeat", "Forge", "Summon", "OstyDamage",
)


def declared_vars(path):
    src = open(path, encoding="utf-8").read()
    names = set()

    named = "|".join(NAMED_VAR_TYPES)
    for m in re.finditer(r'ModCardVars\.(?:%s)\(\s*"([^"]+)"' % named, src):
        names.add(m.group(1))

    unnamed = "|".join(UNNAMED_VAR_TYPES)
    for m in re.finditer(r'ModCardVars\.(%s)\(\s*[^,"\)]' % unnamed, src):
        names.add(m.group(1))

    # Computed* 系列：ModCardVars.Computed("X", ...) / ComputedDamage("X", ...) / ComputedPower<T>("X", ...)
    for m in re.finditer(r'ModCardVars\.Computed\w*(?:<[^>]*>)?\(\s*"([^"]+)"', src):
        names.add(m.group(1))

    # Power<TPower>(...) -> TPower；Power<TPower>("X", ...) -> X
    for m in re.finditer(r"ModCardVars\.Power<(\w+)>\(\s*\"([^\"]+)\"", src):
        names.add(m.group(2))
    for m in re.finditer(r"ModCardVars\.Power<(\w+)>\(\s*\d", src):
        names.add(m.group(1))

    return names


def key_to_class(key):
    body = key.split(".")[0].replace("FGO_CARD_", "").replace("FGO_POWER_", "")
    return "".join(w.capitalize() for w in body.split("_"))


def main():
    filters = [a.upper() for a in sys.argv[1:]]

    cls_vars = {}
    for p in glob.glob(os.path.join(ROOT, "Scripts", "**", "*.cs"), recursive=True):
        src = open(p, encoding="utf-8").read()
        # 一个文件里可能声明多个类（基类 + 派生类），全部登记，避免"找不到类"的误报。
        for m in re.finditer(r"class\s+(\w+)", src):
            cls_vars[m.group(1)] = declared_vars(p)

    tables = [
        ("zhs/cards", os.path.join(ROOT, "Fgo/localization/zhs/cards.json")),
        ("eng/cards", os.path.join(ROOT, "Fgo/localization/eng/cards.json")),
        ("zhs/powers", os.path.join(ROOT, "Fgo/localization/zhs/powers.json")),
        ("eng/powers", os.path.join(ROOT, "Fgo/localization/eng/powers.json")),
    ]

    bad = []
    for name, path in tables:
        tbl = json.load(open(path, encoding="utf-8"))
        for k, v in tbl.items():
            if filters and not any(f in k for f in filters):
                continue
            for sel in re.findall(r"\{([A-Za-z_]\w*)", v or ""):
                if sel in BUILTIN:
                    continue
                cls = key_to_class(k)
                if cls in cls_vars and sel in cls_vars[cls]:
                    continue
                bad.append("%-11s %-58s {%s}  class=%s vars=%s"
                           % (name, k, sel, cls, sorted(cls_vars.get(cls, ["<找不到类>"]))))

    if bad:
        print("MISSING %d:" % len(bad))
        for b in sorted(set(bad)):
            print("  ", b)
        return 1

    print("OK: 描述里引用的变量都有声明（%s）" % (", ".join(filters) if filters else "全量"))
    return 0


if __name__ == "__main__":
    sys.exit(main())
