using MyORM.Attributes;
using MyORM.Enums;
using Test.Models;

namespace TestAPI.DTO;

public class AccountRequest
{
	public string Nickname { get; set; }
}