// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace BlazorRouter
{
    internal class RouteEntry
    {
        public RouteEntry(RouteTemplate template, RenderFragment fragment)
        {
            Template = template;
            Fragment = fragment;
        }

        public RouteTemplate Template { get; }
        public RenderFragment Fragment { get; }

        internal void Match(RouteContext context)
        {
            // Parameters will be lazily initialized.
            IDictionary<string, object> parameters = null;

            // Empty template match all routes
            if (string.IsNullOrEmpty(Template.TemplateText))
            {
                context.Parameters = GetParameters();
                context.Fragment = Fragment;
                context.TemplateText = Template.TemplateText;
                return;
            }

            if (Template.Segments.Length != context.Segments.Length)
            {
                return;
            }

            for (int i = 0; i < Template.Segments.Length; i++)
            {
                var segment = Template.Segments[i];
                var pathSegment = context.Segments[i];
                if (!segment.Match(pathSegment, out var matchedParameterValue))
                {
                    return;
                }
                else
                {
                    context.Fragment = Fragment;
                    context.TemplateText = Template.TemplateText;
                    if (segment.IsParameter)
                    {
                        GetParameters()[segment.Value] = matchedParameterValue;
                    }
                }
            }

            context.Parameters = parameters;
            context.Fragment = Fragment;
            context.TemplateText = Template.TemplateText;

            IDictionary<string, object> GetParameters()
            {
                if (parameters == null)
                {
                    parameters = new Dictionary<string, object>();
                }

                return parameters;
            }
        }
    }
}
