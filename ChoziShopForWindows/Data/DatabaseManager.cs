using ChoziShop.Data;
using ChoziShop.Data.Models;
using ChoziShopForWindows.models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Data
{
    public class DatabaseManager
    {
        private string databasePath;
        private string tableName;
        private long sessionId;
        private string sessionAuthToken;

        public DatabaseManager(string databasePath, string tableName)
        {
            this.databasePath = databasePath;
            this.tableName = tableName;
        }

        public bool CheckDatabaseExists()
        {
            if (!File.Exists(databasePath))
            {
                return false;
            }
            return true;
        }

        public bool CheckTableExists()
        {
            Debug.WriteLine("Checking if table exists: " + DbFileConfig.ConnectionString);
            using (var conn = new SqliteConnection(DbFileConfig.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public MerchantAccount GetMerchant()
        {
            using (var conn = new SqliteConnection(DbFileConfig.ConnectionString))
            {
                conn.Open();
                string query = $"SELECT online_merchant_id, full_name, email, " +
                    $"phone_number, auth_token, bare_jid, full_jid FROM merchants LIMIT 1";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var merchant = new MerchantAccount
                            {
                                OnlineMerchantId = reader.GetInt64(0),
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                PhoneNumber = reader.GetString(3),
                                AuthToken = reader.GetString(4),
                                BareJid = reader.GetString(5),
                                FullJid = reader.GetString(6)
                            };
                            return merchant;
                        }
                    }
                }
                return null;
            }
        }

        public bool MerchantAccountExists()
        {

            using (var conn = new SqliteConnection(DbFileConfig.ConnectionString))
            {
                conn.Open();
                var countCommand = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", conn);
                var count = Convert.ToInt32(countCommand.ExecuteScalar());
                if (count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool StoreHasKeeperAccounts()
        {
            using (var conn = new SqliteConnection(DbFileConfig.ConnectionString))
            {
                conn.Open();
                var countCommand = new SqliteCommand($"SELECT COUNT(*) FROM keepers", conn);
                var count = Convert.ToInt32(countCommand.ExecuteScalar());
                if (count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMerchantSessionActive()
        {
            using (var conn = new SqliteConnection(DbFileConfig.ConnectionString))
            {
                conn.Open();
                const string query = @"SELECT session_id, auth_token, expires_at FROM merchantsessions ORDER BY id DESC LIMIT 1";
                using (var command = new SqliteCommand(query, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var sessionId = reader.GetString(0);
                            var authToken = reader.GetString(1);
                            var expiresAt = reader.GetDateTime(2);
                            this.sessionId = long.Parse(sessionId);
                            sessionAuthToken = authToken;
                            Debug.WriteLine("Merchant session exoires at -> " + expiresAt.ToString());
                            if (expiresAt > DateTime.UtcNow)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;

        }

        public long GetSessionId()
        {
            return sessionId;
        }

        public string GetSessionAuthToken()
        {
            return sessionAuthToken;
        }



    }
}
