using System;
using System.Collections.Generic;

namespace Battleship.Features.Battleship.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public int RowSize { get; set; }
        public int ColumnSize { get; set; }
        public IList<Ship> Ships { get; set; }
        public IList<AttackPosition> Attacks { get; set; }
    }
}