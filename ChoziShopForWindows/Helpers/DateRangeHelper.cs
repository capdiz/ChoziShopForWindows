using ChoziShopForWindows.Enums;
using ChoziShopForWindows.models;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Helpers
{
    public class DateRangeHelper
    {
        public List<DateRangeOption> GetDateRangeOptions(int monthsBack = 6)
        {
            var today = DateTime.Today;
            var options = new List<DateRangeOption>();

            // Add today's date
            options.Add(new DateRangeOption
            {
                DisplayName = "Today",
                StartDate = today,
                EndDate = today.AddDays(1).AddTicks(-1),
                GroupByScope = GroupByScope.Day
            });

            // Add this week
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekend = weekStart.AddDays(6).AddDays(1).AddTicks(-1);
            options.Add(new DateRangeOption
            {
                DisplayName = "This Week",
                StartDate = weekStart,
                EndDate = weekend,
                GroupByScope = GroupByScope.Week
            });

            // Add this month
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1).AddTicks(-1);
            options.Add(new DateRangeOption
            {
                DisplayName = "This Month",
                StartDate = monthStart,
                EndDate = monthEnd,
                GroupByScope = GroupByScope.Month
            });

            // Add the last 7 days
            options.Add(new DateRangeOption
            {
                DisplayName = "Previous 7 Days",
                StartDate = today.AddDays(-6),
                EndDate = today.AddDays(1).AddTicks(-1),
                GroupByScope = GroupByScope.Day
            });

            // Add the last 30 days
            options.Add(new DateRangeOption
            {
                DisplayName = "Previous 30 Days",
                StartDate = today.AddDays(-29),
                EndDate = today.AddDays(1).AddTicks(-1),
                GroupByScope = GroupByScope.Day
            });

            // Add the last 6 months
            for(int i =1;i<monthsBack; i++)
            {
                var month = today.AddMonths(-i);
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);
                options.Add(new DateRangeOption
                {
                    DisplayName = $"Last {i + 1} Month(s)",
                    StartDate = startDate,
                    EndDate = endDate,
                    GroupByScope = GroupByScope.Month
                });
            }

            // Add this year
            var yearStart = new DateTime(today.Year, 1, 1);
            var yearEnd = new DateTime(today.Year, 12, 31, 23, 59, 59, 999);
            options.Add(new DateRangeOption
            {
                DisplayName = "This Year",
                StartDate = yearStart,
                EndDate = yearEnd,
                GroupByScope = GroupByScope.Year
            });

            // Add last 2 years
            for (int i = 1; i <= 2; i++)
            {
                var year = today.Year-i;
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);
                options.Add(new DateRangeOption
                {
                    DisplayName = $"Last {i + 1} Year(s)",
                    StartDate = startDate,
                    EndDate = endDate,
                    GroupByScope = GroupByScope.Year
                });
            }

            return options;
        }
    }
}
