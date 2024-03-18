using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class UserProfile
{
    [Key]
    public string UserId { get; set; } = null!;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(50, ErrorMessage = "Full Name must be at most 50 characters long.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Address 1 is required.")]
    [StringLength(100, ErrorMessage = "Address 1 must be at most 100 characters long.")]
    public string Address1 { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Address 2 must be at most 100 characters long.")]
    public string? Address2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, ErrorMessage = "City must be at most 100 characters long.")]
    public string City { get; set; } = null!;

    [Required(ErrorMessage = "State is required.")]
    [StringLength(2, ErrorMessage = "State must be a 2 character state code.")]
    public string State { get; set; } = null!;

    [Required(ErrorMessage = "Zipcode is required.")]
    // At least 5 characters followed - and 4 additional characters
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Zipcode must be a 5-digit code, optionally followed by a hyphen and a 4-digit code (e.g., 12345 or 12345-6789).")]
    public string Zipcode { get; set; } = null!;
}