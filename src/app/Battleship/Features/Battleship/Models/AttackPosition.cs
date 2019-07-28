namespace Battleship.Features.Battleship.Models
{
    public class AttackPosition
    {
        public int RowPosition { get; set; }
        public int ColumnPosition { get; set; }
        public AttackStatus Status { get; set; }
    }
}