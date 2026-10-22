using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Cache
{
    public static class RedisConfig
    {
        private const string RedisHost = "redis-15349.c328.europe-west3-1.gce.redns.redis-cloud.com"; // e.g., redis-12345.c12345.us-east-1-1.ec2.cloud.redislabs.com
        private const int RedisPort = 15349; // Replace with your port from Redis Cloud
        private const string RedisUsername = "TaralejUser"; // Your custom username (default is often "default")
        private const string RedisPassword = "Prudnatguz123!"; // Replace with your password

        public static string GetConnectionString()
        {
            try
            {
                // Get connection details from environment variables if available
                var host = RedisHost;
                var port =  RedisPort.ToString();
                var username = RedisUsername;
                var password = RedisPassword;

                // Build the connection string with proper escaping and username
                var connectionString = $"{host}:{port},user={username},password={password},ssl=true,abortConnect=false";
                //CloudCodeLogger.Log($"Redis connection configured for host: {host}");
                return connectionString;
            }
            catch (Exception ex)
            {
                //CloudCodeLogger.LogError($"Error configuring Redis connection: {ex.Message}");
                throw;
            }
        }
    }
}

