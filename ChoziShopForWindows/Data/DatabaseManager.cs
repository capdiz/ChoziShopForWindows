using ChoziShop.Data;
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
    }
}
