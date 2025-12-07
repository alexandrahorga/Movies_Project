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
    public class ActorsController : Controller
    {
        private readonly Movies_ProjectContext _context;

        public ActorsController(Movies_ProjectContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            // Corecție: Include atât Manager, cât și Movie pentru a afișa numele/titlurile lor în View
            var actorsContext = _context.Actor
                .Include(a => a.Manager)
                .Include(a => a.Movie);

            return View(await actorsContext.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Corecție: Asigură-te că Manager și Movie sunt incluse
            var actor = await _context.Actor
                .Include(a => a.Manager)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.ActorID == id);

            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            // Corecție: Afișează "Name" pentru Manager și "Title" pentru Film în dropdown-uri
            ViewData["ManagerID"] = new SelectList(_context.Manager, "ManagerID", "Name");
            ViewData["MovieID"] = new SelectList(_context.Movie, "ID", "Title");
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Corecție: Adaugă "Name" la proprietățile legate (Bind)
        public async Task<IActionResult> Create([Bind("ActorID,Name,ManagerID,MovieID")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Corecție: Reîncarcă ViewData folosind Name și Title pe eroare de validare
            ViewData["ManagerID"] = new SelectList(_context.Manager, "ManagerID", "Name", actor.ManagerID);
            ViewData["MovieID"] = new SelectList(_context.Movie, "ID", "Title", actor.MovieID);
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            // Corecție: Afișează "Name" pentru Manager și "Title" pentru Film în dropdown-uri
            ViewData["ManagerID"] = new SelectList(_context.Manager, "ManagerID", "Name", actor.ManagerID);
            ViewData["MovieID"] = new SelectList(_context.Movie, "ID", "Title", actor.MovieID);
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Corecție: Adaugă "Name" la proprietățile legate (Bind)
        public async Task<IActionResult> Edit(int id, [Bind("ActorID,Name,ManagerID,MovieID")] Actor actor)
        {
            if (id != actor.ActorID)
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
                    if (!ActorExists(actor.ActorID))
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
            // Corecție: Reîncarcă ViewData folosind Name și Title pe eroare de validare
            ViewData["ManagerID"] = new SelectList(_context.Manager, "ManagerID", "Name", actor.ManagerID);
            ViewData["MovieID"] = new SelectList(_context.Movie, "ID", "Title", actor.MovieID);
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Corecție: Asigură-te că Manager și Movie sunt incluse
            var actor = await _context.Actor
                .Include(a => a.Manager)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.ActorID == id);

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
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.ActorID == id);
        }
    }
}