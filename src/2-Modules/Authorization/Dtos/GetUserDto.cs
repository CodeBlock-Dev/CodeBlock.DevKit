﻿namespace CodeBlock.DevKit.Authorization.Dtos;

public class GetUserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public IEnumerable<string> Roles { get; set; }
}
