using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DataContext _context;

    public UsersController(DataContext context)
    {
        _context = context;
    }
[AllowAnonymous]
   [HttpGet]
   public ActionResult<IEnumerable<AppUser>> GetUsers(){

    var users = _context.Users.ToList();
    return users;
   }
[Authorize]
[HttpGet("{id:int}")]
public ActionResult<AppUser> GetUser(int id){
var user = _context.Users.Find(id);
if(user == null)
{
    return NotFound();
}
return user;
}
}
