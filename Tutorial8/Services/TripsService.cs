using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=apbd;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
    
    public List<TripDTO> GetAllTrips()
    {
        var trips = new List<TripDTO>();

        // Wybiera wszystkie informacje na temat wycieczki oraz nazwy krajów
        
        var query = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   STRING_AGG(c.Name, ', ') AS Countries
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            JOIN Country c ON ct.IdCountry = c.IdCountry
            GROUP BY t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
        ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trip = new TripDTO
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = reader.GetString(6)
                            .Split(',')
                            .Select(c => new CountryDTO { Name = c.Trim() })
                            .ToList()
                    };
                    trips.Add(trip);
                }
            }
        }

        return trips;
    }
    
    public List<ClientTripDTO> GetTripsForClient(int clientId)
    {
        var trips = new List<ClientTripDTO>();
        
        // Wybiera wszystkie dane wycieczki i informacje o rejestracji oraz płatności dla podanego id klienta

        var query = @"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.RegisteredAt, ct.PaymentDate
        FROM Trip t
        JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        WHERE ct.IdClient = @ClientId
    ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ClientId", clientId);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    DateTime registeredAt = intToTime(reader.GetInt32(6));
                    DateTime? paymentDate = reader.IsDBNull(7) ? null : intToTime(reader.GetInt32(7));

                    trips.Add(new ClientTripDTO
                    {
                        IdClient = clientId,
                        IdTrip = reader.GetInt32(0),
                        RegisteredAt = registeredAt,
                        PaymentDate = paymentDate
                    });
                }
            }
        }

        return trips;
    }
    
    private DateTime intToTime(int unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1).AddSeconds(unixTimestamp).ToUniversalTime();
        return dateTime;
    }

    
    public int CreateClient(ClientDTO client)
    {
        
        // Insertuje nowy rekord do tabeli Client
        
        var query = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)
        ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Email", client.Email);
            command.Parameters.AddWithValue("@Telephone", client.Telephone);
            command.Parameters.AddWithValue("@Pesel", client.Pesel);

            connection.Open();
            return (int)command.ExecuteScalar();
        }
    }
    
    public bool RegisterClientForTrip(int clientId, int tripId)
    {
        
        // wybiera informacje o maksymalnej ilości osób na wyciecze i liczbe osób zarejestrowanych
        
        var query = @"
            SELECT MAXPeople, COUNT(*) AS RegisteredCount
            FROM Trip t
            JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            WHERE t.IdTrip = @TripId
            GROUP BY t.MaxPeople
        ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@TripId", tripId);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    int maxPeople = reader.GetInt32(0);
                    int registeredCount = reader.GetInt32(1);
                    
                    if (registeredCount >= maxPeople)
                    {
                        return false;
                    }
                }
            }
        }

        // Wstawia nowy rekord do tabeli clientTrip 
        
        var insertQuery = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@ClientId, @TripId, @RegisteredAt)
        ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(insertQuery, connection))
        {
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@TripId", tripId);
            var unixTime = (int)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            command.Parameters.AddWithValue("@RegisteredAt", unixTime);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
    
    public bool DeleteClientRegistrationForTrip(int clientId, int tripId)
    {
        
        // Usuwa rekord z tabeli clientTrip według podanego id wycieczki i klienta
        
        var query = @"
            DELETE FROM Client_Trip
            WHERE IdClient = @ClientId AND IdTrip = @TripId
        ";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@TripId", tripId);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
}