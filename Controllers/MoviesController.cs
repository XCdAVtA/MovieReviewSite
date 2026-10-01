using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using MoviesAdmin.Models;

namespace MoviesAdmin.Controllers;

public class MoviesController : Controller
{
    private readonly MoviesContext _context;

    public MoviesController(MoviesContext context)
    {
        _context = context;
    }
    
    // GET: /Movies
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
        return View(movies);
    }
    
    // GET: /Movies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null) return NotFound();

        return View(movie);
    }
    
    // GET: /Movies/Create
    public IActionResult Create() => View();
    
    // POST: /Movies/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movie movie)
    {
        if (!ModelState.IsValid) return View(movie);

        _context.Add(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // GET: /Movies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return NotFound();

        return View(movie);
    }
    
    // POST: /Movies/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Movie movie)
    {
        if (id != movie.Id) return NotFound();
        if (!ModelState.IsValid) return View(movie);

        try
        {
            _context.Update(movie);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Movies.AnyAsync(m => m.Id == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }
    
    // GET: /Movies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null) return NotFound();

        return View(movie);
    }
    
    // POST: /Movies/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}