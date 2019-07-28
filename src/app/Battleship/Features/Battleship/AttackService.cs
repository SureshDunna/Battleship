using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Battleship.Infrastructure.Database;

namespace Battleship.Features.Battleship
{
    public interface IAttackService
    {
        Task UpdateAttackStatus(Guid boardId, Guid? shipId, AttackRequest attackRequest, AttackStatus attackStatus);
        Task<List<AttackPosition>> GetAttackPositionsForShip(Guid boardId, Guid shipId);
        Task<List<AttackPosition>> GetAttackPositions(Guid boardId);
    }
    public class AttackService : IAttackService
    {
        private readonly IDatabase _database;

        public AttackService(IDatabase database)
        {
            _database = database;
        }

        public async Task UpdateAttackStatus(Guid boardId, Guid? shipId, AttackRequest attackRequest, AttackStatus attackStatus)
        {
            const string sql = @"INSERT INTO AttackPosition(Id, BoardId, RowPosition, ColumnPosition, AttackStatus, ShipId) VALUES(@id, @boardId, @rowPosition, @columnPosition, @attackStatus, @shipId)";

            await _database.ExecuteAsync(sql, new { Id = Guid.NewGuid(), BoardId = boardId, attackRequest.RowPosition, attackRequest.ColumnPosition,  AttackStatus = attackStatus.ToString(), ShipId = shipId });
        }

        public async Task<List<AttackPosition>> GetAttackPositionsForShip(Guid boardId, Guid shipId)
        {
            const string sql = @"SELECT RowPosition, ColumnPosition, AttackStatus FROM AttackPosition WHERE BoardId = @boardId AND ShipId = @shipId";

            var attackPositions = await _database.QueryList<AttackPosition>(sql, new { boardId, shipId } );

            return attackPositions.ToList();
        }

        public async Task<List<AttackPosition>> GetAttackPositions(Guid boardId)
        {
            const string sql = @"SELECT RowPosition, ColumnPosition, AttackStatus AS Status FROM AttackPosition WHERE BoardId = @boardId";

            var attackPositions = await _database.QueryList<AttackPosition>(sql, new { boardId } );

            return attackPositions.ToList();
        }
    }
}