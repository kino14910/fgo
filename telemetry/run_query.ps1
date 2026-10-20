param(
    [Parameter(Mandatory = $true)]
    [string]$SqlFile
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SqlFile)) {
    throw "SQL 文件不存在: $SqlFile"
}

# 三个坑都踩过，勿简化回去：
# 1) -Encoding utf8：PS 5.1 的 Get-Content 默认按 ANSI 读，中文注释会乱码
# 2) "$(...)" 插值：Get-Content -Raw 的返回值挂着 FileSystem 提供程序的 NoteProperties
#    （PSPath/PSDrive 等），直接进 ConvertTo-Json 会序列化成 {"query":{"value":...}}
#    而不是 {"query":"..."}，CLI 报 missing required parameter: query
# 3) 非 ASCII 转 \uXXXX + 双写引号：posthog-cli 在 PATH 上是 .CMD，命令行经 cmd.exe
#    按 ANSI 代码页转码，GBK 汉字尾字节可能是 0x22/0x5C，混进 JSON 会打坏结构；
#    PS 5.1 向原生命令传参还会剥掉双引号，需双写。两步之后命令行是纯 ASCII 才稳。
$sql = "$(Get-Content -Raw -Encoding utf8 -LiteralPath $SqlFile)"
$body = @{ query = $sql } | ConvertTo-Json -Compress
$ascii = -join ($body.ToCharArray() | ForEach-Object {
    if ([int]$_ -gt 127) { '\u{0:x4}' -f [int]$_ } else { $_ }
})
$arg = $ascii -replace '"', '""'

Write-Host ""
Write-Host "=== SQL ===" -ForegroundColor Cyan
Write-Host $sql.Trim()

Write-Host ""
Write-Host "=== Result ===" -ForegroundColor Cyan

try {
    $out = posthog-cli api call --json execute-sql $arg 2>&1
}
catch {
    Write-Host $_
    exit 1
}
$text = $out -join "`n"

try {
    $parsed = $text | ConvertFrom-Json

    $result = if ($parsed -is [string]) {
        $parsed
    }
    elseif ($null -ne $parsed.result) {
        $parsed.result
    }
    else {
        $text
    }

    if ($result -is [string]) {
        # PostHog 返回的是 "a\nb\nc" 这种字符串
        $result = $result -replace '\\n', "`n"
        $result = $result -replace '\\"', '"'
    }

    Write-Host $result
}
catch {
    Write-Host $text
}
