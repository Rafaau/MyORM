using ORM;
using ORM.Attributes;

namespace Test.Models;

[Entity("users")]
public class User
{
	[PrimaryGeneratedColumn]
	public int Id;

	[Column]
	public string Name;
}