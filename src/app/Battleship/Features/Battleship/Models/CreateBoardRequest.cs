using System.ComponentModel.DataAnnotations;

namespace Battleship.Features.Battleship.Models
{
    public class CreateBoardRequest
    {
        [Range(1, int.MaxValue)]
        public int RowSize { get; set; }
        [Range(1, int.MaxValue)]
        public int ColumnSize { get; set; }
    }
}