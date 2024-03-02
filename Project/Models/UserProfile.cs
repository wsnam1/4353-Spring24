using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class UserProfile
{
    [Key]
    public string UserId { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string State { get; set; } = null!;

    [Required]
    [StringLength(9, MinimumLength = 5)]
    public string Zipcode { get; set; } = null!;
}