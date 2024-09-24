using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService,IMapper mapper)
    {
        _context = context;
        _tokenService = tokenService;
        _mapper=mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {

        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken");
        }
         using var hmac = new HMACSHA512();

         var user = _mapper.Map<AppUser>(registerDto);

         user.UserName = registerDto.Username.ToLower();
         user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
         user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs=user.KnownAs,
            Gender = user.Gender
        };
    }
    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {

        var user = await _context.Users.Include(p=> p.Photos)
        .FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

        if (user == null) return Unauthorized("Invalid Username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid Password");
            }
        }

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = _tokenService.CreateToken(user),
            PhotoUrl =user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
            Gender = user.Gender
        };
    }
}
