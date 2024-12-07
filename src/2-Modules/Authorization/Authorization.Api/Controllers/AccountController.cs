// Copyright (c) CodeBlock.Dev. All rights reserved.
// See LICENSE in the project root for license information.

using CodeBlock.DevKit.Application.Srvices;
using CodeBlock.DevKit.Authorization.Api.Models;
using CodeBlock.DevKit.Authorization.UseCases.Users.GetUserById;
using CodeBlock.DevKit.Authorization.UseCases.Users.LoginUser;
using CodeBlock.DevKit.Authorization.UseCases.Users.RegisterUser;
using CodeBlock.DevKit.Core.Helpers;
using CodeBlock.DevKit.Web.Api.Filters;
using CodeBlock.DevKit.Web.Api.JwtAuthentication;
using Microsoft.AspNetCore.Mvc;

namespace CodeBlock.DevKit.Authorization.Api.Controllers;

[Route("account")]
public class AccountController : BaseApiController
{
    private readonly IJwtAuthenticationService _jwtAuthenticationService;

    public AccountController(IRequestDispatcher requestDispatcher, IJwtAuthenticationService jwtAuthenticationService)
        : base(requestDispatcher)
    {
        _jwtAuthenticationService = jwtAuthenticationService;
    }

    /// <summary>
    /// Jwt login
    /// </summary>
    [Route("login")]
    [HttpPost]
    public async Task<Result<LoginUserResponse>> Login(LoginUserRequest loginUserRequest)
    {
        var loginUserResult = await _requestDispatcher.SendQuery(loginUserRequest);
        if (!loginUserResult.IsSuccess)
            return Result.Failure<LoginUserResponse>(loginUserResult.Errors, loginUserResult.Message);

        var loginUserResponse = new LoginUserResponse
        {
            User = loginUserResult.Value,
            Token = _jwtAuthenticationService.GenerateJwtToken(loginUserResult.Value.Id, loginUserResult.Value.Email, loginUserResult.Value.Roles),
        };

        return Result.Success(loginUserResponse);
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [Route("register")]
    [HttpPost]
    public async Task<Result<RegisterUserResponse>> Register(RegisterUserRequest registerUserRequest)
    {
        var registerUserResult = await _requestDispatcher.SendCommand(registerUserRequest);

        if (!registerUserResult.IsSuccess)
            return Result.Failure<RegisterUserResponse>(registerUserResult.Errors, registerUserResult.Message);

        var getUserResult = await _requestDispatcher.SendQuery(new GetUserByIdRequest(registerUserResult.Value.EntityId));

        var loginUserResponse = new RegisterUserResponse
        {
            UserId = getUserResult.Value.Id,
            Token = _jwtAuthenticationService.GenerateJwtToken(getUserResult.Value.Id, getUserResult.Value.Email, getUserResult.Value.Roles),
        };

        return Result.Success(loginUserResponse);
    }
}
