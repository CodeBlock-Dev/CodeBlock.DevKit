﻿using System.Reflection;
using CodeBlock.DevKit.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace CodeBlock.DevKit.Authorization.UI;

public static class Startup
{
    public static void AddAuthorizationUiModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorFileProvider();

        builder.AddAdminRolePolicy();
    }

    /// <summary>
    /// It shares all the razor views and components with consumer applications
    /// </summary>
    private static void AddRazorFileProvider(this IServiceCollection services)
    {
        string libraryPath = typeof(Startup).GetTypeInfo().Assembly.Location;

        services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
        {
            options.FileProviders.Add(new PhysicalFileProvider(libraryPath));
        });
    }

    private static void AddAdminRolePolicy(this WebApplicationBuilder builder)
    {
        var authorizationOptions = builder.Configuration.GetSection("Authorization").Get<AuthorizationOptions>();

        builder.Services.PostConfigure(
            (Microsoft.AspNetCore.Authorization.AuthorizationOptions options) =>
            {
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole(authorizationOptions.AdminRole));
            }
        );
    }
}