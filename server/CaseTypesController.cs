using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;

[ApiController]
[Route("api/casetypes")]
public class CasetypesController : ControllerBase
{
    private readonly Database _db;

    public CasetypesController(Database db)
    {
        _db = db;
    }

    [HttpGet]
public IActionResult GetCasetypes([FromQuery] int companyId)
{
    var casetypes = new List<object>();
    using (var conn = _db.Connection().CreateConnection())
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
    return Ok(casetypes);
}

    [HttpPut]
    public IActionResult UpdateCasetype([FromBody] List<CaseTypeUpdate> updates)
    {
        using (var conn = _db.Connection().CreateConnection())
        {
            conn.Open();
            foreach (var update in updates)
            {
                using (var cmd = new NpgsqlCommand("UPDATE casetypes SET text = @text WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@text", update.Text);
                    cmd.Parameters.AddWithValue("@id", update.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        return Ok(new { message = "Ämnen uppdaterade" });
    }
}

public class CaseTypeUpdate
{
    public int Id { get; set; }
    public string Text { get; set; }
}
