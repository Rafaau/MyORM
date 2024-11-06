using MyORM;
using MyORM.Attributes;

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

	[OneToOne<Account>(relationship: Relationship.Optional, cascade: true)]
	public Account Account { get; set; }

	[ManyToMany<User>]
	public List<User> Friends { get; set; }

	public User()
    {
    }
}