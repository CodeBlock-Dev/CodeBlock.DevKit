// Copyright (c) CodeBlock.Dev. All rights reserved.
// See LICENSE in the project root for license information.

using CodeBlock.DevKit.Application.Queries;
using CodeBlock.DevKit.Authorization.Dtos;

namespace CodeBlock.DevKit.Authorization.UseCases.Users.GetUserByEmail;

public class GetUserByEmailRequest : BaseQuery<GetUserDto>
{
    public GetUserByEmailRequest(string email)
    {
        Email = email;
    }

    public string Email { get; set; }
}
