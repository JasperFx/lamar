// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lamar.Testing.IoC.Compliance.Fakes;

public class FakeDisposableCallbackService : IDisposable
{
    private static int _globalId;
    private readonly FakeDisposeCallback _callback;
    private readonly int _id;

    public FakeDisposableCallbackService(FakeDisposeCallback callback)
    {
        _id = _globalId++;
        _callback = callback;
    }

    public void Dispose()
    {
        _callback.Disposed.Add(this);
    }

    public override string ToString()
    {
        return _id.ToString();
    }
}