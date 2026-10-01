using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models;

public class Movie
{
    public int Id { get; set; }

    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;

    [Required] public string Synopsis { get; set; } = string.Empty;

    [Required, StringLength(50)] public string Genre { get; set; } = string.Empty;

    [Required, StringLength(10)] public string Rating { get; set; } = string.Empty;
    
    [Range(0, 24), Display(Name = "Runtime (hours)")] public int RuntimeHours { get; set; }
    
    [Range(0, 59), Display(Name = "Runtime (minutes)")] public int RuntimeMinutes { get; set; }
    
    [DataType(DataType.Date), Display(Name = "Release Date")] public DateTime ReleaseDate { get; set; }
}