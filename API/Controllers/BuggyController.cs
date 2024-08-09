using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;

        public BuggyController(DataContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetAuth()
        {
            return "secret text";
        }
        [HttpGet("server-error")]
        public ActionResult<AppUser> GetServerError()
        {
            var thing = _context.Users.Find(-1);
            if (thing == null)

                return NotFound();

            return thing;
        }



        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequet()
        {
            return BadRequest("This was not a good request");
        }
    }
}

