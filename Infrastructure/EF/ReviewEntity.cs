namespace Infrastructure.EF;

public class ReviewEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } // AspNetUser Id
    public int CompanyRank { get; set; } // połączenie z CompanyEntity
    public string Content { get; set; }

    public UserEntity User { get; set; }
    public CompanyEntity Company { get; set; }
}