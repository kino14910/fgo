-- 近 24h FGO_ 本地化错误 · 明细
-- 识别依据：exception.message 里含 FGO_ 前缀的 key（本地化错误无堆栈，不能用 stack_trace）
SELECT
    timestamp,
    properties.payload.applicant_payload.exception.message AS msg
FROM events
WHERE event = 'exception'
  AND properties.exception_type = 'MegaCrit.Sts2.Core.Localization.LocException'
  AND properties.payload.applicant_payload.exception.message LIKE '%FGO_%'
  AND timestamp >= now() - INTERVAL 24 HOUR
ORDER BY timestamp DESC
LIMIT 30
