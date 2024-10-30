using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_eschwalbach.Data;
using Fall2024_Assignment3_eschwalbach.Models;
using Fall2024_Assignment3_eschwalbach.Models.ViewModels;
using Fall2024_Assignment3_eschwalbach.Services;

namespace Fall2024_Assignment3_eschwalbach.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.OpenAI _aiService;

        public MoviesController(ApplicationDbContext context, Services.OpenAI aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = movie.MovieActors?.Select(ma => ma.Actor!).ToList() ?? new List<Actor>(),
                Reviews = new List<(string Review, double Sentiment)>(),
                OverallSentiment = 0
            };
            try
            {
                viewModel.Reviews = await _aiService.GenerateMovieReviewsAsync(
                    movie.Title,
                    movie.Year,
                    movie.Genre
                );
                viewModel.OverallSentiment = viewModel.Reviews.Average(r => r.Sentiment);
            }
            catch (Services.OpenAI.AIServiceException)
            {
                viewModel.Reviews = new List<(string Review, double Sentiment)>
                {
                    ("Oops! Something went wrong... Reviews are temporarily unavailable.", 0)
                };
            }

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Imdb,Genre,Year,Poster")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Imdb,Genre,Year,Poster")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
