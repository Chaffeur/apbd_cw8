using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripService;

        public TripsController(ITripsService tripService)
        {
            _tripService = tripService;
        }

        // GET /api/trips
        // Wyświetla wszystkie wycieczki
        [HttpGet]
        public IActionResult GetAllTrips()
        {
            var trips = _tripService.GetAllTrips();
            return Ok(trips);
        }

        // GET /api/clients/{id}/trips
        // Wyświetla wszystkie wycieczki na które klient z podanym id jest zarejestrowany
        [HttpGet("/api/clients/{id}/trips")]
        public IActionResult GetTripsForClient(int id)
        {
            var trips = _tripService.GetTripsForClient(id);
            if (trips == null)
                return NotFound("Nie znaleziono klienta");
        
            return Ok(trips);
        }

        // POST /api/clients
        // Tworzy klienta i dodaje go do bazy danych na podstawie podanego body
        [HttpPost("/api/clients")]
        public IActionResult CreateClient([FromBody] ClientDTO client)
        {
            if (!ModelState.IsValid)
                return BadRequest("Błędne dane klienta");

            var clientId = _tripService.CreateClient(client);
            return CreatedAtAction(nameof(GetTripsForClient), new { id = clientId }, clientId);
        }

        // PUT /api/clients/{id}/trips/{tripId}
        // Tworzy nowy rekord clientTrip (rejestruje klienta na wycieczke po podanym id klienta i wycieczki)
        [HttpPut("/api/clients/{id}/trips/{tripId}")]
        public IActionResult RegisterClientForTrip(int id, int tripId)
        {
            var success = _tripService.RegisterClientForTrip(id, tripId);
            if (!success)
                return BadRequest("Nie udało się zarejestrować");
        
            return Ok("Klient zarejestrowany na wycieczke");
        }

        // DELETE /api/clients/{id}/trips/{tripId}
        // Usuwa rekord clientTrip (anuluje rejestracje klienta na wycieczke po podanym id wycieczki i klienta)
        [HttpDelete("/api/clients/{id}/trips/{tripId}")]
        public IActionResult DeleteClientRegistrationForTrip(int id, int tripId)
        {
            var success = _tripService.DeleteClientRegistrationForTrip(id, tripId);
            if (!success)
                return NotFound("Nie znaleziono rejestracji");
        
            return Ok("Usunięto rejestracje");
        }
    }
}
