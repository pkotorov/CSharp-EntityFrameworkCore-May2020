
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Data.Models
{
    public class Projection
    {
        public Projection()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Movie))]
        [Required]
        public int MovieId { get; set; }

        public virtual Movie Movie { get; set; }

        [ForeignKey(nameof(Hall))]
        [Required]
        public int HallId { get; set; }

        public virtual Hall Hall { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
