using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using raylib_rendering.Lighting;
using rlImGui_cs;
using SpixelRenderer;

namespace raylib_rendering.Rendering
{
    internal class RenderSystem
    {
        public List<RenderPass> passes;
        public RenderTexture2D target;
        RenderTexture2D normalTexture;

        public RenderSystem(RenderPass[] renderPasses)
        {
            passes = new List<RenderPass>(renderPasses);
            target = DepthTexture.LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            normalTexture = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        }

        public void AddPass(RenderPass pass)
        {
            passes.Add(pass);
        }

        public delegate void DrawCallback();
        public delegate void ImGuiCallback();

        // function draw that takes a callback
        public void Draw(DrawCallback callback, Camera3D mainCamera, ImGuiCallback imGuiCallback)
        {
            DrawCallback callbackWithCam = delegate
            {
                Raylib.BeginMode3D(mainCamera);
                callback();
                Raylib.EndMode3D();
            };
            
            Assets.SetModelsShader(Assets.defaultShader);

            Raylib.BeginTextureMode(this.target);
                Raylib.ClearBackground(Color.WHITE);

                callbackWithCam();
            Raylib.EndTextureMode();

            Texture2D depthTexture = DepthTexture.GetBufferFromRenderTexture(this.target.depth);

            // apply normalShader
            Assets.SetModelsShader(Assets.normalShader);

            // get normal buffer
            Raylib.BeginTextureMode(normalTexture);
            {
                Raylib.ClearBackground(Color.WHITE);
                
                callbackWithCam();
            }
            Raylib.EndTextureMode();

            // reaply default
            Assets.SetModelsShader(Assets.defaultShader);



            // draw passes
            RenderTexture2D currentTarget = this.target;

            // for each pass
            foreach (RenderPass pass in passes)
            {
                currentTarget = pass.Apply(currentTarget, depthTexture, normalTexture.texture);
            }

            // draw result
            Raylib.BeginDrawing();

            rlImGui.Begin();

            imGuiCallback();

            float ratio = ImGui.GetWindowWidth() / depthTexture.width;

            ImGui.Text("buffers");
            rlImGui.ImageRect(depthTexture, (int)ImGui.GetWindowWidth(), (int)(depthTexture.height*ratio), new Rectangle(0, depthTexture.height, depthTexture.width, -depthTexture.height));
            rlImGui.ImageRect(normalTexture.texture, (int)ImGui.GetWindowWidth(), (int)(normalTexture.texture.height * ratio), new Rectangle(0, normalTexture.texture.height, normalTexture.texture.width, -normalTexture.texture.height));

            Raylib.ClearBackground(Color.WHITE);

            Raylib.DrawTextureRec(currentTarget.texture, new Rectangle(0, 0, currentTarget.texture.width, -currentTarget.texture.height), Vector2.Zero, Color.WHITE);

            rlImGui.End();

            Raylib.EndDrawing();
        }

        public void Dispose()
        {
            Raylib.UnloadRenderTexture(target);

            foreach (RenderPass pass in passes)
            {
                pass.Dispose();
            }
        }
    }
}
