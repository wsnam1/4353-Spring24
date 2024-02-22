namespace Project.Models;

public class FuelHistory
{
    public int Id { get; set; }

    public int GallonsRequested { get; set; }
    
    public string DeliveryDate { get; set; }

    public FuelHistory()
    {
        
    }
}