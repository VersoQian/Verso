using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.Utilities
{
    public static class DateUtils
    {
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");

        public static string DateToHHMMSS(this TimeSpan date)
        {
            return date.ToString(@"hh\:mm\:ss", Culture);
        }

        public static string DateToTotalHHMMSS(this TimeSpan time)
        {
            return $"{Math.Floor(time.TotalHours):00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        public static string DateToMMSS(this TimeSpan date)
        {
            return date.ToString(@"mm\:ss", Culture);
        }

        public static string TimeToMS(this TimeSpan time)
        {
            return $"{time.Minutes:00m} {time.Seconds:00s}";
        }

        public static string TimeToHM(this TimeSpan time)
        {
            return $"{time.Hours:00h} {time.Minutes:00m}";
        }

        public static string TimeToH(this TimeSpan time)
        {
            return $"{time.Hours:0h}";
        }
        public static string TimeToM(this TimeSpan time)
        {
            return $"{time.Minutes:00m}";
        }

        public static string TimeToTotalHM(this TimeSpan time)
        {
            var minutes = string.Empty;
            if (time.Hours == 0)
            {
                minutes = Math.Ceiling(time.TotalMinutes).ToString("00m");
            }
            else
            {
                minutes = time.Minutes.ToString("00m");
            }
            return $"{Math.Floor(time.TotalHours):00h} {minutes}";
        }

        public static string TimeToHMS(this TimeSpan time)
        {
            return $"{time.Hours:00h} {time.Minutes:00m} {time.Seconds:00s}";
        }

        public static DateTime LoadDate(string key)
        {
            var dateString = PlayerPrefs.GetString(key, DateTime.Now.ToBinary().ToString());
            var dateLong = Convert.ToInt64(dateString);
            return DateTime.FromBinary(dateLong);
        }

        public static void SaveDate(string key, DateTime date)
        {
            PlayerPrefs.SetString(key, date.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }
}