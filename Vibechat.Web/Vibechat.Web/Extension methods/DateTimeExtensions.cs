using System;

namespace Vibechat.Web.Services.Extension_methods
{
    public static class DateTimeExtensions
    {
        public static string ToUTCString(this DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Local:
                {
                    return value.ToUniversalTime().ToString("o");
                }

                case DateTimeKind.Unspecified:
                {
                    DateTime.SpecifyKind(value, DateTimeKind.Utc);
                    return value.ToString("o") + "Z";
                }

                case DateTimeKind.Utc:
                {
                    return value.ToString("o");
                }

                default:
                {
                    return value.ToString("o");
                }
            }
        }
    }
}