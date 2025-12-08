using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movies_Project.Data;
using Movies_Project.Models;

namespace Movies_Project.Controllers
{
    public class MoviesController : Controller
    {
        private readonly Movies_ProjectContext _context;

        public MoviesController(Movies_ProjectContext context)
        {
            _context = context;
        }

        // GET: Movies
        

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            // 1. Configurare parametri de sortare
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["BudgetSortParm"] = sortOrder == "Budget" ? "budget_desc" : "Budget";

            // NOU: Parametrul de sortare pentru Director
            ViewData["DirectorSortParm"] = sortOrder == "Director" ? "director_desc" : "Director";

            ViewData["CurrentFilter"] = searchString;

            // 2. Construirea interogării de bază
            var movies = _context.Movie
                .Include(m => m.Genre)
                .Include(m => m.Director)
                .Include(m => m.Actors!)
                .ThenInclude(a => a.Manager)
                .AsNoTracking(); // Recomandat

            // 3. Aplicarea filtrării (logica existentă)
            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(m => m.Title.Contains(searchString));
            }

            // 4. Aplicarea sortării
            switch (sortOrder)
            {
                case "title_desc":
                    movies = movies.OrderByDescending(m => m.Title);
                    break;

                case "Budget":
                    movies = movies.OrderBy(m => m.Budget);
                    break;

                case "budget_desc":
                    movies = movies.OrderByDescending(m => m.Budget);
                    break;

                case "Director": // NOU: Sortează crescător după LastName, apoi FirstName
                    movies = movies.OrderBy(m => m.Director.FirstName).ThenBy(m => m.Director.LastName);
                    break;

                case "director_desc": // NOU: Sortează descrescător după LastName, apoi FirstName
                    movies = movies.OrderByDescending(m => m.Director.FirstName).ThenByDescending(m => m.Director.LastName);
                    break;

                default: // Sortare implicită (Titlu crescător)
                    movies = movies.OrderBy(m => m.Title);
                    break;
            }

            return View(await movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Genre)
                .Include(m => m.Director)
                .Include(m => m.Actors!)
                .ThenInclude(a => a.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            PopulateDropDowns(null); // Încarcă liste pentru Director, Gen și Actori
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Corecție: Am eliminat "ID" din Bind și am adăugat `selectedActors` ca parametru
        public async Task<IActionResult> Create([Bind("Title,DirectorID,Budget,GenreID")] Movie movie, int[] selectedActors)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // 1. Adaugă filmul
                    _context.Add(movie);
                    await _context.SaveChangesAsync();

                    // 2. Actualizează actorii asociați
                    if (selectedActors != null)
                    {
                        var actorsToUpdate = await _context.Actor
                            .Where(a => selectedActors.Contains(a.ActorID))
                            .ToListAsync();

                        foreach (var actor in actorsToUpdate)
                        {
                            actor.MovieID = movie.ID; // Asociază actorul la filmul curent
                        }
                        await _context.SaveChangesAsync(); // Salvează schimbările la actori
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                // Corecție: Adăugarea blocului try-catch pentru a gestiona erorile bazei de date
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists.");
            }

            // Reîncarcă datele pentru View pe eroare
            PopulateDropDowns(movie);
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Actors) // Include actorii asociați pentru a pre-selecta checkbox-urile
                .AsNoTracking() // Recomandat pentru metodele GET
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            PopulateDropDowns(movie); // Încarcă liste (inclusiv lista de actori și cei selectați)
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        // Corecție: Metodă pentru prevenirea overposting-ului (TryUpdateModelAsync)
        public async Task<IActionResult> EditPost(int? id, int[] selectedActors)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Citește entitatea existentă (Movie) din baza de date
            var movieToUpdate = await _context.Movie
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movieToUpdate == null)
            {
                return NotFound();
            }

            // 2. Aplică actualizările din formular doar pe câmpurile specificate
            if (await TryUpdateModelAsync<Movie>(
                movieToUpdate,
                "", // Prefix
                m => m.Title, m => m.DirectorID, m => m.Budget, m => m.GenreID))
            {
                try
                {
                    // 3. Logica de actualizare a asocierilor cu Actorii

                    // a) Des-asociază toți actorii care erau anterior asociați acestui film
                    var oldActors = await _context.Actor.Where(a => a.MovieID == id).ToListAsync();
                    foreach (var actor in oldActors)
                    {
                        actor.MovieID = null;
                    }

                    // b) Asociază actorii nou selectați
                    if (selectedActors != null)
                    {
                        var actorsToUpdate = await _context.Actor
                            .Where(a => selectedActors.Contains(a.ActorID))
                            .ToListAsync();

                        foreach (var actor in actorsToUpdate)
                        {
                            actor.MovieID = id; // Asociază actorul la filmul curent
                        }
                    }

                    // 4. Salvează toate modificările (film și actori)
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    // Corecție: Adăugarea blocului try-catch
                    ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists");
                }
            }

            // 5. Pe eroare, reîncarcă View-ul și datele necesare
            PopulateDropDowns(movieToUpdate);
            return View(movieToUpdate);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Genre)
                .Include(m => m.Director)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);

            if (movie != null)
            {
                // Găsește toți actorii asociați și rupe legătura (setând MovieID la null)
                var associatedActors = await _context.Actor
                    .Where(a => a.MovieID == id)
                    .ToListAsync();

                foreach (var actor in associatedActors)
                {
                    actor.MovieID = null;
                }

                // Șterge filmul
                _context.Movie.Remove(movie);
            }

            // Salvează toate schimbările (actori și film) într-o singură tranzacție
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        }

        // Metodă ajutătoare pentru a popula ViewDara și ViewBag (pentru Director, Gen și Actori)
        private void PopulateDropDowns(Movie? movie)
        {
            // Dropdowns for Director and Genre
            // Presupunem că Director are proprietatea "LastName" pentru afișare.
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name", movie?.GenreID);
            ViewData["DirectorID"] = new SelectList(_context.Set<Director>(), "ID", "LastName", movie?.DirectorID);

            // Data for Actor Multi-select
            var allActors = _context.Actor.OrderBy(a => a.Name).ToList();
            var selectedActorIDs = movie?.Actors?.Select(a => a.ActorID).ToList() ?? new List<int>();

            // Acestea vor fi folosite în Create.cshtml și Edit.cshtml pentru checkbox-uri
            ViewBag.AllActors = allActors;
            ViewBag.SelectedActors = selectedActorIDs;
        }
    }
}