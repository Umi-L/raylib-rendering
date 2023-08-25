using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;

namespace raylib_rendering.Rendering
{
    public enum ExtendedShaderUniformDataType
    {
        SHADER_UNIFORM_FLOAT = 0,
        SHADER_UNIFORM_VEC2,
        SHADER_UNIFORM_VEC3,
        SHADER_UNIFORM_VEC4,
        SHADER_UNIFORM_INT,
        SHADER_UNIFORM_IVEC2,
        SHADER_UNIFORM_IVEC3,
        SHADER_UNIFORM_IVEC4,
        SHADER_UNIFORM_SAMPLER2D,
        SHADER_UNIFORM_MATRIX,
    }
    
    public struct ShaderProgramUniform
    {
        public ExtendedShaderUniformDataType DataType;
        public dynamic value;

        public int location;
    }

    public class ShaderProgram
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

        public void SetShaderUniform(string name, dynamic data, ExtendedShaderUniformDataType? type = null)
        {
            
            // if uniform does not exist, create it
            if (!uniforms.ContainsKey(name))
            {
                if (!type.HasValue)
                {
                    throw new Exception("ShaderProgram.SetShaderUniform: type must be specified when creating a new uniform");
                }
                
                var newUniform = new ShaderProgramUniform();
                newUniform.location = Raylib.GetShaderLocation(shader, name);
                newUniform.DataType = type.Value;
                newUniform.value = data;
                
                uniforms.Add(name, newUniform);
            }
            else
            {
                // get uniform with name
                uniforms.TryGetValue(name, out var uniform);

                // set values to uniform
                uniform.value = data;

                // set uniform back to dictionary
                uniforms[name] = uniform;
            }
        }
        
        public void UpdateShaderUniforms()
        {
            foreach (var uniform in uniforms)
            {
                // if type is texture use Raylib.SetShaderTexture
                if (uniform.Value.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_SAMPLER2D)
                {
                    Raylib.SetShaderValueTexture(shader, uniform.Value.location, uniform.Value.value);
                } 
                else if (uniform.Value.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_MATRIX)
                {
                    Raylib.SetShaderValueMatrix(shader, uniform.Value.location, uniform.Value.value);
                } 
                else
                {
                    // Console.WriteLine($"uniform {uniform.Key} value set to {uniform.Value.value} with loc {uniform.Value.location}");
                    Raylib.SetShaderValue(shader, uniform.Value.location, uniform.Value.value, (ShaderUniformDataType)uniform.Value.DataType);
                }
            }
        }

        public void AddUniformsImGuiModifiers(string? name = null)
        {
            if (name == null)
            {
                name = "Shader with id " + shader.id + " uniforms";
            }
            
            if (ImGui.TreeNode(name))
            {
                DrawUniformsImGui();
                ImGui.TreePop();
            }
            
            
            // add separator
            ImGui.Separator();
            // gap
            ImGui.Spacing();
        }

        private void DrawUniformsImGui()
        {
            // foreach uniform
            for (int i = 0; i < uniforms.Keys.Count; i++)
            {
                var key = uniforms.Keys.ElementAt(i);
                var uniform = uniforms[uniforms.Keys.ElementAt(i)];
                
                // if type is float, add slider
                if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT)
                {
                    var value = (float)uniform.value;
                    if (ImGui.InputFloat(key, ref value))
                    {
                        // set shader value to new value
                        SetShaderUniform(key, value);
                    }
                }
                else if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC2)
                {
                    var value = (Vector2)uniform.value;
                    if (ImGui.InputFloat2(key, ref value))
                    {
                        // set shader value to new value
                        SetShaderUniform(key, value);
                    }
                }
                else if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC3)
                {
                    var value = (Vector3)uniform.value;
                    if (ImGui.InputFloat3(key, ref value))
                    {
                        // set shader value to new value
                        SetShaderUniform(key, value);
                    }
                }
                else if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC4)
                {
                    var value = (Vector4)uniform.value;
                    if (ImGui.InputFloat4(key, ref value))
                    {
                        // set shader value to new value
                        SetShaderUniform(key, value);
                    }
                }
                else if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_INT)
                {
                    var value = (int)uniform.value;
                    if (ImGui.InputInt(key, ref value))
                    {
                        // set shader value to new value
                        SetShaderUniform(key, value);
                    }
                } else if (uniform.DataType == ExtendedShaderUniformDataType.SHADER_UNIFORM_MATRIX)
                {
                    var value = (Matrix4x4)uniform.value;
                    
                    // display matrix
                    ImGui.Text(key);
                    ImGui.Text($"[{value.M11}, {value.M12}, {value.M13}, {value.M14}]");
                    ImGui.Text($"[{value.M21}, {value.M22}, {value.M23}, {value.M24}]");
                    ImGui.Text($"[{value.M31}, {value.M32}, {value.M33}, {value.M34}]");
                    ImGui.Text($"[{value.M41}, {value.M42}, {value.M43}, {value.M44}]");
                }
            }
        }

        public void Dispose()
        {
            Raylib.UnloadShader(shader);
        }

        
    }
}
