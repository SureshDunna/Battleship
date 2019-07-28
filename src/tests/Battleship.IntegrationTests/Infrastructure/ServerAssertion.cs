using System.Collections.Generic;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Battleship.Infrastructure.Database;

namespace Battleship.IntegrationTests.Infrastructure
{
    public class ServerAssertion
    {
        private readonly IDatabase _database;

        public ServerAssertion(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Board>> GetBoards() => await _database.QueryList<Board>("SELECT * FROM Board");

        public async Task<IEnumerable<Ship>> GetShips() => await _database.QueryList<Ship>("SELECT * FROM Ship");

        public async Task<IEnumerable<ShipPosition>> GetShipPositions() => await _database.QueryList<ShipPosition>("SELECT * FROM ShipPosition");
    }
}