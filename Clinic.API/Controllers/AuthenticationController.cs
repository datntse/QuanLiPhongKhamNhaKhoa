﻿using Clinic.API.Services;
using Clinic.Core.Constants;
using Clinic.Core.Models;
using Clinic.Infracstruture.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Clinic.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class AuthenticateController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthenticateController(IJwtTokenService jwtTokenService, IUserService userService, ICurrentUserService currentUserService)
        {
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [AllowAnonymous]
        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp(UserDTO signUpModel)
        {
            var result = await _userService.SignUpAsync(signUpModel);
            if (result == null)
            {
                return BadRequest("Email is existed");
            }
            if (result.Succeeded)
            {
                return Ok(result.Succeeded);
            }

            return StatusCode(500);
        }

        [AllowAnonymous]
        [HttpPost("sigIn")]
        public async Task<IActionResult> SignIn(UserSignIn signIn)
        {
            var user = await _userService.SignInAsync(signIn);
            if (user == null || !(user.Status != 0))
            {
                return Unauthorized();
            }
            var userRoles = await _userService.GetRolesAsync(user);
            var accessToken = _jwtTokenService.CreateToken(user, userRoles);
            var refreshToken = _jwtTokenService.CreateRefeshToken();
            user.RefreshToken = refreshToken;
            user.DateExpireRefreshToken = DateTime.Now.AddDays(7);
            _userService.Update(user);
            var result = await _userService.SaveChangeAsync();
            if (result)
            {
                return Ok(new { token = accessToken, refreshToken });

            }
            return BadRequest("Failed to update user's token");
        }

        [HttpDelete("signOut")]
        public async Task<IActionResult> SignOut()
        {
            var user = await _currentUserService.GetUser();
            if (user is null)
                return Unauthorized();
            var currentUser = _userService.Get(_ => _.Id.Equals(user.Id)).FirstOrDefault();
            currentUser.RefreshToken = null;
            _userService.Update(currentUser);
            await _userService.SaveChangeAsync();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> refeshToken(string refreshToken, string userId)
        {
            var user = await _userService.FindAsync(userId);
            if (user == null || !(user.Status != 0) || user.RefreshToken != refreshToken || user.DateExpireRefreshToken < DateTime.UtcNow)
            {
                return BadRequest(new Message
                {
                    Content = "Not permission",
                    StatusCode = 404
                });
            }
            var userRoles = await _userService.GetRolesAsync(user);
            var newRefreshToken = _jwtTokenService.CreateRefeshToken();
            user.RefreshToken = newRefreshToken;
            user.DateExpireRefreshToken = DateTime.Now.AddDays(7);
            var token = _jwtTokenService.CreateToken(user, userRoles);
            _userService.Update(user);
            await _userService.SaveChangeAsync();
            return Ok(new { token = token, refreshToken = newRefreshToken });
        }

        [HttpGet("profile/id")]
        public IActionResult GetProfile(string id)
        {
            var result = _userService.Get(_ => _.Id.Equals(id));
            return Ok(result);
        }

        [HttpGet("currentUser")]
        public async Task<IActionResult> getCurrentUserId()
        {
            var user = _currentUserService.GetUser();
            return Ok(new { user });
        }



    }
}

