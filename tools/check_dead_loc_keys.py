import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
LOC_DIR = os.path.join(ROOT, "Fgo", "localization")

CATEGORIES = (
    "CARD_", "POWER_", "RELIC_", "MONSTERS_", "MONSTER_", "EVENT_", "POTION_",
    "ORBS_", "ORB_", "CHARACTER_", "NP_", "CARDPILE_", "KEYWORD_",
    "GAMEPLAY_UI_", "SETTINGS_UI_", "STATIC_HOVER_TIPS_", "TUTORIALS_",
    "GAMEPLAY_", "SETTINGS_", "STATIC_", "TUTORIAL_",
)
DYNAMIC_PREFIXES = (
    # FgoFieldId.LocKey 运行时拼接：$"FGO_STATIC_HOVER_TIPS_FIELD_{枚举名大写}"，
    # 枚举在 FgoFieldId.cs，键必然查不到字面量，勿判死
    "FGO_STATIC_HOVER_TIPS_FIELD_",
)
CORPUS_EXTS = (".cs", ".tscn", ".tres", ".gd", ".py")


def load_loc_keys():
    keys = {}
    for f in sorted(glob.glob(os.path.join(LOC_DIR, "*", "*.json"))):
        lang = os.path.basename(os.path.dirname(f))
        with open(f, encoding="utf-8") as fh:
            for k in json.load(fh):
                keys.setdefault(k, set()).add(lang)
    return keys


def gather_corpus():
    texts = []
    for base in ("Scripts", "Fgo", "tools"):
        base_dir = os.path.join(ROOT, base)
        for dirpath, dirnames, files in os.walk(base_dir):
            if "localization" in dirpath.replace("\\", "/").split("/"):
                continue
            dirnames[:] = [d for d in dirnames if d != "localization"]
            for fn in files:
                if fn.endswith(CORPUS_EXTS):
                    try:
                        with open(os.path.join(dirpath, fn), encoding="utf-8",
                                  errors="replace") as fh:
                            texts.append(fh.read())
                    except OSError:
                        pass
    return texts


def main():
    keys = load_loc_keys()
    corpus = gather_corpus()
    print("本地化键 %d 个，引用语料 %d 个文件" % (len(keys), len(corpus)))

    dead = []
    skipped = 0
    for key in sorted(keys):
        root = key.split(".")[0]
        if not root.startswith("FGO_"):
            continue
        if root.startswith(DYNAMIC_PREFIXES):
            skipped += 1
            continue
        if any(root in t for t in corpus):
            continue
        ident = root[len("FGO_"):]
        for cat in CATEGORIES:
            if ident.startswith(cat):
                ident = ident[len(cat):]
                break
        if not ident:
            continue
        pat = r"[\s_\-]*".join(re.escape(p) for p in ident.split("_"))
        if any(re.search(pat, t, re.IGNORECASE) for t in corpus):
            continue
        dead.append((key, sorted(keys[key])))

    if not dead:
        print("未发现死键（跳过动态拼接键 %d 个）" % skipped)
        return
    print("可疑死键 %d 个（跳过动态拼接键 %d 个）：" % (len(dead), skipped))
    for key, langs in dead:
        print("  %s  [%s]" % (key, ",".join(langs)))


if __name__ == "__main__":
    sys.exit(main())
