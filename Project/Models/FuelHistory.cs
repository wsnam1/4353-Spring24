using Microsoft.Build.Framework;

namespace Project.Models;


public class FuelHistory
{
    public int Id { get; set; }

    [Required]
    public int GallonsRequested { get; set; }
    
    public string DeliveryDate { get; set; }
    
}