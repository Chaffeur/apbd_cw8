using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    
    List<TripDTO> GetAllTrips();
    
    List<ClientTripDTO> GetTripsForClient(int clientId);
    
    int CreateClient(ClientDTO client);
    
    bool RegisterClientForTrip(int clientId, int tripId);
    
    bool DeleteClientRegistrationForTrip(int clientId, int tripId);
}