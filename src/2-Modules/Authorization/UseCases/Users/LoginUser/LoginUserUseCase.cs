﻿using AutoMapper;
using CodeBlock.DevKit.Application.Queries;
using CodeBlock.DevKit.Authorization.Domain;
using CodeBlock.DevKit.Authorization.Dtos;
using CodeBlock.DevKit.Authorization.Resources;
using CodeBlock.DevKit.Domain.Services;
using MediatR;
using ApplicationException = CodeBlock.DevKit.Application.Exceptions.ApplicationException;

namespace CodeBlock.DevKit.Authorization.UseCases.Users.LoginUser;

public class LoginUserUseCase : BaseQueryHandler, IRequestHandler<LoginUserRequest, GetUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;

    public LoginUserUseCase(IUserRepository userRepository, IEncryptionService encryptionService, IMapper mapper)
        : base(mapper)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
    }

    public async Task<GetUserDto> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            throw new ApplicationException(AuthorizationResource.User_Email_Is_Wrong);

        if (!user.IsValidPassword(_encryptionService, request.Password))
            throw new ApplicationException(AuthorizationResource.User_Password_Is_Wrong);

        return _mapper.Map<GetUserDto>(user);
    }
}