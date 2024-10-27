using MyORM.Enums;
using MyORM.Attributes;

namespace Test.Models;

[Entity("posts")]
public class Post
{
	[PrimaryGeneratedColumn]
	public int Id { get; set; }

	[Column]
	public DateTime SendDate { get; set; } = DateTime.Now;

	[Column]
	public string Content { get; set; }

	[ManyToOne<Account>(Relationship.Mandatory)]
	public Account Account { get; set; }

	[ManyToMany<Tag>]
	public List<Tag> Tags { get; set; } = new();

	public Post()
	{
	}
}