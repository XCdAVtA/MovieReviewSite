using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MoviesAdmin.Models;

public class Review
{
    public int Id { get; set; }
    
    [Display(Name = "Movie")]
    public int MovieId { get; set; }                  // foreign key

    [ValidateNever, ScaffoldColumn(false)]
    public Movie Movie { get; set; } = null!;         // navigation property

    [Required, StringLength(1000)]
    [Display(Name = "Review")]
    public string Text { get; set; } = string.Empty;

    [Range(0, 5)]
    [Display(Name = "Rating (0-5)")]
    public int Rating { get; set; }

    public bool IsPublished { get; set; } = false;    // draft until published, then locked

    [StringLength(100), Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;   // Auth0 user id

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}