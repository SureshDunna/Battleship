using System.ComponentModel.DataAnnotations;

namespace Battleship.Features.Battleship.Models
{
    public class PlaceShipRequest
    {
        [Range(1, int.MaxValue)]
        public int RowStartPosition { get; set; }
        [Range(1, int.MaxValue)]
        public int ColumnStartPosition { get; set; }
        public PositionStyle PositionStyle { get; set; }
    }

    public enum PositionStyle
    {
        Horizontal,
        Vertical
    }
}