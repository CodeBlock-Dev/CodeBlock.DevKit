﻿using CodeBlock.DevKit.Core.Exceptions;

namespace CodeBlock.DevKit.Domain.Exceptions;

/// <summary>
///
/// </summary>
public class DomainException : ManagedException
{
    public DomainException(string message)
        : base(message) { }
}