using System;
using System.Collections.Generic;
using System.Text;

namespace Cobalt.Graphics
{
    public class Material
    {
        internal Dictionary<string, object> _variables = new Dictionary<string, object>();

        public Material(Shader shader)
        {

        }

        public void Set(string name, object value)
        {
            if(!_variables.ContainsKey(name))
            {
                _variables.Add(name, value);
            }
            else
            {
                _variables[name] = value;
            }
        }
    }
}
