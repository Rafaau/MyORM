using Microsoft.AspNetCore.Mvc;
using MyORM.Querying.Abstract;
using Test.Models;
using TestAPI.DTO;
using MyORM.Abstract;
using MyORM.Projectioner.Methods;
using MyORM.Querying.Repository;

namespace TestAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IRepository<User> _userRepository;
		private readonly DbHandler _dbHandler;

		public UserController(IRepository<User> userRepository, DbHandler dbHandler)
		{
			_userRepository = userRepository;
			_dbHandler = dbHandler;
		}

		[HttpPost]
		public IActionResult Post([FromBody] UserRequest user)
		{
			try
			{
				_userRepository.Create(user.ToProjection<User>());
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetAll")]
		public IActionResult GetAll()
		{
			try
			{
				var users = _userRepository.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetWithOrder")]
		public IActionResult GetWithOrder(string columnName, string order)
		{
			try
			{
				var users = _userRepository
					.OrderBy(columnName, order)
					.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetBy")]
		public IActionResult GetByName(string name, string email, int id)
		{
			try
			{
				var users = _userRepository
					.Where(user => user.Name == name
								&& user.Email == email
								&& user.Id == id)
					.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetOneByName")]
		public IActionResult GetOneByName(string name)
		{
			try
			{
				var user = _userRepository
					.Where(user => user.Name == name)
					.FindOne();
				return Ok(user);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetByOR")]
		public IActionResult GetByOR(string name1, string name2)
		{
			try
			{
				var users = _userRepository
					.Where(user => user.Name == name1
								|| user.Name == name2)
					.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetAllExcept")]
		public IActionResult GetAllExcept(string name)
		{
			try
			{
				var users = _userRepository
					.Where(user => user.Name != name)
					.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpGet("/GetSelect")]
		public IActionResult GetSelect()
		{
			try
			{
				var users = _userRepository
					.Select(user => new { user.Name, user.Email })
					.Find();
				return Ok(users);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpPut("/UpdateOne")]
		public IActionResult Update([FromBody] UserUpdate userToUpdate)
		{
			try
			{
				_userRepository.Update(userToUpdate.ToProjection<User>());
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpPut("/UpdateMany")]
		public IActionResult UpdateMany([FromBody] UserUpdateMany usersToUpdate, string name)
		{
			try
			{
				_userRepository.Where(user => user.Name == name)
							   .UpdateMany(usersToUpdate.ToProjection<User>());
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpDelete("/DeleteById")]
		public IActionResult DeleteById(int id)
		{
			try
			{
				_userRepository.Where(user => user.Id == id)
							   .Delete();
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpDelete("/DeleteByModel")]
		public IActionResult DeleteByModel([FromBody] UserResponse userToDelete)
		{
			try
			{
				_userRepository.Delete(userToDelete.ToProjection<User>());
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpDelete("/DeleteMany")]
		public IActionResult DeleteMany(int lessThan)
		{
			try
			{
				_userRepository.Where(user => user.Id < lessThan)
							   .Delete();
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpPost("/Transaction")]
		public IActionResult Transaction([FromBody] UserRequest user)
		{
			try
			{
				_dbHandler.BeginTransaction();
				_userRepository.Create(user.ToProjection<User>());
				_userRepository.Create(user.ToProjection<User>());
				_dbHandler.CommitTransaction();
				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
	}
}