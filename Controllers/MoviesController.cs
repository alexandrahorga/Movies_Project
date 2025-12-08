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
        public async Task<IActionResult> Index()
        {
            var movies_ProjectContext = _context.Movie
                .Include(m => m.Genre)
                .Include(m => m.Director)
             .Include(m => m.Actors!);
            //.ThenInclude(a => a.Manager);
            return View(await movies_ProjectContext.ToListAsync());
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
                 .ThenInclude(a =>a.Manager)
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
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name");
            
            ViewData["DirectorID"] = new SelectList(_context.Set<Director>(), "ID", "LastName");

            ViewBag.AllActors = _context.Actor.OrderBy(a => a.Name).ToList();

            return View();
        }

        // În MoviesController.cs
        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // NOU: Adaugă `selectedActors` pentru a primi ID-urile din formular
        public async Task<IActionResult> Create([Bind("ID,Title,DirectorID,Budget,GenreID")] Movie movie, int[] selectedActors)
        {
            if (ModelState.IsValid)
            {
                // 1. Găsește obiectele Actor corespunzătoare ID-urilor selectate
                if (selectedActors != null)
                {
                    movie.Actors = new List<Actor>();
                    foreach (var actorId in selectedActors)
                    {
                        var actorToAdd = await _context.Actor.FindAsync(actorId);
                        if (actorToAdd != null)
                        {
                            // Deoarece modelul Actor are FK la MovieID, trebuie să legăm invers
                            // Fiecare Actor referă un singur Movie.
                            // Vom actualiza MovieID al fiecărui actor selectat la ID-ul filmului.
                            // ATENȚIE: Aici presupunem că un actor poate fi asociat unui SINGUR film la un moment dat.
                             // Dacă un actor poate avea mai multe filme (relație Many-to-Many), modelul de date necesită o tabelă intermediară (join table) și o altă logică.

                            // În contextul tău actual (Actor are MovieID), cel mai simplu e să facem update direct pe actor.
                            // Dar pentru a crea un film, vom face următoarele (și vom corecta în Edit):

                            // O simplă implementare "one-to-many" (Film -> Actori)
                            if (actorToAdd.MovieID == null)
                            {
                                movie.Actors.Add(actorToAdd);
                            }
                            // NOTĂ: Logica ta de bază pare a fi un Many-to-Many simplificat.
                            // Vom merge pe soluția standard Many-to-Many pentru View-uri.

                        }
                    }
                }

                // 2. Salvează Filmul
                _context.Add(movie);
                await _context.SaveChangesAsync();

                // 3. După salvarea filmului, actualizăm și actorii asociați
                if (selectedActors != null)
                {
                    foreach (var actorId in selectedActors)
                    {
                        var actorToUpdate = await _context.Actor.FindAsync(actorId);
                        if (actorToUpdate != null)
                        {
                            actorToUpdate.MovieID = movie.ID; // Asociază actorul la filmul nou creat
                        }
                    }
                    await _context.SaveChangesAsync();
                }


                return RedirectToAction(nameof(Index));
            }

            // Pe eroare, reîncarcă View-ul
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name", movie.GenreID);
            ViewData["DirectorID"] = new SelectList(_context.Set<Director>(), "ID", "LastName", movie.DirectorID);
            ViewBag.AllActors = _context.Actor.OrderBy(a => a.Name).ToList();

            return View(movie);
        }
        // În MoviesController.cs
        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // NOU: Includem colecția de Actori pentru a ști care sunt deja selectați
            var movie = await _context.Movie
                .Include(m => m.Actors)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            // NOU: Trimite toți actorii disponibili către View
            ViewBag.AllActors = _context.Actor.OrderBy(a => a.Name).ToList();


            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name", movie.GenreID);
            ViewData["DirectorID"] = new SelectList(_context.Set<Director>(), "ID", "LastName", movie.DirectorID);

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // NOU: Primim actorii selectați din formular
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,DirectorID,Budget,GenreID")] Movie movie, int[] selectedActors)
        {
            if (id != movie.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Obține filmul din DB cu actorii asociați
                    var movieToUpdate = await _context.Movie
                        .Include(m => m.Actors)
                        .FirstOrDefaultAsync(m => m.ID == id);

                    if (movieToUpdate == null)
                    {
                        return NotFound();
                    }

                    // 2. Actualizează proprietățile de bază
                    movieToUpdate.Title = movie.Title;
                    movieToUpdate.DirectorID = movie.DirectorID;
                    movieToUpdate.Budget = movie.Budget;
                    movieToUpdate.GenreID = movie.GenreID;


                    // 3. Actualizează asocierile cu actorii (Logică complexă)
                    // Logica actuală (Actor are MovieID) impune ca actualizarea să se facă pe obiectele Actor:

                    // a) Des-asociază toți actorii care erau anterior asociați acestui film
                    var oldActors = await _context.Actor.Where(a => a.MovieID == id).ToListAsync();
                    foreach (var actor in oldActors)
                    {
                        actor.MovieID = null;
                    }

                    // b) Asociază actorii nou selectați
                    if (selectedActors != null)
                    {
                        foreach (var actorId in selectedActors)
                        {
                            var actorToUpdate = await _context.Actor.FindAsync(actorId);
                            if (actorToUpdate != null)
                            {
                                actorToUpdate.MovieID = id; // Asociază actorul la filmul curent
                            }
                        }
                    }


                    _context.Update(movieToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.ID))
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

            // Pe eroare, reîncarcă View-ul
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name", movie.GenreID);
            ViewData["DirectorID"] = new SelectList(_context.Set<Director>(), "ID", "LastName", movie.DirectorID);
            ViewBag.AllActors = _context.Actor.OrderBy(a => a.Name).ToList();

            return View(movie);
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
                .Include(m =>m.Actors!)
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
            // Găsește filmul (nu e nevoie să includem relațiile pentru ștergere)
            var movie = await _context.Movie.FindAsync(id);

            if (movie != null)
            {
                // 1. Găsește toți actorii care sunt asociați cu acest film (au MovieID = id)
                var associatedActors = await _context.Actor
                    .Where(a => a.MovieID == id)
                    .ToListAsync();

                // 2. Rupe legătura: setează MovieID la null pentru a preveni eroarea Foreign Key
                foreach (var actor in associatedActors)
                {
                    actor.MovieID = null;
                }

                // 3. Salvează schimbările actorilor (opțional, dar recomandat)
                // await _context.SaveChangesAsync(); // Dacă folosești SaveChanges la final, nu e neapărat necesar aici

                // 4. Șterge filmul
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
    }
}
