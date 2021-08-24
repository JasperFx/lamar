module.exports = {
  base: '/lamar/',
  lang: 'en-US',
  title: 'Lamar',
  description: '.NET Fast Inversion of Control and Dynamic code weaving tools',
  head: [
  ],
  themeConfig: {
    logo: null,
    repo: 'JasperFx/lamar',
    docsDir: 'docs',
    docsBranch: 'master',
    editLinks: true,
    editLinkText: 'Suggest changes to this page',

    nav: [
      { text: 'Guide', link: '/guide/' },
      { text: 'Release Notes', link: 'https://github.com/JasperFx/lamar/releases' },
      { text: 'Gitter | Join Chat', link: 'https://gitter.im/JasperFx/lamar?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge' }
    ],

    algolia: {
      apiKey: 'b9590d6ae8461868e8449d64285f57c1',
      indexName: 'lamar_index'
    },

    sidebar: [
      {
        text: 'Getting Started',
        link: '/guide/'
      },
      {
        text: 'Inversion of Control',
        link: '/guide/ioc/',
        children: getIOCSideBar()
      },
      {
        text: 'Compilation',
        link: '/guide/compilation/',
        children: getCompilationSideBar()
      },
    ]
  }
}

function getIOCSideBar() {
  return [
    {
      text: 'Software Design Concepts',
      link: '/guide/ioc/concepts'
    },
    {
      text: 'Bootstrapping a Container',
      link: '/guide/ioc/bootstrapping'
    },
    {
      text: 'Integration with ASP.NET Core',
      link: '/guide/ioc/aspnetcore'
    },
    {
      text: 'Integration with Blazor',
      link: '/guide/ioc/blazor'
    },
    {
      text: 'Registration',
      link: '/guide/ioc/registration/',
      children: [
        {
          text: 'ServiceRegistry DSL',
          link: '/guide/ioc/registration/registry-dsl'
        },
        {
          text: 'Inline Dependencies',
          link: '/guide/ioc/registration/inline-dependencies'
        },
        {
          text: 'Auto-Registration and Conventions',
          link: '/guide/ioc/registration/auto-registration-and-conventions'
        },
        {
          text: 'Working with IConfiguredInstance',
          link: '/guide/ioc/registration/configured-instance'
        },
        {
          text: 'Policies',
          link: '/guide/ioc/registration/policies'
        },
        {
          text: 'Changing Configuration at Runtime',
          link: '/guide/ioc/registration/changing-configuration-at-runtime'
        },
        {
          text: 'Constructor Selection',
          link: '/guide/ioc/registration/constructor-selection'
        },
        {
          text: 'Registering Existing Objects',
          link: '/guide/ioc/registration/existing-objects'
        },
        {
          text: 'Using Attributes for Configuration',
          link: '/guide/ioc/registration/attributes'
        },
        {
          text: 'Overriding Service Registrations',
          link: '/guide/ioc/registration/overrides'
        },
      ]
    },
    {
      text: 'Service Lifetimes',
      link: '/guide/ioc/lifetime'
    },
    {
      text: 'Resolving Services',
      link: '/guide/ioc/resolving/',
      children: [
        {
          text: 'Get a Service by Service Type',
          link: '/guide/ioc/resolving/get-a-service-by-service-type'
        },
        {
          text: 'Get a Service by Service Type and Name',
          link: '/guide/ioc/resolving/get-a-service-by-service-type-and-name'
        },
        {
          text: 'Get all Services by Service Type',
          link: '/guide/ioc/resolving/get-all-services-by-service-type'
        },
        {
          text: 'Try Getting an Optional Service by Service Type',
          link: '/guide/ioc/resolving/try-getting-an-optional-service-by-service-type'
        },
        {
          text: 'Try Getting an Optional Service by Service Type and Name',
          link: '/guide/ioc/resolving/try-getting-an-optional-service-by-service-type-and-name'
        },
        {
          text: 'Auto Resolving Concrete Types',
          link: '/guide/ioc/resolving/requesting-a-concrete-type'
        },
      ]
    },
    {
      text: 'Generic Types',
      link: '/guide/ioc/generics'
    },
    {
      text: 'Decorators, Interceptors, and Activators',
      link: '/guide/ioc/decorators'
    },
    {
      text: 'Lamar Diagnostics',
      link: '/guide/ioc/diagnostics/',
      children: [
        {
          text: 'WhatDoIHave()',
          link: '/guide/ioc/diagnostics/what-do-i-have'
        },
        {
          text: 'Validating Container Configuration',
          link: '/guide/ioc/diagnostics/validating-container-configuration'
        },
        {
          text: 'Environment Tests',
          link: '/guide/ioc/diagnostics/environment-tests'
        },
        {
          text: 'Build Plans',
          link: '/guide/ioc/diagnostics/build-plans'
        },
        {
          text: 'Using the Container Model',
          link: '/guide/ioc/diagnostics/using-the-container-model'
        },
        {
          text: 'Type Scanning Diagnostics',
          link: '/guide/ioc/diagnostics/type-scanning'
        },
      ]
    },
    {
      text: 'Auto Wiring',
      link: '/guide/ioc/auto-wiring'
    },
    {
      text: 'Building Objects with Lambdas',
      link: '/guide/ioc/lambdas'
    },
    {
      text: 'Injecting Services at Runtime',
      link: '/guide/ioc/injecting-at-runtime'
    },
    {
      text: 'Lamar and IDisposable',
      link: '/guide/ioc/disposing'
    },
    {
      text: 'Lazy Resolution',
      link: '/guide/ioc/lazy-resolution'
    },
    {
      text: 'Nested Containers (Per Request/Transaction)',
      link: '/guide/ioc/nested-containers'
    },
    {
      text: 'Setter Injection',
      link: '/guide/ioc/setter-injection'
    },
    {
      text: 'Working with Enumerable Types',
      link: '/guide/ioc/working-with-enumerable-types'
    },
  ]
}

function getCompilationSideBar() {
  return [
    {
      text: 'Compiling code with AssemblyGenerator',
      link: '/guide/compilation/assembly-generator'
    },
    {
      text: 'Generating code with ISourceWriter',
      link: '/guide/compilation/source-writer'
    },
    {
      text: 'The "Frame" Model',
      link: '/guide/compilation/frames/',
      children: [
        {
          text: 'Working with Variables',
          link: '/guide/compilation/frames/variables'
        },
        {
          text: 'Building custom Frames',
          link: '/guide/compilation/frames/frame'
        },
        {
          text: 'Extension methods for Names in code',
          link: '/guide/compilation/frames/extension-methods'
        },
        {
          text: 'Injected Fields',
          link: '/guide/compilation/frames/injected-fields'
        },
      ]
    }
  ]
}
