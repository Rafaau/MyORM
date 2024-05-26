namespace TestAPI.DTO;

public class UserRequest
{
	public string Name { get; set; }
	public string Email { get; set; }
}

public class UserResponse
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
}

public class UserUpdate
{
    public int Id { get; set; }
	public string? Name { get; set; }
	public string? Email { get; set; }
}

public class UserUpdateMany
{
	public string? Name { get; set; }
	public string? Email { get; set; }
}

