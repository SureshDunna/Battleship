using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Features.Battleship
{
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IShipService _shipService;
        private readonly IBoardService _boardService;
        private readonly IAttackService _attackService;

        public PlayerController(IShipService shipService, IBoardService boardService, IAttackService attackService)
        {
            _shipService = shipService;
            _boardService = boardService;
            _attackService = attackService;
        }

        [HttpPost("boards/{boardId}/attack")]
        [ProducesResponseType(typeof(AttackResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Attack(Guid boardId, AttackRequest attackRequest)
        {
            var boardTask = _boardService.GetBoard(boardId);
            var shipsTask = _shipService.GetShipsPlaced(boardId);

            await Task.WhenAll(boardTask, shipsTask);

            var board = await boardTask;
            if(board == null)
            {
                return NotFound();
            }

            var ships = await shipsTask;
            var attackStatus = AttackStatus.Miss;
            Guid? shipId = null;

            foreach(var ship in ships)
            {
                var shipPosition = ship.Positions.FirstOrDefault(x => x.RowPosition == attackRequest.RowPosition
                                                                 && x.ColumnPosition == attackRequest.ColumnPosition);

                if(shipPosition != null)
                {
                    attackStatus = AttackStatus.Hit;
                    shipId = ship.Id;

                    var currentAttackPositions = await _attackService.GetAttackPositionsForShip(boardId, ship.Id);
                    if(ship.Size == (currentAttackPositions.Count + 1))
                    {
                        await _shipService.UpdateShipStatus(ship.Id, ShipStatus.Sunk);
                    }
                    break;
                }
            }

            await _attackService.UpdateAttackStatus(boardId, shipId, attackRequest, attackStatus);

            return Ok(new AttackResponse { AttackStatus = attackStatus });
        }
    }
}