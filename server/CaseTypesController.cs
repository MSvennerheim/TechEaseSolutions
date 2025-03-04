using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Properties;  // ✅ Se till att detta är rätt namespace

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
            using (var conn = _db.Connection())
            {
                using (var cmd = new NpgsqlCommand("SELECT id, text FROM casetypes WHERE company = @companyId"))
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
            using (var conn = _db.Connection())
            {

                foreach (var update in updates)
                {
                    Console.WriteLine($"📌 Uppdaterar casetype ID {update.Id} → '{update.Text}'");

                    using (var cmd = new NpgsqlCommand("UPDATE casetypes SET text = @text WHERE id = @id"))
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

    // 🆕 Lägg till nya casetypes i databasen (flyttad utanför UpdateCasetype)
    [HttpPost]
    public IActionResult AddCasetype([FromBody] CaseTypeUpdate newCasetype)
    {
        Console.WriteLine($"🔍 Mottagen POST-request: '{newCasetype.Text}'");

        if (string.IsNullOrWhiteSpace(newCasetype.Text))
        {
            return BadRequest(new { error = "Text får inte vara tom" });
        }

        try
        {
            using (var conn = _db.Connection())  // 🟢 Anslut till databasen
            {
                using (var cmd = new NpgsqlCommand("INSERT INTO casetypes (text, company) VALUES (@text, @company) RETURNING id")) // 🟢 Lägg till nytt case
                {
                    cmd.Parameters.AddWithValue("@text", newCasetype.Text);
                    cmd.Parameters.AddWithValue("@company", newCasetype.Company);
                    int newId = (int)cmd.ExecuteScalar();  // 🟢 Hämta ID för den nya posten
                    
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

    [HttpDelete("{id}")]
    public IActionResult DeleteCasetype(int id)
    {
        using (var conn = _db.Connection())
        {
            using (var cmd = new NpgsqlCommand("DELETE FROM casetypes WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { error = "Ämnet kunde inte hittas" });
                }
                return Ok(new { message = "Ämnet har raderats" });
            }
        }
    } 
} 

   

// 🟢 Klass för att hantera casetypes
public class CaseTypeUpdate
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int Company { get; set; }  // 🆕 Se till att Company är med!
}