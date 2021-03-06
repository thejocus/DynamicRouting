﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicRouting.Kentico.MVC
{
    public static class DynamicRoutingAnalyzer
    {
        private static readonly Dictionary<string, DynamicRouteConfiguration> classNameLookup =
            new Dictionary<string, DynamicRouteConfiguration>();

        static DynamicRoutingAnalyzer()
        {
            var attributes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => !a.FullName.StartsWith("CMS.") && !a.FullName.StartsWith("Kentico."))
                .SelectMany(a => a.GetCustomAttributes<DynamicRoutingAttribute>());

            foreach (var attribute in attributes)
            {
                if (attribute == null)
                {
                    continue;
                }
                foreach (string pageClassName in attribute.PageClassNames)
                {
                    string pageClassNameLookup = pageClassName.ToLowerInvariant();
                    if (classNameLookup.TryGetValue(pageClassNameLookup, out var pair))
                    {
                        throw new Exception(
                            "Duplicate Annotation: " +
                            $"{pair.ControllerName}Controller.{pair.ActionName} already registered for NodeClassName {pageClassNameLookup}. " +
                            $"Cannot be registered for {attribute.ControllerName}.{attribute.ActionMethodName}"
                        );
                    }

                    classNameLookup.Add(pageClassNameLookup, new DynamicRouteConfiguration(
                        controllerName: attribute.ControllerName,
                        actionName: attribute.ActionMethodName,
                        viewName: attribute.ViewName,
                        modelType: attribute.ModelType,
                        routeType: attribute.RouteType
                        ));
                }
            }
        }

        public static bool TryFindMatch(string nodeClassName, out DynamicRouteConfiguration match)
        {
            return classNameLookup.TryGetValue(nodeClassName.ToLowerInvariant(), out match);
        }
    }
}
