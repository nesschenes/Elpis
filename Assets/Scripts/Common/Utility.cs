using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elpis
{
    public static class Utility
    {
        private static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();

        public static double GetUnixTime(this DateTime datetime)
        {
            return datetime.ToUniversalTime().Subtract(UnixTimeStart).TotalSeconds;
        }

        public static DateTime GetDateTime(double unixtime)
        {
            try
            {
                return UnixTimeStart.AddSeconds(unixtime);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 回傳 Y/M/D 格式
        /// </summary>
        /// <param name="_unixtime">時間戳記</param>
        /// <returns>2017/12/31</returns>
        public static string GetDateText(double _unixtime)
        {
            return GetDateTime(_unixtime).ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// 回傳 Y/M/D Hr：Min 格式
        /// </summary>
        /// <param name="_unixtime">時間戳記</param>
        /// <returns>2017/12/31 23：59</returns>
        public static string GetDateTimeText(double _unixtime)
        {
            return GetDateTime(_unixtime).ToString("yyyy/MM/dd/ tt h:mm");
        }

        /// <summary>
        /// 回傳 Hr：Min：Sec 格式
        /// </summary>
        /// <param name="second">總秒數</param>
        /// <returns>49：59：59</returns>
        public static string GetNormalizedTimeText(int _second)
        {
            int hr = _second / 3600;
            int min = _second % 3600 / 60;
            int sec = _second % 60;

            return (string.Format("{0}：{1}：{2}", hr.ToString(), min.ToString("00"), sec.ToString("00")));
        }
    }
}
