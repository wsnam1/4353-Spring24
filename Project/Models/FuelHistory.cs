using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace Project.Models;


public class FuelHistory
{
    [Key]
    public string UserId { get; set; } = null!;

    [Required(ErrorMessage = "Gallons Requested is required.")]
    [Range(1, Int32.MaxValue, ErrorMessage = "Please enter a valid number for gallons requested.")]
    public int GallonsRequested { get; set; }

    public string DeliveryAddress { get; set; } =  null!;

    public string DeliveryDate { get; set; } = null!;

    public decimal SuggestedPrice { get; set; }

    public decimal TotalAmountDue { get; set; }

}