// Copyright (c) CodeBlock.Dev. All rights reserved.
// See LICENSE in the project root for license information.

using CodeBlock.DevKit.Application.Commands;
using CodeBlock.DevKit.Application.Queries;
using CodeBlock.DevKit.Application.Srvices;
using CodeBlock.DevKit.Core.Helpers;
using CodeBlock.DevKit.Domain.Events;
using MediatR;

namespace CodeBlock.DevKit.Infrastructure.Services;

/// <summary>
/// Implementation of IRequestDispatcher using MediatR.
/// </summary>
internal class MediatRDispatcher : IRequestDispatcher
{
    #region Fields

    private readonly IMediator _mediator;
    private readonly INotificationService _notifications;

    #endregion

    #region Ctors

    public MediatRDispatcher(IMediator mediator, INotificationService notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    #endregion

    #region Public Methods






    /// <summary>
    ///
    /// </summary>
    public async Task<Result<CommandResult>> SendCommand<TCommand>(TCommand cmd)
        where TCommand : BaseCommand
    {
        var result = await _mediator.Send(cmd);

        //get notification errors
        var errors = _notifications.GetList();

        //result is null when throw application or domain exception
        if (result == null)
            return Result.Failure<CommandResult>(errors);

        //if there is any notification error so result is failed
        if (errors.Count > 0)
            return Result.Failure<CommandResult>(errors, result.Message);

        return Result.Success(result, result.Message);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task<Result<TQueryResult>> SendQuery<TQueryResult>(BaseQuery<TQueryResult> query)
    {
        var result = await _mediator.Send(query);
        if (_notifications.HasAny())
            return Result.Failure<TQueryResult>(_notifications.GetList());

        return Result.Success(result);
    }

    /// <summary>
    ///
    /// </summary>
    public async Task PublishEvent(DomainEvent @event)
    {
        await _mediator.Publish(@event);
    }

    #endregion

    #region Private Methods



    #endregion
}
