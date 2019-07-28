using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Battleship.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Battleship.IntegrationTests.Features.Battleship
{
    [Collection(DatabaseCollection.Name)]
    public class BattleshipTests
    {
        private readonly DatabaseFixture _databaseFixture;

        public BattleshipTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        [Fact]
        public async Task GivenSize_To_Create_Board_Must_Create_New_Board()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var createBoardRequest = new CreateBoardRequest
                {
                    RowSize = 10,
                    ColumnSize = 10
                };
                var response = await server.Client.PostAsJsonAsync("boards", createBoardRequest);

                response.StatusCode.Should().Be(HttpStatusCode.Created);

                var boardTask = response.Content.ReadAsAsync<Board>();
                var currentBoardsTask = server.Assert().GetBoards();

                await Task.WhenAll(boardTask, currentBoardsTask);

                var board = await boardTask;
                var currentBoards = await currentBoardsTask;
                var currentBoard = currentBoards.First();

                board.Should().BeEquivalentTo(currentBoard);
            }
        }

        [Fact]
        public async Task GivenSize_To_Create_Ship_Must_Create_New_Ship()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var createShipRequest = new CreateShipRequest
                {
                    Size = 3
                };
                var response = await server.Client.PostAsJsonAsync("ships", createShipRequest);

                response.StatusCode.Should().Be(HttpStatusCode.Created);

                var shipTask = response.Content.ReadAsAsync<Ship>();
                var currentShipsTask = server.Assert().GetShips();

                await Task.WhenAll(shipTask, currentShipsTask);

                var ship = await shipTask;
                var currentShips = await currentShipsTask;
                var currentShip = currentShips.First();

                ship.Should().BeEquivalentTo(currentShip);
            }
        }

        [Fact]
        public async Task GivenShip_Must_Be_Able_To_Place_On_Board_Horizontally()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var shipId = Guid.NewGuid();

                server
                .Arrange()
                .AddBoard(new Board { Id = boardId, RowSize = 10, ColumnSize = 10 })
                .AddShip(new Ship { Id = shipId, Size = 3 });

                var placeShipRequest = new PlaceShipRequest
                {
                    RowStartPosition = 1,
                    ColumnStartPosition = 1,
                    PositionStyle = PositionStyle.Horizontal
                };
                var response = await server.Client.PostAsJsonAsync($"boards/{boardId}/ships/{shipId}/place", placeShipRequest);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var shipPositions = await server.Assert().GetShipPositions();
                shipPositions.Any(x => x.RowPosition == 1 && x.ColumnPosition == 1).Should().BeTrue();
                shipPositions.Any(x => x.RowPosition == 1 && x.ColumnPosition == 2).Should().BeTrue();
                shipPositions.Any(x => x.RowPosition == 1 && x.ColumnPosition == 3).Should().BeTrue();
            }
        }

        [Fact]
        public async Task GivenShip_Must_Be_Able_To_Place_On_Board_Vertically()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var shipId = Guid.NewGuid();

                server
                .Arrange()
                .AddBoard(new Board { Id = boardId, RowSize = 10, ColumnSize = 10 })
                .AddShip(new Ship { Id = shipId, Size = 3 });

                var placeShipRequest = new PlaceShipRequest
                {
                    RowStartPosition = 1,
                    ColumnStartPosition = 1,
                    PositionStyle = PositionStyle.Vertical
                };
                var response = await server.Client.PostAsJsonAsync($"boards/{boardId}/ships/{shipId}/place", placeShipRequest);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var shipPositions = await server.Assert().GetShipPositions();
                shipPositions.Any(x => x.RowPosition == 1 && x.ColumnPosition == 1).Should().BeTrue();
                shipPositions.Any(x => x.RowPosition == 2 && x.ColumnPosition == 1).Should().BeTrue();
                shipPositions.Any(x => x.RowPosition == 3 && x.ColumnPosition == 1).Should().BeTrue();
            }
        }

        [Fact]
        public async Task GivenPlayer_Must_Be_Able_To_Attack_With_Position_And_Returns_Miss()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var ship1Id = Guid.NewGuid();
                var ship2Id = Guid.NewGuid();

                SetupAttackTestData(server, boardId, ship1Id, ship2Id);

                var attackRequest = new AttackRequest { RowPosition = 2, ColumnPosition = 4 };
                var response = await server.Client.PostAsJsonAsync($"boards/{boardId}/attack", attackRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var attackResponse = await response.Content.ReadAsAsync<AttackResponse>();
                attackResponse.AttackStatus.Should().Be(AttackStatus.Miss);
            }
        }

        [Fact]
        public async Task GivenPlayer_Must_Be_Able_To_Attack_With_Position_And_Returns_Hit()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var ship1Id = Guid.NewGuid();
                var ship2Id = Guid.NewGuid();

                SetupAttackTestData(server, boardId, ship1Id, ship2Id);

                var attackRequest = new AttackRequest { RowPosition = 2, ColumnPosition = 1 };
                var response = await server.Client.PostAsJsonAsync($"boards/{boardId}/attack", attackRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var attackResponse = await response.Content.ReadAsAsync<AttackResponse>();
                attackResponse.AttackStatus.Should().Be(AttackStatus.Hit);
            }
        }

        [Fact]
        public async Task GivenPlayer_Must_Be_Able_To_Attack_With_All_Ship_Positions_And_Sunk_Ship()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var ship1Id = Guid.NewGuid();
                var ship2Id = Guid.NewGuid();

                SetupAttackTestData(server, boardId, ship1Id, ship2Id);

                var attackRequest = new AttackRequest { RowPosition = 1, ColumnPosition = 2 };
                var response = await server.Client.PostAsJsonAsync($"boards/{boardId}/attack", attackRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var ships = await server.Assert().GetShips();
                var ship = ships.First();
                ship.Status.Should().Be(ShipStatus.Sunk);
            }
        }

        [Fact]
        public async Task Given_Board_Ships_And_Its_Attacking_Positions_GetBoard_Must_Returns_BoardTracker()
        {
            using (var server = new ServerFixture(databasePort: _databaseFixture.Port))
            {
                var boardId = Guid.NewGuid();
                var ship1Id = Guid.NewGuid();
                var ship2Id = Guid.NewGuid();

                SetupAttackTestData(server, boardId, ship1Id, ship2Id);

                var response = await server.Client.GetAsync($"boards/{boardId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var board = await response.Content.ReadAsAsync<Board>();
                board.Id.Should().Be(boardId);
                board.RowSize.Should().Be(10);
                board.ColumnSize.Should().Be(10);
                board.Ships.Should().HaveCount(2);
                board.Ships.First(x => x.Id == ship1Id).Positions.Should().BeEquivalentTo(new List<ShipPosition>
                {
                    new ShipPosition { RowPosition = 1, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 1, ColumnPosition = 2 },
                    new ShipPosition { RowPosition = 1, ColumnPosition = 3 }
                });
                board.Ships.First(x => x.Id == ship2Id).Positions.Should().BeEquivalentTo(new List<ShipPosition>
                {
                    new ShipPosition { RowPosition = 2, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 3, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 4, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 5, ColumnPosition = 1 },
                    new ShipPosition { RowPosition = 6, ColumnPosition = 1 }
                });
                board.Attacks.Should().BeEquivalentTo(new List<AttackPosition>
                {
                    new AttackPosition { RowPosition = 1, ColumnPosition = 1,  Status = AttackStatus.Hit },
                    new AttackPosition { RowPosition = 1, ColumnPosition = 3,  Status = AttackStatus.Hit }
                });
            }
        }

        private void SetupAttackTestData(ServerFixture server, Guid boardId, Guid ship1Id, Guid ship2Id)
        {
            server
            .Arrange()
            .AddBoard(new Board { Id = boardId, RowSize = 10, ColumnSize = 10 })
            .AddShip(new Ship { Id = ship1Id, Size = 3 })
            .AddShip(new Ship { Id = ship2Id, Size = 5 })
            .AddShipPositions(boardId, ship1Id, new List<ShipPosition>
            {
                new ShipPosition { RowPosition = 1, ColumnPosition = 1 },
                new ShipPosition { RowPosition = 1, ColumnPosition = 2 },
                new ShipPosition { RowPosition = 1, ColumnPosition = 3 }
            })
            .AddShipPositions(boardId, ship2Id, new List<ShipPosition>
            {
                new ShipPosition { RowPosition = 2, ColumnPosition = 1 },
                new ShipPosition { RowPosition = 3, ColumnPosition = 1 },
                new ShipPosition { RowPosition = 4, ColumnPosition = 1 },
                new ShipPosition { RowPosition = 5, ColumnPosition = 1 },
                new ShipPosition { RowPosition = 6, ColumnPosition = 1 }
            })
            .AddAttackPositions(boardId, ship1Id, new List<AttackPosition>
            {
                new AttackPosition { RowPosition = 1, ColumnPosition = 1,  Status = AttackStatus.Hit },
                new AttackPosition { RowPosition = 1, ColumnPosition = 3,  Status = AttackStatus.Hit }
            });
        }
    }
}