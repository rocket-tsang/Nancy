﻿namespace Nancy.ViewEngines
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Default implementation on how views are resolved by Nancy.
    /// </summary>
    public class DefaultViewResolver : IViewResolver
    {
        private readonly IEnumerable<Func<string, dynamic, ViewLocationContext, string>> conventions;
        private readonly IViewLocator viewLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewResolver"/> class.
        /// </summary>
        /// <param name="viewLocator">The view locator that should be used to locate views.</param>
        /// <param name="conventions">The conventions that the view resolver should use to figure out where to look for views.</param>
        public DefaultViewResolver(IViewLocator viewLocator, IEnumerable<Func<string, dynamic, ViewLocationContext, string>> conventions)
        {
            if (viewLocator == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultViewResolver with view locator parameter having null value.");
            }

            if (conventions == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultViewResolver with conventions parameter having null value.");
            }

            this.viewLocator = viewLocator;
            this.conventions = conventions;
        }

        /// <summary>
        /// Locates a view based on the provided information.
        /// </summary>
        /// <param name="viewName">The name of the view to locate.</param>
        /// <param name="model">The model that will be used with the view.</param>
        /// <param name="viewLocationContext">A <see cref="ViewLocationContext"/> instance, containing information about the context for which the view is being located.</param>
        /// <returns>A <see cref="ViewLocationResult"/> instance if the view could be found, otherwise <see langword="null"/>.</returns>
        public ViewLocationResult GetViewLocation(string viewName, dynamic model, ViewLocationContext viewLocationContext)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                return null;
            }

            foreach (var convention in conventions)
            {
                var conventionBasedViewName =
                    SafeInvokeConvention(convention, viewName, model, viewLocationContext);

                if (String.IsNullOrEmpty(conventionBasedViewName))
                {
                    continue;
                }

                var locatedView = 
                    this.viewLocator.LocateView(conventionBasedViewName);

                if (locatedView != null)
                {
                    return locatedView;
                }
            }

            return null;
        }

        private static string SafeInvokeConvention(Func<string, object, ViewLocationContext, string> convention, string viewName, dynamic model, ViewLocationContext viewLocationContext)
        {
            try
            {
                return convention.Invoke(viewName, model, viewLocationContext);
            }
            catch
            {
                return null;
            }
        }
    }
}