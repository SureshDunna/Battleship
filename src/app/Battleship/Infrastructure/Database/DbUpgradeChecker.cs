using System;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace Battleship.Infrastructure.Database
{
    public interface IDbUpgradeChecker
    {
        void EnsureDatabaseUpToDate();
    }

    public class DbUpgradeChecker : IDbUpgradeChecker
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDatabaseConfiguration _databaseConfiguration;

        public DbUpgradeChecker(ILoggerFactory loggerFactory, IDatabaseConfiguration databaseConfiguration)
        {
            _loggerFactory = loggerFactory;
            _databaseConfiguration = databaseConfiguration;
        }

        public void EnsureDatabaseUpToDate()
        {
            var dbUpgradeLogger = _loggerFactory.CreateLogger<DbUpgradeChecker>();

            try
            {
                var logger = new DbUpgradeLog(dbUpgradeLogger);
                EnsureDatabase.For.SqlDatabase(_databaseConfiguration.BattleshipConnectionString, logger);

                var upgradeEngine = DeployChanges.To.SqlDatabase(_databaseConfiguration.BattleshipConnectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithTransactionPerScript()
                    .LogScriptOutput()
                    .LogTo(logger)
                    .LogToConsole()
                    .Build();

                if (!upgradeEngine.IsUpgradeRequired())
                {
                    dbUpgradeLogger.LogInformation("Database upgrade is not required.");
                    return;
                }

                if (!upgradeEngine.PerformUpgrade().Successful)
                {
                    throw new Exception("DbUp upgrade engine failed to perform upgrade.");
                }
            }
            catch (Exception ex)
            {
                dbUpgradeLogger.LogCritical(ex, "Failed to create or upgrade database.");
                throw;
            }
        }
    }
}
