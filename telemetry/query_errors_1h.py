from posthog import run_query

# print("=== 近1小时报错按类型统计 ===")
# out = run_query(
#     "SELECT properties.exception_type, count() AS cnt "
#     "FROM events WHERE event = 'exception' "
#     "AND timestamp >= now() - INTERVAL 1 HOUR "
#     "GROUP BY properties.exception_type ORDER BY cnt DESC"
# )
# print(out)

print()
print("=== 近1小时报错明细（最新在前） ===")
out = run_query(
    "SELECT timestamp, properties.exception_type, "
    "properties.payload.applicant_payload.exception.message AS msg "
    "FROM events WHERE event = 'exception' "
    "AND timestamp >= now() - INTERVAL 1 HOUR "
    "ORDER BY timestamp DESC LIMIT 200",
    truncate=False,
)
print(out)
