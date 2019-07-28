using System;
using System.Net;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Features.Battleship
{
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly IShipService _shipService;
        private readonly IAttackService _attackService;

        public BoardController(IBoardService boardService, IShipService shipService, IAttackService attackService)
        {
            _boardService = boardService;
            _shipService = shipService;
            _attackService = attackService;
        }

        [HttpPost("boards")]
        [ProducesResponseType(typeof(Board), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateBoard(CreateBoardRequest createBoardRequest)
        {
            var board = await _boardService.CreateBoard(createBoardRequest);
            var boardUri = new Uri($"/boards/{board.Id}", UriKind.Relative);
            return Created(boardUri, board);
        }

        [HttpGet("boards/{boardId}")]
        [ProducesResponseType(typeof(Board), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetBoard(Guid boardId)
        {
            var boardTask = _boardService.GetBoard(boardId);
            var shipsTask = _shipService.GetShipsPlaced(boardId);
            var attackPositionsTask = _attackService.GetAttackPositions(boardId);

            await Task.WhenAll(boardTask, shipsTask, attackPositionsTask);

            var board = await boardTask;
            if(board == null)
            {
                return NotFound();
            }

            var ships = await shipsTask;
            var attackPositions = await attackPositionsTask;

            board.Ships = ships;
            board.Attacks = attackPositions;

            return Ok(board);
        }
    }
}