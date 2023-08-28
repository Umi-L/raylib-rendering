﻿using Raylib_cs;
using rlImGui_cs;
using raylib_rendering;
using raylib_rendering.Rendering;
using SpixelRenderer;
using System.Numerics;
using ImGuiNET;
using raylib_rendering.Lighting;
using Raylights_cs;

namespace raylib_rendering
{
    static class Program
    {
        public static float ambientLightLevel = 0.7f;
        public static Vector2 screenSize = new Vector2(1200, 800);

        public static void Main()
        {

            float runningrot = 0;

            Raylib.InitWindow((int)screenSize.X, (int)screenSize.Y, "Raylib-Rendering");

            // make window resizable
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            
            // make window run on gpu
            // Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_WINDOW_HIGHDPI);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
            
            
            Assets.Load();
            DepthTexture.Init();
            
            // init camera
            var camera = new Camera3D(
                new Vector3(100.0f, 100.0f, 100.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                10.0f,
                CameraProjection.CAMERA_ORTHOGRAPHIC
            );
            
            // init RenderSystem
            RenderSystem renderSystem = new RenderSystem(new RenderPass[]
            {
                new RenderPass(Assets.colourFilterShaderProgram),
                new RenderPass(Assets.outlineShaderProgram),
                new RenderPass(Assets.inlineShaderProgram,
                    delegate
                    {
                        Assets.inlineShaderProgram.SetShaderUniform("viewProjectionMatrix", Utils.GetCameraViewProjectionMatrix(ref camera), ExtendedShaderUniformDataType.SHADER_UNIFORM_MATRIX);
                    }),
                new RenderPass(Assets.paperShaderProgram),
                // new RenderPass(Assets.testShaderProgram),
            });

            var scene = SceneManager.LoadScene("assets/scenes/Scene1.json");

            // ambient light level
            int ambientLoc = Raylib.GetShaderLocation(Assets.lightingShader, "ambient");
            float[] ambient = new[] { 0.7f, 0.7f, 0.7f, 1.0f };
            Raylib.SetShaderValue(Assets.lightingShader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

            DirectionalLight directionalLight = new DirectionalLight(new Vector3(-100, 100, -100), new Vector3(0,0,0), 120f);
            
            rlImGui.Setup(true);

            while (!Raylib.WindowShouldClose())
            {
                
                // check for window resize
                if (Raylib.IsWindowResized())
                {
                    ResizeManager.Invoke(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                }
                
                // update camera
                //Raylib.UpdateCamera(ref camera, CameraMode.CAMERA_THIRD_PERSON);
                CustomCamera.UpdateCamera(ref camera);

                runningrot += 0.1f;

                unsafe
                {
                    Raylib.SetShaderValue(Assets.normalShader, Assets.normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position.X, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                    Raylib.SetShaderValue(Assets.lightingShader, Assets.normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position.X, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                }

                Assets.paperShaderProgram.SetShaderUniform("scroll", new Vector2(camera.position.X, camera.position.Y)*10);

                renderSystem.Draw(delegate
                {
                    RewrittenFunctions.DrawModelEx(Assets.scarecrowModel, new Vector3(0,0,0), new Vector3(0,1,0), runningrot, Vector3.One, Color.WHITE);
                    
                    scene.Draw(1f);
                },camera,
                delegate
                {
                    // get fps
                    float fps = Raylib.GetFPS();
                    ImGui.Text($"fps: {fps}");
                    
                    ImGui.Spacing();
                    
                    Assets.outlineShaderProgram.AddUniformsImGuiModifiers("outline");

                    Assets.paperShaderProgram.AddUniformsImGuiModifiers("paper");
                    
                    Assets.colourFilterShaderProgram.AddUniformsImGuiModifiers("colour filter");
                    
                    Assets.displacementShaderProgram.AddUniformsImGuiModifiers("displacement");


                    if (ImGui.TreeNode("lighting"))
                    {
                        if (ImGui.SliderFloat("ambientLight", ref ambientLightLevel, 0, 1))
                        {
                            ambient = new[] { ambientLightLevel, ambientLightLevel, ambientLightLevel, 1.0f };
                            Raylib.SetShaderValue(Assets.lightingShader, ambientLoc, ambient,
                                ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                        }
                        ImGui.TreePop();
                    }
                    ImGui.Separator();
                    ImGui.Spacing();
                    
                    Assets.inlineShaderProgram.AddUniformsImGuiModifiers("inline");
                    
                    scene.DebugInfo();
                }
                );
            }

            rlImGui.Shutdown();

            Assets.Unload();

            Raylib.CloseWindow();
        }
    }
}