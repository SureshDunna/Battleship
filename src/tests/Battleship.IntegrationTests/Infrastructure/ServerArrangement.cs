using System;
using System.Collections.Generic;
using System.Linq;
using Battleship.Features.Battleship.Models;
using Battleship.Infrastructure.Database;

namespace Battleship.IntegrationTests.Infrastructure
{
    public class ServerArrangement
    {
        private readonly IDatabase _database;

        public ServerArrangement(IDatabase database)
        {
            _database = database;
        }

        public ServerArrangement AddBoard(Board board)
        {
            const string sql = @"INSERT INTO Board(Id, RowSize, ColumnSize) VALUES(@id, @rowSize, @columnSize)";
            _database.Execute(sql, board);
            return this;
        }

        public ServerArrangement AddShip(Ship ship)
        {
            const string sql = @"INSERT INTO Ship(Id, Size, Status) VALUES(@id, @size, @status)";
            _database.Execute(sql, ship);
            return this;
        }

        public ServerArrangement AddShipPositions(Guid boardId, Guid shipId, List<ShipPosition> shipPositions)
        {
            const string sql = @"INSERT INTO ShipPosition(Id, BoardId, ShipId, RowPosition, ColumnPosition) VALUES(@id, @boardId, @shipId, @rowPosition, @columnPosition)";
            _database.Execute(sql, shipPositions.Select(x => new
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                ShipId = shipId,
                RowPosition = x.RowPosition,
                ColumnPosition = x.ColumnPosition
            }));
            return this;
        }

        public ServerArrangement AddAttackPositions(Guid boardId, Guid shipId, List<AttackPosition> attackPositions)
        {
            const string sql = @"INSERT INTO AttackPosition(Id, BoardId, RowPosition, ColumnPosition, AttackStatus, ShipId) VALUES(@id, @boardId, @rowPosition, @columnPosition, @attackStatus, @shipId)";
            _database.Execute(sql, attackPositions.Select(x => new
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                ShipId = shipId,
                RowPosition = x.RowPosition,
                ColumnPosition = x.ColumnPosition,
                AttackStatus = x.Status
            }));
            return this;
        }
    }
}