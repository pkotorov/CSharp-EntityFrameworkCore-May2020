using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Data.Models
{
    public class Seat
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [ForeignKey(nameof(Hall))]
        [Required]
        public int HallId { get; set; }

        public virtual Hall Hall { get; set; }
    }
}
