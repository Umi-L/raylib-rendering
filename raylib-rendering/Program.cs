using Raylib_cs;
using rlImGui_cs;
using raylib_rendering;
using raylib_rendering.Rendering;
using SpixelRenderer;
using System.Numerics;
using ImGuiNET;
using Raylights_cs;

namespace HelloWorld
{
    static class Program
    {

        public static float outlineWidth = 2f;
        public static Vector4 outlineColor = new Vector4(1,0,0,1);
        public static int paperDetail = 10;
        public static Vector2 paperScroll = new Vector2(0,0);
        public static Vector2 paperScale = new Vector2(1,1);
        public static float paperFactor = 1f;
        public static Vector4 paperColor = new Vector4(0,0,0,1);

        public static void Main()
        {

            float runningrot = 0;

            Raylib.InitWindow(1200, 800, "Hello World");

            RenderTexture2D rtex = DepthTexture.LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            Assets.Load();

            // init RenderSystem
            RenderSystem renderSystem = new RenderSystem(new RenderPass[]
            {
                //new RenderPass(Assets.testShader),
                new RenderPass(Assets.outlineShaderProgram),
                new RenderPass(Assets.paperShaderProgram),
            });

            // init camera
            var camera = new Camera3D(
                new Vector3(50.0f, 50.0f, 50.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                10.0f,
                CameraProjection.CAMERA_PERSPECTIVE
            );

            var scene = SceneManager.LoadScene("assets/scenes/testScene.txt");

            // Using 4 point lights: Color.gold, Color.red, Color.green and Color.blue
            Light[] lights = new Light[4];
            lights[0] = Rlights.CreateLight(
                0,
                LightType.Directorional,
                new Vector3(1, 1, 0),
                Vector3.Zero,
                new Color(255,255,255, 255),
                Assets.lightingShader
            );

            lights[0].enabled = true;

            // ambient light level
            int ambientLoc = Raylib.GetShaderLocation(Assets.lightingShader, "ambient");
            float[] ambient = new[] { 0.7f, 0.7f, 0.7f, 1.0f };
            Raylib.SetShaderValue(Assets.lightingShader, ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

            rlImGui.Setup(true);

            while (!Raylib.WindowShouldClose())
            {

                // update camera
                Raylib.UpdateCamera(ref camera, CameraMode.CAMERA_THIRD_PERSON);

                runningrot += 0.1f;

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Y))
                {
                    lights[0].enabled = !lights[0].enabled;
                }

                // Update light values (actually, only enable/disable them)
                Rlights.UpdateLightValues(Assets.lightingShader, lights[0]);

                unsafe
                {
                    Raylib.SetShaderValue(Assets.normalShader, Assets.normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position.X, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                    Raylib.SetShaderValue(Assets.lightingShader, Assets.normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position.X, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                }

                Assets.paperShaderProgram.SetShaderUniform("scroll", new Vector2(camera.position.X, camera.position.Y)*10);

                renderSystem.Draw(delegate ()
                {
                    Raylib.BeginMode3D(camera);
                    {
                        Raylib.DrawModelEx(Assets.scarecrowModel, new Vector3(-2,0,0), new Vector3(0,1,0), runningrot, Vector3.One, Color.WHITE);

                        scene.Draw(1f);
                    }
                    Raylib.EndMode3D();
                },
                
                delegate ()
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
                }
                );


            }

            rlImGui.Shutdown();

            Assets.Unload();

            Raylib.CloseWindow();
        }
    }
}