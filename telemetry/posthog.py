import glob, json, os, re, shutil, subprocess

# PostHog API CLI 的唯一调用入口（Python 侧）。
# 与 run_query.ps1 走同一条路径：PATH 上 pnpm/npm 全局安装的 posthog-cli；
# 区别是 Python 不走 shell，直接 node + run-posthog-cli.js，参数用 list 传递，
# 不存在 PowerShell->cmd 的 JSON 引号吞噬问题。
# run-posthog-cli.js 的路径全部动态解析，不硬编码 pnpm 哈希目录。

NODE = "node"


def _from_env():
    p = os.environ.get("POSTHOG_CLI_JS")
    return p if p and os.path.exists(p) else None


def _from_pnpm_global():
    # pnpm 全局布局：<LOCALAPPDATA>/pnpm/global/v11/<哈希>/node_modules/@posthog/cli/
    # 哈希会变，通配符匹配；取排序最后一个（重装后新目录字典序更大）
    local = os.environ.get("LOCALAPPDATA", "")
    hits = sorted(glob.glob(os.path.join(
        local, "pnpm", "global", "v11", "*", "node_modules",
        "@posthog", "cli", "run-posthog-cli.js")))
    return hits[-1] if hits else None


def _from_shim():
    # 兜底：从 PATH 上的 posthog-cli shim 文件里提取 js 路径。
    # shim 由 pnpm/npm 在（重）安装时生成，哈希永远是新的，适合全局目录搬家的场景
    for name in ("posthog-cli.CMD", "posthog-cli.ps1", "posthog-cli"):
        shim = shutil.which(name)
        if not shim:
            continue
        try:
            with open(shim, encoding="utf-8", errors="replace") as f:
                text = f.read()
        except OSError:
            continue
        m = re.search(r"[^\s\"']*run-posthog-cli\.js", text)
        if not m:
            continue
        p = m.group(0)
        base = os.path.dirname(shim)
        p = p.replace("%~dp0", base + os.sep)
        p = p.replace("$basedir", base.replace("\\", "/") + "/")
        if os.path.exists(p):
            return p
    return None


def _cli_js():
    for finder in (_from_env, _from_pnpm_global, _from_shim):
        p = finder()
        if p:
            return p
    raise RuntimeError(
        "找不到 @posthog/cli 的 run-posthog-cli.js。"
        "请先 pnpm add -g @posthog/cli，或用 POSTHOG_CLI_JS 环境变量显式指定该 js 路径")


def run_query(sql, truncate=True):
    body = {"query": sql}
    if not truncate:
        body["truncate"] = False
    payload = json.dumps(body, ensure_ascii=False)
    cmd = [NODE, _cli_js(), "api", "call", "--json", "execute-sql", payload]
    r = subprocess.run(cmd, capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if r.returncode != 0:
        raise RuntimeError("STDOUT:\n%s\nSTDERR:\n%s" % (r.stdout, r.stderr))
    out = json.loads(r.stdout.strip())
    # CLI 直接返回表格字符串；新版包一层 {"results": "..."}（旧版为 {"result": "..."}），统一解包
    if isinstance(out, dict):
        for key in ("results", "result"):
            if isinstance(out.get(key), str):
                out = out[key]
                break
    return out
