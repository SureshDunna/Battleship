using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Battleship.Infrastructure.Database;

namespace Battleship.Features.Battleship
{
    public interface IShipService
    {
        Task<Ship> CreateShip(CreateShipRequest createShipRequest);
        Task<Ship> GetShip(Guid shipId);
        Task<List<Ship>> GetShipsPlaced(Guid boardId);
        Task PlaceShip(Guid boardId, Ship ship, PlaceShipRequest placeShipRequest);
        Task UpdateShipStatus(Guid shipId, ShipStatus status);
    }

    public class ShipService : IShipService
    {
        private readonly IDatabase _database;

        public ShipService(IDatabase database)
        {
            _database = database;
        }

        public async Task<Ship> CreateShip(CreateShipRequest createShipRequest)
        {
            var shipId = Guid.NewGuid();
            const string sql = @"INSERT INTO Ship(Id, Size, Status) VALUES(@id, @size, @status)";

            await _database.ExecuteAsync(sql, new { Id = shipId, createShipRequest.Size, Status = ShipStatus.Active.ToString() });

            return new Ship
            {
                Id = shipId,
                Size = createShipRequest.Size,
                Status = ShipStatus.Active
            };
        }

        public async Task<Ship> GetShip(Guid shipId)
        {
            const string sql = @"SELECT Id, Size FROM Ship WHERE Id = @shipId";

            var ship = await _database.Query<Ship>(sql, new { shipId });

            return ship;
        }

        public async Task<List<Ship>> GetShipsPlaced(Guid boardId)
        {
            const string sql = @"SELECT
                                 s.Id,
                                 s.Size,
                                 s.Status,
                                 sp.ShipId,
                                 sp.RowPosition,
                                 sp.ColumnPosition
                                 FROM ShipPosition sp
                                 INNER JOIN Ship s
                                 ON sp.ShipId = s.Id
                                 WHERE BoardId = @boardId
                                 ORDER BY sp.ShipId, sp.RowPosition, sp.ColumnPosition";

            var lookup = new Dictionary<Guid, Ship>();
            await _database.Query<Ship, ShipPosition, Ship>(sql,
            (s, p) =>
            {
                if (lookup.TryGetValue(s.Id, out var ship) == false)
                {
                    lookup.Add(s.Id, ship = s);
                }
                if (ship.Positions == null)
                {
                    ship.Positions = new List<ShipPosition>();
                }
                ship.Positions.Add(p);
                return ship;
            }, "ShipId", new { boardId });

            return lookup.Values.ToList();
        }

        public async Task PlaceShip(Guid boardId, Ship ship, PlaceShipRequest placeShipRequest)
        {
            var shipPositions = new List<object>();
            var index = 0;

            switch(placeShipRequest.PositionStyle)
            {
                case PositionStyle.Horizontal:
                    while(index < ship.Size)
                    {
                        shipPositions.Add(new
                        {
                            Id = Guid.NewGuid(),
                            BoardId = boardId,
                            ShipId = ship.Id,
                            RowPosition = placeShipRequest.RowStartPosition,
                            ColumnPosition = placeShipRequest.ColumnStartPosition + index,
                        });

                        index++;
                    }
                    break;
                case PositionStyle.Vertical:
                    while(index < ship.Size)
                    {
                        shipPositions.Add(new
                        {
                            Id = Guid.NewGuid(),
                            BoardId = boardId,
                            ShipId = ship.Id,
                            RowPosition = placeShipRequest.RowStartPosition + index,
                            ColumnPosition = placeShipRequest.ColumnStartPosition,
                        });

                        index++;
                    }
                    break;
            }

            const string sql = @"INSERT INTO ShipPosition(Id, BoardId, ShipId, RowPosition, ColumnPosition) VALUES(@id, @boardId, @shipId, @rowPosition, @columnPosition)";

            await _database.ExecuteAsync(sql, shipPositions);
        }

        public async Task UpdateShipStatus(Guid shipId, ShipStatus status)
        {
            const string sql = @"UPDATE Ship SET Status = @status WHERE Id = @shipId";

            await _database.ExecuteAsync(sql, new { shipId, Status = status.ToString() });
        }
    }
}