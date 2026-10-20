"""Push the FGO card table to the remote Tencent Doc sheet.

Usage:
    python import_to_tdoc.py            # export from source -> push to remote -> verify
    python import_to_tdoc.py --dry      # export only, no remote write
"""

import importlib.util
import json
import os
import re
import sys

ROOT = "D:/projects/fgo"
FILE_URL = "https://docs.qq.com/sheet/DSGR3bE1zZkJoR1Jh"
SHEET_ID = "qbPkXC"

HEADERS = [
    "卡名", "描述", "费用", "稀有度", "类型",
    "英文卡名(title)", "英文描述(description)",
    "日文卡名(title)", "日文描述(description)",
]


def load_export_cards():
    spec = importlib.util.spec_from_file_location(
        "export_cards", os.path.join(ROOT, "export_cards.py")
    )
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def find_tencentdocs():
    """Locate the newest installed tencent-docs-plugin skill dir (version changes often)."""
    base = "C:/Users/14910/.workbuddy/plugins/cache/workbuddy-builtin"
    cands = glob_paths(os.path.join(base, "tencent-docs-plugin", "*", "skills", "tencent-docs"))
    if not cands:
        raise SystemExit("tencent-docs plugin not found under " + base)

    def verkey(p):
        m = re.search(r"tencent-docs-plugin/(\d+)\.(\d+)\.(\d+)", p.replace("\\", "/"))
        return tuple(int(x) for x in m.groups()) if m else (0, 0, 0)

    cands.sort(key=verkey)
    return cands[-1]


def glob_paths(pattern):
    import glob

    return glob.glob(pattern)


def build_values(rows):
    values = []
    for c, h in enumerate(HEADERS):
        values.append({"row": 0, "col": c, "value_type": "STRING", "string_value": h})
    for i, r in enumerate(rows, start=1):
        cells = [
            (r["name_zh"] or "", "STRING"),
            (r["desc_zh"] or "", "STRING"),
            (r["cost"], "NUMBER"),
            (r["rarity"] or "", "STRING"),
            (r["type"] or "", "STRING"),
            (r["name_en"] or "", "STRING"),
            (r["desc_en"] or "", "STRING"),
            (r["name_jp"] or "", "STRING"),
            (r["desc_jp"] or "", "STRING"),
        ]
        for c, (val, kind) in enumerate(cells):
            cell = {"row": i, "col": c}
            if kind == "NUMBER" and val is not None:
                cell["value_type"] = "NUMBER"
                cell["number_value"] = val
            else:
                cell["value_type"] = "STRING"
                cell["string_value"] = str(val) if val is not None else ""
            values.append(cell)
    return values


def main():
    dry = "--dry" in sys.argv
    ec = load_export_cards()
    rows, unresolved = ec.collect_rows(ec.build_global_const_table())
    print("卡牌行数：", len(rows))
    print("未解析项：", len(unresolved))
    for item in unresolved:
        print("  -", item[0], ":", item[1])
    if unresolved:
        raise SystemExit("abort: unresolved placeholders remain")
    if dry:
        return

    sys.path.insert(0, find_tencentdocs())
    import tencentdocs as td

    values = build_values(rows)
    print("写入单元格数：", len(values))
    res, err = td.call_tool("sheet-mcp", "set_range_value", {
        "file_url": FILE_URL, "sheet_id": SHEET_ID, "values": values,
    })
    print("set_range_value err:", err)

    # keep the filter covering the whole (possibly grown/shrunk) table
    res2, err2 = td.call_tool("sheet-mcp", "update_filter", {
        "file_url": FILE_URL, "sheet_id": SHEET_ID, "is_update_range": True,
        "start_row": 0, "start_col": 0, "end_row": len(rows), "end_col": len(HEADERS) - 1,
    })
    print("update_filter err:", err2)

    # verify: card-name column has exactly len(rows) non-empty cells, header is 9 wide
    res3, err3 = td.call_tool("sheet-mcp", "get_cell_data", {
        "file_url": FILE_URL, "sheet_id": SHEET_ID,
        "start_row": 0, "start_col": 0, "end_row": len(rows) + 5, "end_col": 0,
        "return_csv": True,
    })
    names = []
    for line in json.loads(res3["result"]["content"][0]["text"])["csv_data"].split("\n")[1:]:
        if line.strip():
            names.append(line.strip().strip('"'))
    print("verify -> remote data rows:", len(names), "| first:", names[0] if names else None,
          "| last:", names[-1] if names else None)

    res4, err4 = td.call_tool("sheet-mcp", "get_cell_data", {
        "file_url": FILE_URL, "sheet_id": SHEET_ID,
        "start_row": 0, "start_col": 0, "end_row": 0, "end_col": 8, "return_csv": True,
    })
    print("verify -> header:", json.loads(res4["result"]["content"][0]["text"])["csv_data"])


if __name__ == "__main__":
    main()
