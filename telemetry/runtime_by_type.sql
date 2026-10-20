-- 近 24h 堆栈含 Fgo/FGO 的运行时异常 · 按异常类型聚合
-- 与 runtime_count.sql 同口径；etype 为异常类型全名，按次数排序定位高频崩溃点
SELECT
    properties.exception_type AS etype,
    count() AS cnt
FROM events
WHERE event = 'exception'
  AND properties.exception_type != 'MegaCrit.Sts2.Core.Localization.LocException'
  AND properties.payload.applicant_payload.exception.stack_trace ILIKE '%fgo%'
  AND timestamp >= now() - INTERVAL 24 HOUR
GROUP BY etype
ORDER BY cnt DESC
LIMIT 30
