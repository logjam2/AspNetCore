// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleControllerApplicationModelConvention.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.AspNetCore
{
    using System;
    using System.Reflection;

    using global::Microsoft.AspNetCore.Mvc.ApplicationModels;

    /// <summary>
    /// Uses a single controller type in a webapp (for testing), instead of using the normal process for controller discovery.
    /// </summary>
    public class SingleControllerApplicationModelConvention : IApplicationModelConvention
    {

        private readonly Type _controllerType;

        public SingleControllerApplicationModelConvention(Type controllerType)
        {
            _controllerType = controllerType;
        }

        public void Apply(ApplicationModel application)
        {
            application.Controllers.Clear();
            application.Controllers.Add(new ControllerModel(_controllerType.GetTypeInfo(), _controllerType.GetCustomAttributes(true)));
        }

    }
}
