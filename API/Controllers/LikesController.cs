using System;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly ILikesRepository _likesRepository;
        public LikesController(ILikesRepository likesRepository, DataContext context)
        {
            _likesRepository = likesRepository;
        }
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int TargetUserId)
        {
            var SourceUserId = User.GetUserId();

            if (SourceUserId == TargetUserId) return BadRequest("You cannot like yourself");

            var existinglike = await _likesRepository.GetUserLike(SourceUserId, TargetUserId);

            if (existinglike == null)
            {
                var like = new UserLike
                {
                    TargetUserId = TargetUserId,
                    SourceUserId = SourceUserId
                };
                _likesRepository.AddLike(like);
            }
            else
            {
                _likesRepository.DeleteLike(existinglike);
            }

            if (await _likesRepository.SaveChanges())
            {
                return Ok();
            }

            return BadRequest("Failed to update like");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            return Ok(await _likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }

    }
}
