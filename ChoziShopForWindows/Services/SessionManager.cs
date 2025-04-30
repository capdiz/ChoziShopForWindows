using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ChoziShopForWindows.Services
{
    public class SessionManager
    {
        private readonly DispatcherTimer _timer;
        private readonly DateTimeOffset _sessionExpiresAt;

        public event Action? SessionExpired;
        public SessionManager(string sessionExpiresAt)
        {
            var eatParser = 
                DateTime.ParseExact(sessionExpiresAt, "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            // Parse expiration time to EAT
            var eastAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(eatParser, eastAfricaTimeZone);
            _sessionExpiresAt = new DateTimeOffset(expiration, eastAfricaTimeZone.GetUtcOffset(expiration));
          

            // Set up the timer
            _timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        }

        public void StartSessionMonitoring() => _timer.Start();
        public void StopSessionMonitoring() => _timer.Stop();

        private void Timer_Tick(object sender, EventArgs e)
        {
           Debug.WriteLine("Session expires at "+_sessionExpiresAt.ToString());
            var currentTime = GetCurrentEastAfricanTime();
            var remaining = _sessionExpiresAt.Subtract(currentTime);
            Debug.WriteLine("Session is expring in: "+remaining.ToString());

            if (remaining <= TimeSpan.Zero)
            {
                StopSessionMonitoring();
                SessionExpired?.Invoke();
            }
        }

        private static DateTimeOffset GetCurrentEastAfricanTime()
        {
            var eastAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, eastAfricaTimeZone);
        }
    }
}
