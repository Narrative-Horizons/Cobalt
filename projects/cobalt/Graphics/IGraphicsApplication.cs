using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    /// <summary>
    /// Interface for graphics applications used to instantiate graphics objects.
    /// </summary>
    public interface IGraphicsApplication : IDisposable
    {
        /// <summary>
        /// Creation information for building graphics applications.
        /// </summary>
        public class CreateInfo
        {
            /// <summary>
            /// Builder for <see cref="CreateInfo" />. 
            /// </summary>
            public class Builder : CreateInfo
            {
                /// <summary>
                /// Sets the name of the application.
                /// </summary>
                /// <param name="name">Application Name</param>
                /// <returns><code>this</code></returns>
                public new Builder Name(string name)
                {
                    base.Name = name;
                    return this;
                }

                /// <summary>
                /// Sets whether or not to enable a debug runtime of the graphics application.
                /// </summary>
                /// <param name="debug">Debug runtime enabled</param>
                /// <returns><code>this</code></returns>
                public new Builder Debug(bool debug)
                {
                    base.Debug = debug;
                    return this;
                }

                /// <summary>
                /// Builds a new <see cref="CreateInfo" /> from the contents of the builder.
                /// </summary>
                /// <returns><code>New creation information</code></returns>
                public CreateInfo Build()
                {
                    var info = new CreateInfo();
                    info.Debug = base.Debug;
                    info.Name = base.Name;
                    return info;
                }
            }

            /// <summary>
            /// Name of the application.
            /// </summary>
            public string Name { get; private set; }
            
            /// <summary>
            /// Debug runtime enabled.
            /// </summary>
            public bool Debug { get; private set; }
        }
    }
}
