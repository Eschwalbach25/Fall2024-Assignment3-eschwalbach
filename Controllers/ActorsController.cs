using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_eschwalbach.Data;
using Fall2024_Assignment3_eschwalbach.Models;
using Fall2024_Assignment3_eschwalbach.Models.ViewModels;
using Fall2024_Assignment3_eschwalbach.Services;
using System.Diagnostics;

namespace Fall2024_Assignment3_eschwalbach.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.OpenAI _aiService;

        public ActorsController(ApplicationDbContext context, Services.OpenAI aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = actor.MovieActors?.Select(ma => ma.Movie!).ToList() ?? new List<Movie>(),
                Tweets = new List<(string Tweet, double Sentiment)>(),
                OverallSentiment = 0
            };

            try
            {
                // Generate tweets and calculate sentiment in parallel to improve performance
                var tweetsTask = _aiService.GenerateActorTweetsAsync(actor.Name, actor.Gender, actor.Age);
                viewModel.Tweets = await tweetsTask;

                // Calculate the average sentiment if tweets are available
                viewModel.OverallSentiment = viewModel.Tweets.Any()
                    ? viewModel.Tweets.Average(t => t.Sentiment)
                    : 0;
            }
            catch (Services.OpenAI.AIServiceException ex)
            {
                // Log the error for debugging purposes
                Debug.WriteLine($"Error generating tweets for actor {actor.Name}: {ex.Message}");

                // Provide a default tweet message
                viewModel.Tweets = new List<(string Tweet, double Sentiment)>
                {
                    ("Oops! Something went wrong... Tweets are temporarily unavailable.", 0)
                };
            }

            return View(viewModel);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,Imdb,Photo")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,Imdb,Photo")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Utility method to check if an actor exists
        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
    }
}
