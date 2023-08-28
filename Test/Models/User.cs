using ORM;

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

	[OneToOne<Account>]
	public Account Account { get; set; }
}