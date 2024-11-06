using MyORM;
using MyORM.Attributes;

namespace Test.Models;

[Entity("tags")]
public class Tag
{
	[PrimaryGeneratedColumn]
	public int Id { get; set; }

	[Column]
	public string Name { get; set; }

	[ManyToMany<Post>]
	public List<Post> Posts { get; set; }

	public Tag()
	{
	}
}