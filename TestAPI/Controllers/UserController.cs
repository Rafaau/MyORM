using Microsoft.AspNetCore.Mvc;
using ORM.Querying;
using ORM.Querying.Abstract;
using Test.Models;
using TestAPI.DTO;
using ORM.Projection;

namespace TestAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IRepository<User> _userRepository;

        public UserController(IRepository<User> userRepository)
		{
			_userRepository = userRepository;
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
        public IActionResult GetWithOrder(string order1)
        {
            try
            {
                var users = _userRepository.Find(
                    order: new Order
                    {
                        {"ASC", order1}
                    }
                );
                return Ok(users);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("/GetByName")]
        public IActionResult GetByName(string name1, string name2, string email)
        {
            try
            {
                var users = _userRepository.Find(
                    where: new Where
                    {
                        {"Name", name1, name2},
                        {"Email", email}
                    }
                );
                return Ok(users);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("/GetByNameWithOrder")]
        public IActionResult GetByNameWithOrder(string name1, string name2, string order)
        {
            try
            {
                var users = _userRepository.Find(
                    where: new Where
                    {
                        {"Name", name1, name2},
                    },
                    order: new Order
                    {
                        {"ASC", order}
                    }
                );
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
                var user = _userRepository.FindOne(
                    where: new Where
                    {
                        {"Name", name}
                    }
                );
                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] User userToUpdate)
        {
            try
            {
                _userRepository.Update(
                    where: new Where
                    {
                        { "Id", userToUpdate.Id }
                    },
                    userToUpdate
                );
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                _userRepository.Delete(
                    where: new Where
                    {
                        { "Id", id }
                    }
                );
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
}
}