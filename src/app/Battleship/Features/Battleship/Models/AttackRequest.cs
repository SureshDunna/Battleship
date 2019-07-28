using System.ComponentModel.DataAnnotations;

namespace Battleship.Features.Battleship.Models
{
    public class AttackRequest
    {
        [Range(1, int.MaxValue)]
        public int RowPosition { get; set; }
        [Range(1, int.MaxValue)]
        public int ColumnPosition { get; set; }
    }
}