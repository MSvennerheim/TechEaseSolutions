using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Properties;  // ✅ Se till att detta är rätt namespace
using System.ComponentModel.DataAnnotations;  // Lägg till denna using

[ApiController]
[Route("api/casetypes")]
public class CaseTypesController : ControllerBase
{
    private readonly Database _db;

    public CaseTypesController(Database db)
    {
        _db = db;
    }

    // 🟢 Hämta casetypes från databasen
    [HttpGet]
    public IActionResult GetCasetypes([FromQuery] int companyId)
    {
        Console.WriteLine($"🔍 Mottagen GET-request för companyId: {companyId}");
        var casetypes = new List<object>();

        try
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, text FROM casetypes WHERE company = @companyId", conn))
                {
                    cmd.Parameters.AddWithValue("@companyId", companyId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            casetypes.Add(new { id = reader.GetInt32(0), text = reader.GetString(1) });
                        }
                    }
                }
            }

            Console.WriteLine($"✅ Returnerar {casetypes.Count} casetypes.");
            return Ok(casetypes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fel vid hämtning av casetypes: {ex.Message}");
            return StatusCode(500, new { error = "Internt serverfel", details = ex.Message });
        }
    }

    // 🟢 Uppdatera casetypes i databasen
    [HttpPut]
    public IActionResult UpdateCasetype([FromBody] List<CaseTypeUpdate> updates)
    {
        Console.WriteLine("🔍 Mottagen PUT-request");

        if (updates == null || updates.Count == 0)
        {
            Console.WriteLine("❌ Ingen data mottagen!");
            return BadRequest(new { error = "Ingen data skickades" });
        }

        try
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                foreach (var update in updates)
                {
                    Console.WriteLine($"📌 Uppdaterar casetype ID {update.Id} → '{update.Text}'");

                    using (var cmd = new NpgsqlCommand("UPDATE casetypes SET text = @text WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@text", update.Text);
                        cmd.Parameters.AddWithValue("@id", update.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"⚠️ Ingen rad uppdaterades för ID {update.Id}!");
                        }
                    }
                }
            }

            Console.WriteLine("✅ Ämnen uppdaterade i databasen!");
            return Ok(new { message = "Cases updated" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fel vid uppdatering: {ex.Message}");
            return StatusCode(500, new { error = "Internt serverfel", details = ex.Message });
        }
    }

    // 🟢 Klass för att hantera casetypes
    public class CaseTypeRequest
    {
        [Required(ErrorMessage = "NewCasetype är obligatoriskt")]
        public CaseTypeUpdate NewCasetype { get; set; } = null!;
    }

    // Ny POST-metod med förenklad struktur
    [HttpPost]
    public IActionResult AddCasetype([FromBody] NewCaseTypeRequest request)
    {
        try 
        {
            Console.WriteLine($"🔍 Mottagen POST-request: {System.Text.Json.JsonSerializer.Serialize(request)}");

            if (string.IsNullOrWhiteSpace(request?.Text))
            {
                return BadRequest(new { error = "Text får inte vara tom" });
            }

            using (var conn = _db.GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO casetypes (text, company) VALUES (@text, @company) RETURNING id", 
                    conn))
                {
                    cmd.Parameters.AddWithValue("@text", request.Text);
                    cmd.Parameters.AddWithValue("@company", request.Company);
                    
                    var newId = (int)cmd.ExecuteScalar();
                    Console.WriteLine($"✅ Nytt ämne sparat med ID {newId}");
                    return Ok(new { id = newId, message = "Casetype added" });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fel vid skapande: {ex.Message}");
            return StatusCode(500, new { error = "Internt serverfel", details = ex.Message });
        }
    }
}

// 🟢 Klass för att hantera casetypes
public class CaseTypeUpdate
{
    public int Id { get; set; }  // Inte nullable för uppdateringar
    public string Text { get; set; } = string.Empty;
    public int Company { get; set; }

    public override string ToString()
    {
        return $"CaseTypeUpdate(Id={Id}, Text='{Text}', Company={Company})";
    }
}

public class NewCaseTypeRequest
{
    public string Text { get; set; } = string.Empty;
    public int Company { get; set; }
}
