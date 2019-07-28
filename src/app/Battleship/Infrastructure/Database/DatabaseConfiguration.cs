using Microsoft.Extensions.Configuration;

namespace Battleship.Infrastructure.Database
{
    public interface IDatabaseConfiguration
    {
        string BattleshipConnectionString { get; }
    }

    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        public string BattleshipConnectionString { get; }

        public DatabaseConfiguration(IConfiguration configuration)
        {
            BattleshipConnectionString = configuration.GetConnectionString("BattleshipDbConnection");
        }
    }
}