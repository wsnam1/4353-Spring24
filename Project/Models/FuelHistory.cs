using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace Project.Models;


public class FuelHistory
{
    [Key]
    [Required]
    public string UserId { get; set; } = null!;

    public int GallonsRequested { get; set; }

    public string DeliveryAddress { get; set; } =  null!;

    public string DeliveryDate { get; set; } = null!;

    public decimal SuggestedPrice { get; set; }

    public decimal TotalAmountDue { get; set; }

}