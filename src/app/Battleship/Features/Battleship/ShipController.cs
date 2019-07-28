using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Features.Battleship
{
    [ApiController]
    public class ShipController : ControllerBase
    {
        private readonly IShipService _shipService;
        private readonly IBoardService _boardService;

        public ShipController(IShipService shipService, IBoardService boardService)
        {
            _shipService = shipService;
            _boardService = boardService;
        }

        [HttpPost("ships")]
        [ProducesResponseType(typeof(Ship), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateShip(CreateShipRequest createShipRequest)
        {
            var ship = await _shipService.CreateShip(createShipRequest);
            var shipUri = new Uri($"/ships/{ship.Id}", UriKind.Relative);
            return Created(shipUri, ship);
        }

        [HttpPost("boards/{boardId}/ships/{shipId}/place")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceShip(Guid boardId, Guid shipId, PlaceShipRequest placeShipRequest)
        {
            var boardTask = _boardService.GetBoard(boardId);
            var shipTask = _shipService.GetShip(shipId);
            var shipsPlacedTask = _shipService.GetShipsPlaced(boardId);

            await Task.WhenAll(boardTask, shipTask, shipsPlacedTask);

            var board = await boardTask;
            var ship = await shipTask;
            var shipsPlaced = await shipsPlacedTask;

            if(board == null || ship == null)
            {
                return NotFound();
            }

            if(shipsPlaced.Any(x => x.Id == shipId))
            {
                return BadRequest("Ship cannot be placed as requested ship already placed");
            }

            var isShipToBePlacedWithInBoundaries = IsShipToBePlacedWithInBoundaries(board, ship, placeShipRequest);
            if(isShipToBePlacedWithInBoundaries == false)
            {
                return BadRequest("Ship cannot be placed as positions should not cross boundaries");
            }

            var shipPlacedPositions = shipsPlaced.SelectMany(x => x.Positions).ToList();
            var haveShipsAlreadyOccupiedThePosition = HaveShipsAlreadyOccupiedThePosition(ship, shipPlacedPositions, placeShipRequest);
            if(haveShipsAlreadyOccupiedThePosition)
            {
                return BadRequest("Ship cannot be placed as the positions are occupied with ships");
            }

            await _shipService.PlaceShip(boardId, ship, placeShipRequest);

            return NoContent();
        }

        private static bool IsShipToBePlacedWithInBoundaries(Board board, Ship ship, PlaceShipRequest placeShipRequest)
        {
            switch(placeShipRequest.PositionStyle)
            {
                case PositionStyle.Horizontal:
                    if(placeShipRequest.RowStartPosition <= board.RowSize
                       && board.ColumnSize >= (placeShipRequest.ColumnStartPosition + ship.Size - 1))
                    {
                        return true;
                    }
                    break;
                case PositionStyle.Vertical:
                    if(placeShipRequest.ColumnStartPosition <= board.ColumnSize
                       && board.RowSize >= (placeShipRequest.RowStartPosition + ship.Size - 1))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static bool HaveShipsAlreadyOccupiedThePosition(Ship ship, List<ShipPosition> shipPlacedPositions, PlaceShipRequest placeShipRequest)
        {
            if(shipPlacedPositions == null || shipPlacedPositions.Any() == false)
            {
                return false;
            }

            switch(placeShipRequest.PositionStyle)
            {
                case PositionStyle.Horizontal:
                    foreach(var shipPlacedPosition in shipPlacedPositions)
                    {
                        if(shipPlacedPosition.RowPosition == placeShipRequest.RowStartPosition
                        && shipPlacedPosition.ColumnPosition >= placeShipRequest.ColumnStartPosition
                        && shipPlacedPosition.ColumnPosition <= (placeShipRequest.ColumnStartPosition + ship.Size))
                        {
                            return true;
                        }
                    }
                    break;
                case PositionStyle.Vertical:
                    foreach(var shipPlacedPosition in shipPlacedPositions)
                    {
                        if(shipPlacedPosition.ColumnPosition == placeShipRequest.ColumnStartPosition
                        && shipPlacedPosition.RowPosition >= placeShipRequest.RowStartPosition
                        && shipPlacedPosition.RowPosition <= (placeShipRequest.RowStartPosition + ship.Size))
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
    }
}