using System.ComponentModel.DataAnnotations;

namespace Battleship.Features.Battleship.Models
{
    public class CreateShipRequest
    {
        [Range(1, int.MaxValue)]
        public int Size { get; set; }
    }
}