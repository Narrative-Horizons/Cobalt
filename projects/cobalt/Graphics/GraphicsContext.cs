﻿using System;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public class GraphicsContext
    {
        /// <summary>
        /// Supported graphics APIs.
        /// </summary>
        public enum API
        {
            /// <summary>
            /// Load an OpenGL 4 context
            /// </summary>
            OpenGL_4
        }

        public API ContextAPI { get; private set; }

        private static GraphicsContext _context;
        private readonly List<IGraphicsApplication> _applications = new List<IGraphicsApplication>();

        private GraphicsContext(API api)
        {
            
        }

        public static GraphicsContext GetInstance()
        {
            return GetInstance(API.OpenGL_4);
        }

        public static GraphicsContext GetInstance(API api)
        {
            if (_context == null)
            {
                _context = new GraphicsContext(api);
            }
            else if (_context.ContextAPI != api)
            {
                _context._applications.ForEach(application =>
                {
                    application.Dispose();
                });

                _context = new GraphicsContext(api);
            }

            return _context;
        }

        public IGraphicsApplication CreateApplication(IGraphicsApplication.CreateInfo info)
        {
            switch (ContextAPI)
            {
                case API.OpenGL_4:
                    {
                        IGraphicsApplication application = new GL.GraphicsApplication(info);
                        _applications.Add(application);
                        return application;
                    }
            }

            throw new InvalidOperationException("Selected API does not support function.");
        }
    }
}
