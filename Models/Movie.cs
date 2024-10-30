using System.ComponentModel.DataAnnotations;

namespace Fall2024_Assignment3_eschwalbach.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        [Required]
        [Url]
        public string Imdb { get; set; } = default!;

        [Required]
        public string Genre { get; set; } = default!;

        [Required]
        [Range(1888, 2024)]
        public int Year { get; set; }

        [Required]
        [Url]
        public string Poster { get; set; } = default!;

        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}