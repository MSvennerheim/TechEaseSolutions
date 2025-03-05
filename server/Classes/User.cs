namespace server;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public int Company { get; set; }
    public bool CsRep { get; set; }
    public bool IsAdmin { get; set; }
    public string? CompanyName { get; set; }
    
}