using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Battleship.Features.Battleship;
using Battleship.Features.Battleship.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Battleship.UnitTests.Features.Battleship
{
    public class ShipControllerTests
    {
        private readonly IShipService _shipService;
        private readonly IBoardService _boardService;

        private readonly ShipController _shipController;

        public ShipControllerTests()
        {
            _shipService = Substitute.For<IShipService>();
            _boardService = Substitute.For<IBoardService>();

            _shipController = new ShipController(_shipService, _boardService);
        }

        [Fact]
        public async Task Given_Ship_Must_Not_Be_Placed_If_Its_Already_Placed()
        {
            var boardId = Guid.NewGuid();
            var shipId = Guid.NewGuid();

            _boardService.GetBoard(boardId).Returns(x => new Board { Id = boardId });
            _shipService.GetShip(shipId).Returns(x => new Ship { Id = shipId });
            _shipService.GetShipsPlaced(boardId).Returns(x => new List<Ship>
            {
                new Ship { Id = shipId }
            });

            var response = await _shipController.PlaceShip(boardId, shipId, null);

            response.Should().BeOfType<BadRequestObjectResult>();

            ((BadRequestObjectResult)response).Value.Should().Be("Ship cannot be placed as requested ship already placed");
        }

        [Fact]
        public async Task Given_Ship_Must_Not_Be_Placed_Vertically_If_Its_Crosses_Boundaries()
        {
            var boardId = Guid.NewGuid();
            var shipId = Guid.NewGuid();

            _boardService.GetBoard(boardId).Returns(x => new Board { Id = boardId, RowSize = 5, ColumnSize = 5 });
            _shipService.GetShip(shipId).Returns(x => new Ship { Id = shipId, Size = 3 });
            _shipService.GetShipsPlaced(boardId).Returns(x => new List<Ship>());

            var response = await _shipController.PlaceShip(boardId, shipId, new PlaceShipRequest
            {
                RowStartPosition = 4,
                ColumnStartPosition = 2,
                PositionStyle = PositionStyle.Vertical
            });

            response.Should().BeOfType<BadRequestObjectResult>();

            ((BadRequestObjectResult)response).Value.Should().Be("Ship cannot be placed as positions should not cross boundaries");
        }

        [Fact]
        public async Task Given_Ship_Must_Not_Be_Placed_Horizontally_If_Its_Crosses_Boundaries()
        {
            var boardId = Guid.NewGuid();
            var shipId = Guid.NewGuid();

            _boardService.GetBoard(boardId).Returns(x => new Board { Id = boardId, RowSize = 5, ColumnSize = 5 });
            _shipService.GetShip(shipId).Returns(x => new Ship { Id = shipId, Size = 3 });
            _shipService.GetShipsPlaced(boardId).Returns(x => new List<Ship>());

            var response = await _shipController.PlaceShip(boardId, shipId, new PlaceShipRequest
            {
                RowStartPosition = 2,
                ColumnStartPosition = 4,
                PositionStyle = PositionStyle.Horizontal
            });

            response.Should().BeOfType<BadRequestObjectResult>();

            ((BadRequestObjectResult)response).Value.Should().Be("Ship cannot be placed as positions should not cross boundaries");
        }

        [Fact]
        public async Task Given_Ship_Must_Not_Be_Placed_If_Its_Occupied_By_Another_Ship()
        {
            var boardId = Guid.NewGuid();
            var shipId = Guid.NewGuid();

            _boardService.GetBoard(boardId).Returns(x => new Board { Id = boardId, RowSize = 5, ColumnSize = 5 });
            _shipService.GetShip(shipId).Returns(x => new Ship { Id = shipId, Size = 3 });
            _shipService.GetShipsPlaced(boardId).Returns(x => new List<Ship>
            {
                new Ship { Id = Guid.NewGuid(), Positions = new List<ShipPosition>
                {
                    new ShipPosition { RowPosition = 2, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 2, ColumnPosition = 2 }
                } }
            });

            var response = await _shipController.PlaceShip(boardId, shipId, new PlaceShipRequest
            {
                RowStartPosition = 1,
                ColumnStartPosition = 2,
                PositionStyle = PositionStyle.Vertical
            });

            response.Should().BeOfType<BadRequestObjectResult>();

            ((BadRequestObjectResult)response).Value.Should().Be("Ship cannot be placed as the positions are occupied with ships");
        }
    }
}