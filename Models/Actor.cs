
using System.ComponentModel.DataAnnotations;

namespace Fall2024_Assignment3_eschwalbach.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Gender { get; set; } = default!;

        [Required]
        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        [Url]
        public string Imdb { get; set; } = default!;

        [Required]
        [Url]
        public string Photo { get; set; } = default!;

        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
