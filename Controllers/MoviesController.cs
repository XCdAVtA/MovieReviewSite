using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using MoviesAdmin.Models;
using MoviesAdmin.Services;

namespace MoviesAdmin.Controllers;

[Authorize]
public class MoviesController : Controller
{
    private readonly MoviesContext _context;
    private readonly IAzureStorageService _storage;

    public MoviesController(MoviesContext context, IAzureStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies.OrderByDescending(m => m.ReleaseDate).ToListAsync();
        return View(movies);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null) return NotFound();

        ViewBag.PublishedReviewCount = await _context.Reviews
            .CountAsync(r => r.MovieId == id && r.IsPublished);

        return View(movie);
    }

    [Authorize(Roles = "administrator")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "administrator")]
    public async Task<IActionResult> Create(
        [Bind("Title,Synopsis,Genre,Rating,RuntimeHours,RuntimeMinutes,ReleaseDate")] Movie movie,
        IFormFile? ImageUpload)
    {
        if (ImageUpload == null)
            ModelState.AddModelError("ImageUpload", "A poster image is required.");
        else if (!ImageUpload.ContentType.StartsWith("image/"))
            ModelState.AddModelError("ImageUpload", "The file must be an image.");

        if (!ModelState.IsValid) return View(movie);

        // unique file name (GUID + original extension)
        string newName = $"{Guid.NewGuid()}{Path.GetExtension(ImageUpload!.FileName)}";
        movie.PosterUrl = await _storage.UploadFileAsync(ImageUpload, newName);

        _context.Add(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return NotFound();
        return View(movie);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "administrator")]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Title,Synopsis,Genre,Rating,RuntimeHours,RuntimeMinutes,ReleaseDate")] Movie movie,
        IFormFile? ImageUpload)
    {
        if (id != movie.Id) return NotFound();

        if (ImageUpload != null && !ImageUpload.ContentType.StartsWith("image/"))
            ModelState.AddModelError("ImageUpload", "The file must be an image.");

        // the current poster comes from the database, not from the form
        string? oldUrl = await _context.Movies.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => m.PosterUrl)
            .FirstOrDefaultAsync();
        if (oldUrl == null) return NotFound();
        movie.PosterUrl = oldUrl;

        if (!ModelState.IsValid) return View(movie);

        if (ImageUpload != null)
        {
            string newName = $"{Guid.NewGuid()}{Path.GetExtension(ImageUpload.FileName)}";
            movie.PosterUrl = await _storage.UploadFileAsync(ImageUpload, newName);
        }

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

        // remove the replaced poster only after the database save succeeded
        if (ImageUpload != null) await _storage.DeleteFileAsync(oldUrl);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null) return NotFound();
        return View(movie);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            string posterUrl = movie.PosterUrl;
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            await _storage.DeleteFileAsync(posterUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}