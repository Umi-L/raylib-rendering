using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace raylib_rendering.Rendering
{
    struct ShaderProgramUniform
    {
        public ShaderUniformDataType DataType;
        public dynamic value;

        public int location;
    }

    internal class ShaderProgram
    {
        public Shader shader;
        public Dictionary<string, ShaderProgramUniform> uniforms;
        
        public ShaderProgram(Shader shader, Dictionary<string, ShaderProgramUniform> uniforms) {
            this.shader = shader;
            this.uniforms = uniforms;

            foreach (var _uniform in uniforms)
            {
                uniforms.TryGetValue(_uniform.Key, out var uniform);

                uniform.location = Raylib.GetShaderLocation(shader, _uniform.Key);

                uniforms[_uniform.Key] = uniform;
            }
        }

        public void SetShaderUniform(string name, dynamic data)
        {
            // get uniform with name
            uniforms.TryGetValue(name, out var uniform);

            // set values to uniform
            uniform.value = data;

            // set uniform back to dictionary
            uniforms[name] = uniform;
        }

        public void UpdateShaderUniforms()
        {
            foreach (var uniform in uniforms)
            {
                // if type is texture use Raylib.SetShaderTexture
                if (uniform.Value.DataType == ShaderUniformDataType.SHADER_UNIFORM_SAMPLER2D)
                {
                    Raylib.SetShaderValueTexture(shader, uniform.Value.location, uniform.Value.value);
                }
                else
                {
                    // Console.WriteLine($"uniform {uniform.Key} value set to {uniform.Value.value} with loc {uniform.Value.location}");
                    Raylib.SetShaderValue(shader, uniform.Value.location, uniform.Value.value, uniform.Value.DataType);
                }
            }
        }

        
    }
}
