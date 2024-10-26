﻿using CodeBlock.DevKit.Authorization.Domain;
using CodeBlock.DevKit.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CodeBlock.DevKit.Authorization.Infrastructure;

public static class AuthorizationDbInitialization
{
    public static void InitialAuthorizationDb(this IServiceProvider serviceProvider)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetService<AuthorizationDbContext>();

        SeedEssentialData(serviceScope, dbContext);

        CreateIndexes(dbContext);
    }

    private static void SeedEssentialData(IServiceScope serviceScope, AuthorizationDbContext dbContext)
    {
        var authorizationSettings = serviceScope.ServiceProvider.GetService<IOptions<AuthorizationSettings>>();

        if (authorizationSettings == null)
            return;

        serviceScope.SeedDefaultRole(dbContext, authorizationSettings.Value);
        serviceScope.SeedAdminRole(dbContext, authorizationSettings.Value);
        serviceScope.SeedAdminUser(dbContext, authorizationSettings.Value);
    }

    private static void SeedAdminUser(this IServiceScope serviceScope, AuthorizationDbContext dbContext, AuthorizationSettings authorizationSettings)
    {
        if (dbContext.Users.Find(u => u.Email == authorizationSettings.AdminUser.Email).Any())
            return;

        var userRepository = serviceScope.ServiceProvider.GetService<IUserRepository>();
        var encryptionService = serviceScope.ServiceProvider.GetService<IEncryptionService>();

        var user = User.Register(userRepository, authorizationSettings.AdminUser.Email);

        user.SetPassword(encryptionService, authorizationSettings.AdminUser.Password);

        user.AddRole(authorizationSettings.AdminRole);

        userRepository.AddAsync(user).GetAwaiter().GetResult();
    }

    private static void SeedDefaultRole(
        this IServiceScope serviceScope,
        AuthorizationDbContext dbContext,
        AuthorizationSettings authorizationSettings
    )
    {
        serviceScope.CreateRole(dbContext, authorizationSettings.DefaultRole);
    }

    private static void SeedAdminRole(this IServiceScope serviceScope, AuthorizationDbContext dbContext, AuthorizationSettings authorizationSettings)
    {
        serviceScope.CreateRole(dbContext, authorizationSettings.AdminRole);
    }

    private static void CreateRole(this IServiceScope serviceScope, AuthorizationDbContext dbContext, string roleName)
    {
        if (dbContext.Roles.Find(u => u.Name == roleName).Any())
            return;

        var roleRepository = serviceScope.ServiceProvider.GetService<IRoleRepository>();

        var role = Role.Create(roleRepository, roleName);
        roleRepository.AddAsync(role).GetAwaiter().GetResult();
    }

    private static void CreateIndexes(AuthorizationDbContext dbContext)
    {
        dbContext.Users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.Email),
                new CreateIndexOptions() { Name = nameof(User.Email), Unique = true }
            )
        );

        dbContext.Roles.Indexes.CreateOneAsync(
            new CreateIndexModel<Role>(
                Builders<Role>.IndexKeys.Ascending(x => x.Name),
                new CreateIndexOptions() { Name = nameof(Role.Name), Unique = true }
            )
        );
    }
}
