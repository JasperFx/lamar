﻿using Lamar.IoC.Instances;
using JasperFx.CodeGeneration.Model;

namespace Lamar.IoC.Frames
{
    public static class VariableExtensions
    {
        public static bool RefersTo(this Variable variable, Instance instance)
        {
            return instance == (variable as IServiceVariable)?.Instance;
        }
    }
}