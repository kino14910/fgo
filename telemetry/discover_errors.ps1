$ErrorActionPreference = 'Stop'

$here = $PSScriptRoot

# 报错查询的唯一清单：口径（本地化/运行时）× 视图（计数/聚合/明细）。
# 改口径只改对应 sql 文件；增删视图只改这张表 + 落一个 sql 文件，两处不会漂移。
$queries = @(
    @{ Title = "近 24h FGO_ 本地化错误 · 计数";          File = "localization_count.sql" },
    @{ Title = "近 24h FGO_ 本地化错误 · 按 key 聚合";   File = "localization_by_key.sql" },
    @{ Title = "近 24h FGO_ 本地化错误 · 明细";          File = "localization_detail.sql" },
    @{ Title = "近 24h 堆栈含 Fgo/FGO 运行时异常 · 计数"; File = "runtime_count.sql" },
    @{ Title = "近 24h 堆栈含 Fgo/FGO 运行时异常 · 按类型聚合"; File = "runtime_by_type.sql" },
    @{ Title = "近 24h 堆栈含 Fgo/FGO 运行时异常 · 明细"; File = "runtime_detail.sql" }
)

foreach ($q in $queries) {
    Write-Host ""
    Write-Host "=== $($q.Title) ===" -ForegroundColor Cyan
    & (Join-Path $here "run_query.ps1") -SqlFile (Join-Path $here $q.File)
}
