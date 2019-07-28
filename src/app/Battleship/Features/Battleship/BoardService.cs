using System;
using System.Threading.Tasks;
using Battleship.Features.Battleship.Models;
using Battleship.Infrastructure.Database;

namespace Battleship.Features.Battleship
{
    public interface IBoardService
    {
        Task<Board> CreateBoard(CreateBoardRequest createBoardRequest);
        Task<Board> GetBoard(Guid boardId);
    }

    public class BoardService : IBoardService
    {
        private readonly IDatabase _database;

        public BoardService(IDatabase database)
        {
            _database = database;
        }

        public async Task<Board> CreateBoard(CreateBoardRequest createBoardRequest)
        {
            var boardId = Guid.NewGuid();
            const string sql = @"INSERT INTO Board(Id, RowSize, ColumnSize) VALUES(@id, @rowSize, @columnSize)";

            await _database.ExecuteAsync(sql, new { Id = boardId, createBoardRequest.RowSize, createBoardRequest.ColumnSize });

            return new Board
            {
                Id = boardId,
                RowSize = createBoardRequest.RowSize,
                ColumnSize = createBoardRequest.ColumnSize
            };
        }

        public async Task<Board> GetBoard(Guid boardId)
        {
            const string sql = @"SELECT Id, RowSize, ColumnSize FROM Board WHERE Id = @boardId";

            var board = await _database.Query<Board>(sql, new { boardId });

            return board;
        }
    }
}