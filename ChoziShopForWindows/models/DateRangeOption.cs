using ChoziShopForWindows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.models
{
    public class DateRangeOption
    {
        public string? DisplayName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }   
        public GroupByScope GroupByScope { get; set; }
    }
}
