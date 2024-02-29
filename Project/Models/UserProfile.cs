using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class UserProfile
{
    [Key]
    public string UserId { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Address1 { get; set; }

    public string? Address2 { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string State { get; set; }

    [Required]
    [StringLength(9, MinimumLength = 5)]
    public string Zipcode { get; set; }
}