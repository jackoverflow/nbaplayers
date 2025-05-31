using System.ComponentModel.DataAnnotations.Schema;

namespace nbaplayers.Models {
 public class NBAPlayer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [Column(TypeName = "date")]
    public DateTime DateOfBirth { get; set; }
    
    public string Team { get; set; }
    public bool Retired { get; set; }
    public bool Injured { get; set; }

    public NBAPlayer(string firstName, string lastName, DateTime dateOfBirth, string team, bool retired, bool injured)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Team = team;
        Retired = retired;
        Injured = injured;
    }
}
   
}
