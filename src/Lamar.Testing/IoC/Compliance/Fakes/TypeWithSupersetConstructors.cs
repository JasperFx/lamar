// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lamar.Testing.IoC.Compliance.Fakes;

public class TypeWithSupersetConstructors
{
    public TypeWithSupersetConstructors(IFactoryService factoryService)
        : this(
            null,
            factoryService)
    {
    }

    public TypeWithSupersetConstructors(IFakeService fakeService)
        : this(
            fakeService,
            null)
    {
    }

    public TypeWithSupersetConstructors(
        IFakeService fakeService,
        IFactoryService factoryService)
        : this(
            fakeService,
            null,
            factoryService)
    {
    }

    public TypeWithSupersetConstructors(
        IFakeService fakeService,
        IFakeMultipleService multipleService,
        IFactoryService factoryService)
        : this(
            multipleService,
            factoryService,
            fakeService,
            null)
    {
    }

    public TypeWithSupersetConstructors(
        IFakeMultipleService multipleService,
        IFactoryService factoryService,
        IFakeService fakeService,
        IFakeScopedService scopedService)
    {
        MultipleService = multipleService;
        FactoryService = factoryService;
        Service = fakeService;
        ScopedService = scopedService;
    }

    public IFakeService Service { get; }

    public IFactoryService FactoryService { get; }

    public IFakeMultipleService MultipleService { get; }

    public IFakeScopedService ScopedService { get; }
}