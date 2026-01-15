using System.Runtime.InteropServices;

namespace FindBearingsApi.Application.Common
{
    public static class DateTimeHelper
    {
        // 可在测试中替换
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;

        // 缓存中国时区（启动时初始化）
        private static readonly TimeZoneInfo _chinaTimeZone =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")
                : TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");

        /// <summary>
        /// 获取当前中国标准时间（UTC+8）
        /// </summary>
        public static DateTime BeijingNow =>
            TimeZoneInfo.ConvertTimeFromUtc(UtcNow(), _chinaTimeZone);

        /// <summary>
        /// 将 UTC 时间转换为中国标准时间（UTC+8）
        /// </summary>
        public static DateTime ToChinaStandardTime(this DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Input must be UTC time.", nameof(utcTime));
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _chinaTimeZone);
        }

        /// <summary>
        /// 将 UTC 时间转换为指定时区的时间
        /// </summary>
        public static DateTime ToTimeZone(this DateTime utcTime, string timeZoneId)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Input must be UTC time.", nameof(utcTime));
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
        }
    }
}
