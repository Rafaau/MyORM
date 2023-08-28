using ORM;

namespace Test.Models;

[Entity("accounts")]
public class Account
{
    [PrimaryGeneratedColumn]
    public int Id { get; set; }

    [Column]
    public string Nickname { get; set; }

    [OneToOne<User>]
    public User User { get; set; }
}

