using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;
using SpixelRenderer;

namespace raylib_rendering.Rendering
{
    public class RenderPass
    {
        public ShaderProgram shader;
        public ScreenSizeRenderTexture target;

        private int depthLoc;
        private int normalsLoc;
        private int screenSizeLoc;
        
        private UniformCallback uniformCallback;
        
        public delegate void UniformCallback();

        public RenderPass(ShaderProgram shader, UniformCallback? uniformCallback = null)
        {
            this.shader = shader;
            this.target = new ScreenSizeRenderTexture();
            
            this.depthLoc = Raylib.GetShaderLocation(shader.shader, "depth");
            this.normalsLoc = Raylib.GetShaderLocation(shader.shader, "normals");
            this.screenSizeLoc = Raylib.GetShaderLocation(shader.shader, "screenSize");
            
            this.uniformCallback = uniformCallback ?? delegate { };
        }

        public RenderTexture2D Apply(RenderTexture2D input, Texture2D depth, Texture2D normals)
        {
            Raylib.BeginTextureMode(target.renderTexture);
            {
                Raylib.ClearBackground(Color.WHITE);

                // start shader
                Raylib.BeginShaderMode(shader.shader);
                {
                    // must set shader value inside of shader mode SMH
                    Raylib.SetShaderValueTexture(shader.shader, depthLoc, depth);
                    Raylib.SetShaderValueTexture(shader.shader, normalsLoc, normals);
                    Raylib.SetShaderValue(shader.shader, screenSizeLoc, new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), ShaderUniformDataType.SHADER_UNIFORM_VEC2);

                    shader.UpdateShaderUniforms();
                    
                    uniformCallback();

                    // draw input
                    Raylib.DrawTextureRec(input.texture, new Rectangle(0, 0, input.texture.width, -input.texture.height), Vector2.Zero, Color.WHITE);
                }

                // end shader and texture modes
                Raylib.EndShaderMode();
            }
            Raylib.EndTextureMode();

            return target.renderTexture;
        }

        public void Dispose()
        {
            Raylib.UnloadRenderTexture(target.renderTexture);
        }
    }
}
