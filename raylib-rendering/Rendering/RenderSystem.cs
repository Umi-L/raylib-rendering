using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using raylib_rendering.Debug;
using raylib_rendering.Lighting;
using rlImGui_cs;
using SpixelRenderer;

namespace raylib_rendering.Rendering
{
    
    enum CullMode
    {
        CULL_FRONT = 0,
        CULL_BACK = 1
    }
    
    public class RenderSystem
    {
        public List<RenderPass> passes;
        public ScreenSizeRenderTexture target;
        public ScreenSizeRenderTexture depthTarget;
        public ScreenSizeRenderTexture displacementTarget;
        ScreenSizeRenderTexture normalTexture;

        public RenderSystem(RenderPass[] renderPasses)
        {
            passes = new List<RenderPass>(renderPasses);
            target = new ScreenSizeRenderTexture();
            depthTarget = new ScreenSizeRenderTexture(true);
            normalTexture = new ScreenSizeRenderTexture();
            displacementTarget = new ScreenSizeRenderTexture();
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
            LightManager.UpdateLights(callback);
            
            DrawCallback callbackWithCam = delegate
            {
                Raylib.BeginMode3D(mainCamera);
                callback();
                Raylib.EndMode3D();
            };
            
            DrawCallback callbackWithCamAndLights = delegate
            {
                LightManager.SetShaderValues();
                Raylib.BeginMode3D(mainCamera);
                Assets.SetModelsShader(Assets.lightingShader);
                callback();
                Raylib.EndMode3D();
            };

            Raylib.BeginTextureMode(displacementTarget.renderTexture);
                
                    Raylib.ClearBackground(Color.WHITE);
                    Raylib.BeginShaderMode(Assets.displacementShader);
                        // draw fullscreen quad
                        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Color.BLACK);
                    Raylib.EndShaderMode();
                        
            Raylib.EndTextureMode();
            
            
            
            Raylib.BeginTextureMode(this.target.renderTexture);
            
                // Update light shader value
                Raylib.ClearBackground(Color.WHITE);
                

                callbackWithCamAndLights();
            Raylib.EndTextureMode();
            
            // get depth buffer
            Raylib.BeginTextureMode(this.depthTarget.renderTexture);
            
            // Update light shader value
            Raylib.ClearBackground(Color.WHITE);
                

            callbackWithCam();
            Raylib.EndTextureMode();

            

            Texture2D depthTexture = DepthTexture.GetBufferFromRenderTexture(this.depthTarget.renderTexture.depth);

            // apply normalShader
            Assets.SetModelsShader(Assets.normalShader);

            // get normal buffer
            Raylib.BeginTextureMode(normalTexture.renderTexture);
            {
                Raylib.ClearBackground(Color.WHITE);
                
                callbackWithCam();
            }
            Raylib.EndTextureMode();

            // reapply default
            // Assets.SetModelsShader(Assets.lightingShader);

            // draw passes
            RenderTexture2D currentTarget = this.target.renderTexture;

            // for each pass
            foreach (RenderPass pass in passes)
            {
                currentTarget = pass.Apply(currentTarget, depthTexture, normalTexture.renderTexture.texture, displacementTarget.renderTexture.texture);
            }

            // draw result
            Raylib.BeginDrawing();

            rlImGui.Begin();

            imGuiCallback();
            
            DebugDraw.Draw();

            float ratio = ImGui.GetWindowWidth() / depthTexture.width;

            if (ImGui.TreeNode("buffers"))
            {
                rlImGui.ImageRect(depthTexture, (int)ImGui.GetWindowWidth(), (int)(depthTexture.height * ratio),
                    new Rectangle(0, depthTexture.height, depthTexture.width, -depthTexture.height));
                rlImGui.ImageRect(normalTexture.renderTexture.texture, (int)ImGui.GetWindowWidth(),
                    (int)(normalTexture.renderTexture.texture.height * ratio),
                    new Rectangle(0, normalTexture.renderTexture.texture.height, normalTexture.renderTexture.texture.width,
                        -normalTexture.renderTexture.texture.height));  
                rlImGui.ImageRect(displacementTarget.renderTexture.texture, (int)ImGui.GetWindowWidth(),
                    (int)(displacementTarget.renderTexture.texture.height * ratio),
                    new Rectangle(0, displacementTarget.renderTexture.texture.height, displacementTarget.renderTexture.texture.width,
                        -displacementTarget.renderTexture.texture.height));
            }

            Raylib.ClearBackground(Color.WHITE);

            Raylib.DrawTextureRec(currentTarget.texture, new Rectangle(0, 0, currentTarget.texture.width, -currentTarget.texture.height), Vector2.Zero, Color.WHITE);
            
            rlImGui.End();

            Raylib.EndDrawing();
        }

        public void Dispose()
        {
            Raylib.UnloadRenderTexture(target.renderTexture);

            foreach (RenderPass pass in passes)
            {
                pass.Dispose();
            }
        }
    }
}
