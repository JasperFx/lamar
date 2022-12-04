// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lamar.Testing.IoC.Compliance.Fakes;

public class ClassWithOptionalArgsCtorWithStructs
{
    public ClassWithOptionalArgsCtorWithStructs(
        DateTime dateTime = new(),
        DateTime dateTimeDefault = default,
        TimeSpan timeSpan = new(),
        TimeSpan timeSpanDefault = default,
        DateTimeOffset dateTimeOffset = new(),
        DateTimeOffset dateTimeOffsetDefault = default,
        Guid guid = new(),
        Guid guidDefault = default,
        CustomStruct customStruct = new(),
        CustomStruct customStructDefault = default
    )
    {
    }

    public struct CustomStruct
    {
    }
}