using ChoziShop.Data;
using ChoziShop.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChoziShopForWindows.Converters
{
    public class UserIdToNameConverter : IValueConverter
    {
        private static Dictionary<long, string> _userLookupcache = new();
        private static readonly SemaphoreSlim _cacheLock = new(1, 1);
        private static bool _isInitialized;

        public enum UserAccountType
        {
            Merchant,
            Keeper            
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            long userId;
            if (value is long id)
            {
                userId = id;
            }
            else if (value is string strId && long.TryParse(strId, out long parsedId))
            {
                userId = parsedId;
            }else if(value is int userAccountId)
            {
                userId = userAccountId;
            }
            else
            {
                return string.Empty; // Invalid input
            }
            // Lazy-Load user account details from parameter value details
            if (!_isInitialized)
            {
                if(parameter is string userAccountTypeStr && Enum.TryParse(userAccountTypeStr, out UserAccountType userAccountType))
                {
                    InitializeUserAccountAsync(userAccountType).ConfigureAwait(false).GetAwaiter().GetResult();
                }                              
            }

            if (_userLookupcache.TryGetValue(userId, out string userName))
            {
                return userName;
            }
            else
            {
                return $"Unknown {userId}"; // Fallback if user not found
            }
        }

        public static async Task RefreshCacheAsyn(UserAccountType userAccountType)
        {
            await _cacheLock.WaitAsync();
            try
            {
                _isInitialized = false; // Reset initialization flag
                await InitializeUserAccountAsync(userAccountType); // Reinitialize the cache
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static async Task InitializeUserAccountAsync(UserAccountType accountType)
        {
            await _cacheLock.WaitAsync();
            try
            {
                if (_isInitialized) return;
                var dbContext = App.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
                switch (accountType)
                {
                    case UserAccountType.Keeper:
                        _userLookupcache = await dbContext.Keepers
                            .AsNoTracking().ToDictionaryAsync(k => k.OnlineKeeperId, k => k.FullName);
                        break;
                    case UserAccountType.Merchant:
                        _userLookupcache = await dbContext.Merchants
                            .AsNoTracking().ToDictionaryAsync(m => m.OnlineMerchantId, m => m.FullName);
                        break;
                }              
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                Console.WriteLine($"Error initializing user accounts: {ex.Message}");
                _userLookupcache = new Dictionary<long, string>();
                _isInitialized = true; // Ensure we don't retry on failure
            }
            finally
            {
                _cacheLock.Release();
            }
        }
    }
}
