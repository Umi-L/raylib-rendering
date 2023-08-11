using Raylib_cs;
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

        public static float outlineWidth = 2f;
        public static Vector4 outlineColor = new Vector4(1,0,0,1);
        public static int paperDetail = 10;
        public static Vector2 paperScroll = new Vector2(0,0);
        public static Vector2 paperScale = new Vector2(1,1);
        public static float paperFactor = 1f;
        public static Vector4 paperColor = new Vector4(0, 0, 0, 1);
        
        public static float hueShift = 0f;
        public static float saturationShift = 0.2f;
        public static float valueShift = 0.2f;
        
        public static float ambientLightLevel = 0.7f;

        public static void Main()
        {

            float runningrot = 0;

            Raylib.InitWindow(1200, 800, "Hello World");

            RenderTexture2D rtex = DepthTexture.LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            Assets.Load();
            DepthTexture.Init();
            
            // init RenderSystem
            RenderSystem renderSystem = new RenderSystem(new RenderPass[]
            {
                // new RenderPass(Assets.colourFilterShaderProgram),
                new RenderPass(Assets.outlineShaderProgram),
                // new RenderPass(Assets.paperShaderProgram),
                // new RenderPass(Assets.testShaderProgram),
            });

            // init camera
            var camera = new Camera3D(
                new Vector3(100.0f, 100.0f, 100.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                10.0f,
                CameraProjection.CAMERA_ORTHOGRAPHIC
            );

            var scene = SceneManager.LoadScene("assets/scenes/Scene1.txt");


            // ambient light level
            int ambientLoc = Raylib.GetShaderLocation(Assets.lightingShader, "ambient");
            float[] ambient = new[] { 0.7f, 0.7f, 0.7f, 1.0f };
            Raylib.SetShaderValue(Assets.lightingShader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

            DirectionalLight directionalLight = new DirectionalLight(new Vector3(10, 100, 10), new Vector3(-0.5f,-1,-0.5f));
            
            rlImGui.Setup(true);

            while (!Raylib.WindowShouldClose())
            {

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
                    Raylib.DrawModelEx(Assets.scarecrowModel, new Vector3(-2,0,0), new Vector3(0,1,0), runningrot, Vector3.One, Color.WHITE);

                    scene.Draw(1f);
                },camera,
                delegate
                {
                    ImGui.Text("Outline");
                    if ( ImGui.SliderFloat("outlineWidth", ref outlineWidth, 0.1f, 10f))
                    {
                        Assets.outlineShaderProgram.SetShaderUniform("outlineWidth", outlineWidth);
                    }
                    if (ImGui.ColorEdit4("outlineColor", ref outlineColor))
                    {
                        Assets.outlineShaderProgram.SetShaderUniform("outlineColor", outlineColor);
                    }

                    ImGui.Text("Paper");
                    if (ImGui.SliderInt("paperDetail", ref paperDetail, 0, 20))
                    {
                        Assets.paperShaderProgram.SetShaderUniform("detail", paperDetail);
                    }
                    if (ImGui.SliderFloat2("paperScroll", ref paperScroll, -10000f, 10000f))
                    {
                        Assets.paperShaderProgram.SetShaderUniform("scroll", paperScroll);
                    }
                    if (ImGui.SliderFloat2("paperScale", ref paperScale, 0f, 50f))
                    {
                        Assets.paperShaderProgram.SetShaderUniform("scale", paperScale);
                    }
                    if (ImGui.SliderFloat("paperFactor", ref paperFactor, -10f, 10f))
                    {
                        Assets.paperShaderProgram.SetShaderUniform("factor", paperFactor);
                    }
                    if (ImGui.ColorEdit4("paperColor", ref paperColor))
                    {
                        Assets.paperShaderProgram.SetShaderUniform("paperColor", paperColor);
                    }

                    if (ImGui.SliderFloat("hueShift", ref hueShift, -1f, 1f))
                    {
                        Assets.colourFilterShaderProgram.SetShaderUniform("hueShift", hueShift);
                    }
                    if (ImGui.SliderFloat("saturationShift", ref saturationShift, -1f, 1f))
                    {
                        Assets.colourFilterShaderProgram.SetShaderUniform("saturationShift", saturationShift);
                    }
                    if (ImGui.SliderFloat("lightnessShift", ref valueShift, -1f, 1f))
                    {
                        Assets.colourFilterShaderProgram.SetShaderUniform("lightnessShift", valueShift);
                    }

                    if (ImGui.SliderFloat("ambientLight", ref ambientLightLevel, 0, 1))
                    {
                        ambient = new[] { ambientLightLevel, ambientLightLevel, ambientLightLevel, 1.0f };
                        Raylib.SetShaderValue(Assets.lightingShader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                    }
                    
                    ImGui.Text("SceneInfo");
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