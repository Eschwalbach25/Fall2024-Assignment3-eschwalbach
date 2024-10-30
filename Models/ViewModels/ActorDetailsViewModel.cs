using Fall2024_Assignment3_eschwalbach.Models;

namespace Fall2024_Assignment3_eschwalbach.Models.ViewModels;

public class ActorDetailsViewModel
{
    public Actor Actor { get; set; } = default!;
    public List<Movie> Movies { get; set; } = new();

    public List<(string Tweet, double Sentiment)> Tweets { get; set; } = new();
    public double OverallSentiment { get; set; }
}