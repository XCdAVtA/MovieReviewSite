using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Data;
using MoviesAdmin.Models;

namespace MoviesAdmin.Controllers;

[Authorize(Roles = "critic")]
public class ReviewsController : Controller
{
    private readonly MoviesContext _context;
    private string _userId = string.Empty;

    public ReviewsController(MoviesContext context) => _context = context;

    // User is not available in the constructor, so read it here (same trick as the reference)
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        _userId = User.GetUserId();
    }

    private IQueryable<Review> MyReviews => _context.Reviews.Where(r => r.CreatedBy == _userId);

    private IQueryable<Review> MyDrafts =>
        MyReviews.Include(r => r.Movie).Where(r => !r.IsPublished);

    private void LoadMovies(int? selected) =>
        ViewData["MovieId"] = new SelectList(_context.Movies.OrderBy(m => m.Title), "Id", "Title", selected);

    public async Task<IActionResult> Index() =>
        View(await MyReviews.Include(r => r.Movie).OrderByDescending(r => r.CreatedDate).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var review = await MyReviews.Include(r => r.Movie).FirstOrDefaultAsync(r => r.Id == id);
        return review == null ? NotFound() : View(review);
    }

    public IActionResult Create(int? movieId)
    {
        LoadMovies(movieId);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MovieId,Text,Rating,IsPublished")] Review review)
    {
        review.CreatedBy = _userId;

        if (await MyReviews.AnyAsync(r => r.MovieId == review.MovieId))
            ModelState.AddModelError("MovieId", "You already have a review for this movie.");

        if (!ModelState.IsValid)
        {
            LoadMovies(review.MovieId);
            return View(review);
        }

        _context.Add(review);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var review = await MyDrafts.FirstOrDefaultAsync(r => r.Id == id);
        return review == null ? NotFound() : View(review);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Text,Rating,IsPublished")] Review posted)
    {
        if (id != posted.Id) return NotFound();

        var review = await MyDrafts.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return NotFound();      // not yours, or already published

        review.Text = posted.Text;
        review.Rating = posted.Rating;
        review.IsPublished = posted.IsPublished;    // tick the box to publish

        if (!ModelState.IsValid) return View(review);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var review = await MyDrafts.FirstOrDefaultAsync(r => r.Id == id);
        return review == null ? NotFound() : View(review);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var review = await MyDrafts.FirstOrDefaultAsync(r => r.Id == id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
