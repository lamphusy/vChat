using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VChatCore
{
    public class EnviConfig
    {
        public static string ConnectionString { get; private set; }
        public static string SecretKey { get; private set; }
        public static int ExpirationInMinutes { get; private set; }
        public static string DailyToken { get; private set; }

        public static void Config(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
            SecretKey = configuration["JwtConfig:SecretKey"];
            ExpirationInMinutes = Convert.ToInt32(configuration["JwtConfig:ExpirationInMinutes"]);
            DailyToken = configuration["DailyToken"];
        }
    }
}
