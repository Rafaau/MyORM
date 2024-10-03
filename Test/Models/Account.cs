using MyORM.Enums;
using MyORM.Attributes;

namespace Test.Models;

[Entity("accounts")]
public class Account
{
    [PrimaryGeneratedColumn]
    public int Id { get; set; }

    [Column]
    public string Nickname { get; set; }

    [OneToOne<User>(Relationship.Mandatory)]
    public User User { get; set; }

    [OneToMany<Post>(Relationship.Optional)]
    public List<Post> Posts { get; set; }

    public Account()
    {
	}
}

