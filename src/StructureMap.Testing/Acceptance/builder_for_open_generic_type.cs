﻿using Shouldly;
using StructureMap.Building;
using StructureMap.Pipeline;
using System;
using System.Diagnostics;
using System.Reflection;
using StructureMap.TypeRules;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class builder_for_open_generic_type
    {
        #region sample_generic-builders-in-action
        [Fact]
        public void show_the_workaround_for_generic_builders()
        {
            var container = new Container(_ =>
            {
                _.For(typeof(IRepository<,>)).Use(new RepositoryInstanceFactory());
            });

            container.GetInstance<IRepository<string, int>>()
                .ShouldBeOfType<Repository<string, int>>();

            Debug.WriteLine(container.WhatDoIHave(assembly: GetType().GetAssembly()));
        }

        #endregion

        [Fact]
        public void using_repository_instance()
        {
            #region sample_using-repository-instance
            var container = new Container(_ =>
            {
                _.For<IRepository<string, int>>().UseInstance(new RepositoryInstance<string, int>());

                // or skip the custom Instance with:

                _.For<IRepository<string, int>>().Use(() => RepositoryBuilder.Build<string, int>());
            });
            #endregion
        }
    }

    #region sample_RepositoryInstanceFactory
    public class RepositoryInstanceFactory : Instance
    {
        // This is the key part here. This method is called by
        // StructureMap to "find" an Instance for a closed
        // type of IRepository<,>
        public override Instance CloseType(Type[] types)
        {
            // StructureMap will cache the object built out of this,
            // so the expensive Reflection hit only happens
            // once
            var instanceType = typeof(RepositoryInstance<,>).MakeGenericType(types);
            return Activator.CreateInstance(instanceType).As<Instance>();
        }

        // Don't worry about this one, never gets called
        public override IDependencySource ToDependencySource(Type pluginType)
        {
            throw new NotSupportedException();
        }

        public override string Description
        {
            get { return "Build Repository<T, T1>() with RepositoryBuilder"; }
        }

        public override Type ReturnedType
        {
            get { return typeof(Repository<,>); }
        }
    }

    #endregion

    #region sample_RepositoryInstance
    public class RepositoryInstance<TDocument, TQuery> : LambdaInstance<IRepository<TDocument, TQuery>>
    {
        public RepositoryInstance() : base(() => RepositoryBuilder.Build<TDocument, TQuery>())
        {
        }

        // This is purely to make the diagnostic views prettier
        public override string Description
        {
            get
            {
                return "RepositoryBuilder.Build<{0}, {1}>()"
                    .ToFormat(typeof(TDocument).Name, typeof(TQuery).Name);
            }
        }
    }

    #endregion

    #region sample_IRepository<T,T1>
    public interface IRepository<TDocument, TQuery>
    {
    }

    #endregion

    #region sample_RepositoryBuilder
    public static class RepositoryBuilder
    {
        public static IRepository<TDocument, TQuery> Build<TDocument, TQuery>()
        {
            return new Repository<TDocument, TQuery>();
        }
    }

    #endregion

    public class Repository<T, T1> : IRepository<T, T1>
    {
    }
}