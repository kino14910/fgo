-- 近 24h 堆栈含 Fgo/FGO 的运行时异常 · 明细
-- 识别依据：exception.stack_trace 里出现 Fgo/FGO（本地化错误单独用 localization_*.sql）
-- 注意 ILIKE 大小写不敏感，单个 '%fgo%' 即覆盖 FGO/Fgo/fgo，勿写两份 OR
SELECT
    timestamp,
    properties.exception_type AS etype,
    properties.payload.applicant_payload.exception.message AS msg,
    properties.payload.applicant_payload.exception.stack_trace AS st
FROM events
WHERE event = 'exception'
  AND properties.exception_type != 'MegaCrit.Sts2.Core.Localization.LocException'
  AND properties.payload.applicant_payload.exception.stack_trace ILIKE '%fgo%'
  AND timestamp >= now() - INTERVAL 24 HOUR
ORDER BY timestamp DESC
LIMIT 30
