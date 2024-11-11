using MyORM;
using MyORM.Attributes;

namespace Test.Models;

[Entity("users")]
public class User
{
	[PrimaryGeneratedColumn("UUID")]
	public int Id { get; set; }

	[Column(nullable: false)]
	public string Name { get; set; }

	[Column(defaultValue: "default@gmail.com")]
	public string Email { get; set; }

	[OneToOne<Account>(relationship: Relationship.Optional, cascade: true)]
	public Account Account { get; set; }

	[ManyToMany<User>]
	public List<User> Friends { get; set; }

	public User()
    {
    }
}