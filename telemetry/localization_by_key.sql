-- 近 24h FGO_ 本地化错误 · 按缺失 key 聚合
-- 与 localization_count.sql 同口径；msg 即缺失的本地化 key，按出现次数排序定位高频缺失项
SELECT
    properties.payload.applicant_payload.exception.message AS msg,
    count() AS cnt
FROM events
WHERE event = 'exception'
  AND properties.exception_type = 'MegaCrit.Sts2.Core.Localization.LocException'
  AND properties.payload.applicant_payload.exception.message LIKE '%FGO_%'
  AND timestamp >= now() - INTERVAL 24 HOUR
GROUP BY msg
ORDER BY cnt DESC
LIMIT 30
