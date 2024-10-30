using Fall2024_Assignment3_eschwalbach.Models;

namespace Fall2024_Assignment3_eschwalbach.Models.ViewModels;

public class MovieDetailsViewModel
{
    public Movie Movie { get; set; } = default!;
    public List<Actor> Actors { get; set; } = new();

    // For AI-generated content
    public List<(string Review, double Sentiment)> Reviews { get; set; } = new();
    public double OverallSentiment { get; set; }
}