namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public int IdClient { get; set; }
    public int IdTrip { get; set; } 
    public DateTime RegisteredAt { get; set; } 
    public DateTime? PaymentDate { get; set; }
}