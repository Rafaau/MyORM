using ORM;
using ORM.Enums;

namespace Test.Models;

[Entity("users")]
public class User
{
	[PrimaryGeneratedColumn]
	public int Id { get; set; }

	[Column]
	public string Name { get; set; }

	[Column]
	public string Email { get; set; }

	[OneToOne<Account>(Relationship.Optional)]
	public Account Account { get; set; }

    public User()
    {
    }
}