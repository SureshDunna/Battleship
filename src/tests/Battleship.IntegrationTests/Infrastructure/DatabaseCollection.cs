using Xunit;

namespace Battleship.IntegrationTests.Infrastructure
{
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Database collection";
    }
}