using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public static class FriendlyDateHelper
    {
        public static string TodaysFriendlyDate()
        {
            var today = DateTime.Today;
            string formattedDate = $"Today, {today.ToString("dddd d, MMMM, yyyy}", CultureInfo.InvariantCulture)}";
            return formattedDate;
        }

        public static string ThisWeeksFriendlyDate()
        {
            var today = DateTime.Today;
            // get week number by breaking the month into 7 day blocks
            int weekNumber = (today.Day - 1) / 7 + 1; // 1-based week number

            string monthYear = today.ToString("MMMM, yyyy", CultureInfo.InvariantCulture);
            string formattedDate = $"Week {weekNumber} of {monthYear}";
            return formattedDate;
        }

        public static string ThisMonthsFriendlyDate()
        {
            var today = DateTime.Today;
            string formattedDate = $"Month of {today.ToString("MMMM, yyyy", CultureInfo.InvariantCulture)}";
            return formattedDate;
        }

        public static string TodaysFriendlySalesDate()
        {
            var today = DateTime.Today;
            string formattedDate = $"Today, {today.ToString("dddd d, MMMM, yyyy", CultureInfo.InvariantCulture)} Sales";
            return formattedDate;
        }

        public static string ThisWeeksFriendlySalesDate()
        {
            var today = DateTime.Today;
            // get week number by breaking the month into 7 day blocks
            int weekNumber = (today.Day - 1) / 7 + 1; // 1-based week number
            string friendlyWeek = weekNumber switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                4 => "4th",
                _ => "5th"
            } + " Week";

            string monthYear = today.ToString("MMMM, yyyy", CultureInfo.InvariantCulture);
            string formattedDate = $"Sales of the {friendlyWeek} of {monthYear}";
            return formattedDate;
        }

        public static string WeeklySalesTimeSpan(int week, int month, int year)
        {
            string friendlyWeek = week switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                4 => "4th",
                _ => "5th"
            } + " Week";

            string monthYear = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}";
            string formattedDate = $"{friendlyWeek} week of {monthYear}";
            return formattedDate;
        }

        public static string ThisMonthsFriendlySalesDate()
        {
            var today = DateTime.Today;
            string formattedDate = $"Sales of {today.ToString("MMMM, yyyy", CultureInfo.InvariantCulture)}";
            return formattedDate;

        }

        public static string MonthlySalesTimeSpan(int month, int year)
        {
            string formattedDate = $"Month of {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}";
            return formattedDate;

        }

        

        public static string GetFriendlyTimespan(DateTime firstDate, DateTime lastDate, int itemCount)
        {                     
            DateTime earlier = firstDate < lastDate ? firstDate : lastDate;
            DateTime later = firstDate < lastDate ? lastDate : firstDate;

            TimeSpan timeSpan = later.Subtract(earlier);

            // Format timespan based on duration
            if (timeSpan.TotalSeconds < 60)
            {
                return "A few moments ago";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                if (itemCount == 1)
                {
                    return $"{minutes} minute{(minutes > 1 ? "s" : "")} since last sale and now";
                }
                return $"{minutes} minute{(minutes > 1 ? "s" : "")} passed between last two sales";
            }
            else if (timeSpan.TotalHours < 24)
            {
                int hours = (int)timeSpan.TotalHours;
                int minutes = timeSpan.Minutes;
                if (minutes > 0)
                {
                    if (itemCount == 1)
                    {
                        return $"{hours} hour{(hours > 1 ? "s" : "")} and {minutes} minute{(minutes > 1 ? "s" : "")} since last sale and now";
                    }
                    return $"{hours} hour{(hours > 1 ? "s" : "")} and {minutes} minute{(minutes > 1 ? "s" : "")} passed between last two sales";
                }
                else
                {
                    if (itemCount == 1)
                    {
                        return $"{hours} hour{(hours > 1 ? "s" : "")} since last sale and now";
                    }
                    return $"{hours} hour{(hours > 1 ? "s" : "")} passed between last two sales";
                }
            }
            else
            {
                int days = (int)timeSpan.TotalDays;
                return $"{days} day{(days > 1 ? "s" : "")} ago";
            }
        }

        public static string LastSaleTimespan(DateTime firstDate, DateTime lastDate)
        {
            DateTime earlier = firstDate < lastDate ? firstDate : lastDate;
            DateTime later = firstDate < lastDate ? lastDate : firstDate;

            TimeSpan timeSpan = later.Subtract(earlier);


            // Format timespan based on duration
            if (timeSpan.TotalSeconds < 60)
            {
                return "A few moments ago";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                int minutes = (int)timeSpan.TotalMinutes;
               
                return $"Last sale was {minutes} minute{(minutes > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                int hours = (int)timeSpan.TotalHours;
                int minutes = timeSpan.Minutes;
                if (minutes > 0)
                {                   
                    return $"Last sale: {hours} hour{(hours > 1 ? "s" : "")} and {minutes} minute{(minutes > 1 ? "s" : "")} ago";
                }
                else
                {
                   
                    return $"Last sale: {hours} hour{(hours > 1 ? "s" : "")} ago ";
                }
            }
            else
            {
                int days = (int)timeSpan.TotalDays;
                return $"Last sale: {days} day{(days > 1 ? "s" : "")} ago";
            }
        }
    }
}
