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
    internal class RenderPass
    {
        public ShaderProgram shader;
        public RenderTexture2D target;

        private int depthLoc;
        private int normalsLoc;
        private int screenSizeLoc;

        public RenderPass(ShaderProgram shader)
        {
            this.shader = shader;
            this.target = DepthTexture.LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            
            this.depthLoc = Raylib.GetShaderLocation(shader.shader, "depth");
            this.normalsLoc = Raylib.GetShaderLocation(shader.shader, "normals");
            this.screenSizeLoc = Raylib.GetShaderLocation(shader.shader, "screenSize");
        }

        public RenderTexture2D Apply(RenderTexture2D input, Texture2D depth, Texture2D normals)
        {
            Raylib.BeginTextureMode(target);
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

                    // draw input
                    Raylib.DrawTextureRec(input.texture, new Rectangle(0, 0, input.texture.width, -input.texture.height), Vector2.Zero, Color.WHITE);
                }

                // end shader and texture modes
                Raylib.EndShaderMode();
            }
            Raylib.EndTextureMode();

            return target;
        }

        public void Dispose()
        {
            Raylib.UnloadRenderTexture(target);
        }
    }
}
