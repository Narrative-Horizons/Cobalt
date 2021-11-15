﻿using System;
using System.Collections.Generic;

namespace Cobalt.Graphics
{
    public struct PassShaderInfo
    {
        public string vertexModulePath;
        public string fragmentModulePath;
        public string geometryModulePath;
        public string tesselationEvalModulePath;
        public string tesselationControlModulePath;
        public string computeModulePath;
    }

    public class Pass
    {
        public Dictionary<string, Tuple<Shader, PassShaderInfo>> Shaders { get; set; } =
            new Dictionary<string, Tuple<Shader, PassShaderInfo>>();

        public enum PassType : uint
        {
            Compute,
            Graphics
        }

        public virtual PassType GetPassType()
        {
            throw new NotImplementedException();
        }

        public Shader AddShader(string name, PassShaderInfo shaderInfo)
        {
            Shader shader = new Shader();
            Shaders.Add(name, new Tuple<Shader, PassShaderInfo>(shader, shaderInfo));

            return shader;
        }
    }
}