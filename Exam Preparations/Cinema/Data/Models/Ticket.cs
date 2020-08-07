using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Data.Models
{
    public class Ticket
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public decimal Price { get; set; }

        [ForeignKey(nameof(Customer))]
        [Required]
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(Projection))]
        [Required]
        public int ProjectionId { get; set; }

        public virtual Projection Projection { get; set; }
    }
}
