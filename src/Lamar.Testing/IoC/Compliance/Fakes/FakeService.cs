// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lamar.Testing.IoC.Compliance.Fakes
{
    public class FakeService : IFakeEveryService, IDisposable
    {
        public PocoClass Value { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(FakeService));
            }

            Disposed = true;
        }
    }
}
