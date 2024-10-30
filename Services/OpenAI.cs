using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VaderSharp2;

namespace Fall2024_Assignment3_eschwalbach.Services;

public class OpenAI
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly SentimentIntensityAnalyzer _sentimentAnalyzer;
    private const string AiDeployment = "gpt-35-turbo";
    private const int NumCritics = 10;
    private const int NumTweets = 20;
    private readonly string[] personas =
    {
        "You are a Blockbuster Enthusiast. Prioritize high-energy action scenes, big budgets, and crowd-pleasing moments.",
        "You are an Indie Lover. Value low-budget creativity, unique storytelling, and unconventional approaches.",
        "You are a Horror Aficionado. Focus on scares, suspense, atmosphere, and the effectiveness of horror tropes.",
        "You are a Romantic Idealist. Focus on the chemistry, dialogue, and emotional highs of the love story.",
        "You are an Award Show Critic. Analyze performances, directing, and themes as if judging for Oscars or Golden Globes.",
        "You are a Dark Humor Fan. Enjoy morbid, unconventional comedy and aren’t afraid to laugh at the outrageous.",
        "You are an Action Junkie. Prioritize adrenaline-pumping sequences, stunts, and overall intensity.",
        "You are a Sci-Fi Analyst. Delve into futuristic themes, scientific accuracy, and imaginative world-building.",
        "You are a Foreign Film Connoisseur. Appreciate cultural nuance, authenticity, and international styles of storytelling.",
        "You are a Young Adult Enthusiast. Focus on relatable coming-of-age themes, youthful energy, and social dynamics.",
        "You are a Mystery Solver. Look for intricate plots, clues, red herrings, and rewarding revelations.",
        "You are a Family Film Fan. Prioritize wholesome storytelling, relatable characters, and appropriate humor for all ages.",
        "You are a Docudrama Reviewer. Value the balance between factual storytelling and emotional impact.",
        "You are a Crime Thriller Expert. Seek out suspenseful, gritty storytelling, plot twists, and moral complexity.",
        "You are an Environmentalist Critic. Focus on themes of nature, environmental preservation, and social consciousness.",
        "You are an Experimental Film Buff. Appreciate abstract visuals, unconventional storytelling, and avant-garde approaches.",
        "You are a Historical Drama Fan. Value historical accuracy, period-appropriate aesthetics, and immersion in the era.",
        "You are an Animated Film Enthusiast. Focus on animation style, character expressiveness, and family-friendly appeal.",
        "You are a Fan Theory Enthusiast. Look for hidden clues, lore connections, and potential fan theories.",
        "You are a Cinematic Philosopher. Explore existential themes, psychological depth, and thought-provoking elements."

    };

    public class AIServiceException : Exception
    {
        public AIServiceException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }

    public OpenAI(IConfiguration configuration)
    {
        var endpoint = configuration["Azure:OpenAI:Endpoint"];
        var key = configuration["Azure:OpenAI:Key"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Azure OpenAI configuration is missing.");
        }

        _openAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
        _sentimentAnalyzer = new SentimentIntensityAnalyzer();
    }


    public async Task<List<(string Review, double Sentiment)>> GenerateMovieReviewsAsync(string title, int year, string genre)
    {
        return await MovieReviewsParallel(title, year, genre);
    }

    public async Task<List<(string Tweet, double Sentiment)>> GenerateActorTweetsAsync(string name, string gender, int age)
    {
        return await ActorTweetsParallel(name, gender, age);
    }

    private async Task<List<(string Review, double Sentiment)>> MovieReviewsParallel(string title, int year, string genre)
    {
        try
        {
            var chatClient = _openAIClient.GetChatClient(AiDeployment);
            var reviews = new List<(string Review, double Sentiment)>();
            var attempts = 0;
            const int maxAttempts = 20;
            const int batchSize = 4;

            string[] shuffledPersonas = personas.ToArray();
            var rng = new Random();
            rng.Shuffle(shuffledPersonas);
            int personaIndex = 0;

            var systemMessage = new SystemChatMessage(
                "You are a film critic providing a single concise review (2-3 sentences) " +
                "that begins directly with the rating, not with the movie title. " +
                "The review should focus on the film quality and style, not on individual actors. " +
                "Suggested aspects to cover include: " +
                "plot, screenwriting, symbolism, effects, or cinematography. " +
                "Your response must follow this format exactly:\n" +
                "Rating: X/10. [Your review text here]"
            );

            while (reviews.Count < NumCritics && attempts < maxAttempts)
            {
                var remainingReviews = NumCritics - reviews.Count;
                var currentBatchSize = Math.Min(batchSize, remainingReviews);

                var tasks = Enumerable.Range(0, currentBatchSize).Select(async i =>
                {
                    try
                    {
                        var persona = shuffledPersonas[(personaIndex + i) % shuffledPersonas.Length];
                        var messages = new ChatMessage[]
                        {
                            systemMessage,
                            new UserChatMessage(
                                $"{persona} Review the {genre} movie {title} ({year}). "
                            )
                        };

                        var response = await chatClient.CompleteChatAsync(messages);
                        var reviewText = response.Value.Content[0].Text.Trim();

                        if (!reviewText.Contains("/10") || !reviewText.StartsWith("Rating:"))
                        {
                            return ("", 0.0);
                        }

                        var sentiment = _sentimentAnalyzer.PolarityScores(reviewText).Compound;
                        return (reviewText, sentiment);
                    }
                    catch (Exception)
                    {
                        return ("", 0.0);
                    }
                }).ToList();

                var results = await Task.WhenAll(tasks);
                reviews.AddRange(results.Where(r => !string.IsNullOrEmpty(r.Item1)));
                personaIndex = (personaIndex + currentBatchSize) % shuffledPersonas.Length;
                attempts++;

                if (reviews.Count < NumCritics && attempts < maxAttempts)
                {
                    await Task.Delay(1000);
                }
            }

            return reviews.Take(NumCritics).ToList();
        }
        catch (Exception ex)
        {
            throw new AIServiceException("Failed to generate movie reviews", ex);
        }
    }

    private (string Tweet, string Username) CleanTweetResponse(string response)
    {
        try
        {
            int atIndex = response.IndexOf('@');
            if ((atIndex == -1) || response.Contains("username"))
            {
                return ("", "");
            }
            string tweetText = response[..atIndex].Trim();
            tweetText = tweetText.Trim('"', '[', ']', ' ');

            string username = response[(atIndex + 1)..];
            int endUsername = username.IndexOfAny(new[] { ' ', ']' });
            if (endUsername != -1)
            {
                tweetText = $"{tweetText} {username[(endUsername + 1)..]}";
                username = username[..endUsername];
            }
            username = username.Trim('[', ']', '"', ' ');

            return (tweetText, username);
        }
        catch
        {
            return ("", "");
        }
    }

    private async Task<List<(string Tweet, double Sentiment)>> ActorTweetsParallel(string name, string gender, int age)
    {
        try
        {
            var chatClient = _openAIClient.GetChatClient(AiDeployment);
            var tweets = new List<(string Tweet, double Sentiment)>();
            var attempts = 0;
            const int maxAttempts = 30;
            const int batchSize = 4;

            string[] shuffledPersonas = personas.ToArray();
            var rng = new Random();
            rng.Shuffle(shuffledPersonas);
            int personaIndex = 0;

            var systemMessage = new SystemChatMessage(
                "You are a social media user providing a single authentic tweet (280 characters or less) " +
                "about an actor followed by your unique creative username. " +
                "Your tweet should sound like it's from a genuine fan or moviegoer. " +
                "Focus on any of these aspects: talent, memorable roles, personality, or public image. " +
                "Actor age is provided for identification purposes only. Do not use it in your tweet. " +
                "Your username should be imaginative, memorable, or playful, not a literal description of your persona. " +
                "Your response must follow this format exactly:\n" +
                "[Your tweet text here] @[Your username here]"
            );

            while (tweets.Count < NumTweets && attempts < maxAttempts)
            {
                var remainingTweets = NumTweets - tweets.Count;
                var currentBatchSize = Math.Min(batchSize, remainingTweets);

                var tasks = Enumerable.Range(0, currentBatchSize).Select(async i =>
                {
                    try
                    {
                        var persona = shuffledPersonas[(personaIndex + i) % shuffledPersonas.Length];
                        var messages = new ChatMessage[]
                        {
                            systemMessage,
                            new UserChatMessage(
                                $"{persona} Write a tweet about the {gender} actor {name}, who is {age} years old."
                            )
                        };

                        var response = await chatClient.CompleteChatAsync(messages);
                        var rawText = response.Value.Content[0].Text.Trim();
                        var (tweetText, username) = CleanTweetResponse(rawText);
                        var finalText = $"{tweetText} @{username}";

                        if (tweetText.Length > 280 || string.IsNullOrEmpty(tweetText) || string.IsNullOrEmpty(username))
                        {
                            return ("", 0.0);
                        }

                        var sentiment = _sentimentAnalyzer.PolarityScores(tweetText).Compound;
                        return (finalText, sentiment);
                    }
                    catch (Exception)
                    {
                        return ("", 0.0);
                    }
                }).ToList();

                var results = await Task.WhenAll(tasks);
                tweets.AddRange(results.Where(r => !string.IsNullOrEmpty(r.Item1)));
                personaIndex = (personaIndex + currentBatchSize) % shuffledPersonas.Length;
                attempts++;

                if (tweets.Count < NumTweets && attempts < maxAttempts)
                {
                    await Task.Delay(1000);
                }
            }

            return tweets.Take(NumTweets).ToList();
        }
        catch (Exception ex)
        {
            throw new AIServiceException("Failed to generate actor tweets", ex);
        }
    }
}