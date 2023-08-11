using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using raylib_rendering.Debug;
using raylib_rendering.Rendering;
using rlImGui_cs;

namespace raylib_rendering.Lighting;

public class LightManager
{
    public static List<Light> Lights = new List<Light>();
    private static Texture2D ThrowawayTexture = Raylib.LoadTexture("assets/textures/throwaway.png");

    public static int MaxLights = 4;
    public static int MaxLightCameras = 4;
    
    public static  int LightsCountLoc = -1;
    

    public enum LightType
    {
        Directional = 0,
        Point = 1
    }
    
    public struct LightCameraData
    {
        public Vector3 CameraPosition;
        public Texture2D DepthTexture;
        public Vector2 TextureSize;
        public Matrix4x4 ViewProjectionMatrix;
        
        public int CameraPositionLoc;
        public int TextureSizeLoc;
        public int ViewProjectionMatrixLoc;
    }
    
    public struct LightData
    {
        public LightType Type;
        public Vector3 Position;
        public Vector3 Direction;
        public bool CastShadows;

        public int CameraDataCount;
        public LightCameraData[] CameraData;
        
        public int TypeLoc;
        public int PositionLoc;
        public int DirectionLoc;
        public int CastShadowsLoc;
        public int CameraDataCountLoc;
        public int[] DepthTextureLocs;
    }

    public static void UpdateLightsAndSetShaderValues(RenderSystem.DrawCallback drawCallback)
    {

        if (LightsCountLoc == -1)
        {
            LightsCountLoc = Raylib.GetShaderLocation(Assets.lightingShader, "lightsCount");
        }
        
        List<LightData> lightData = new List<LightData>();
        
        foreach (Light light in Lights)
        {
            lightData.Add(light.UpdateLight(drawCallback));
        }

        Raylib.SetShaderValue(Assets.lightingShader, LightsCountLoc, lightData.Count, ShaderUniformDataType.SHADER_UNIFORM_INT);
        foreach (LightData data in lightData)
        {
            SetLightShaderValue(data);
        }
    }

    public static void SetLightShaderValue(LightData lightData)
    {
        Raylib.SetShaderValue(Assets.lightingShader, lightData.TypeLoc, (int)lightData.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);
        Raylib.SetShaderValue(Assets.lightingShader, lightData.PositionLoc, lightData.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        Raylib.SetShaderValue(Assets.lightingShader, lightData.DirectionLoc, lightData.Direction, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        Raylib.SetShaderValue(Assets.lightingShader, lightData.CastShadowsLoc, lightData.CastShadows, ShaderUniformDataType.SHADER_UNIFORM_INT);
        Raylib.SetShaderValue(Assets.lightingShader, lightData.CameraDataCountLoc, lightData.CameraDataCount, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        for (int i = 0; i < lightData.CameraData.Length; i++)
        {
            LightCameraData cameraData = lightData.CameraData[i];
            
            Raylib.SetShaderValue(Assets.lightingShader, cameraData.CameraPositionLoc, cameraData.CameraPosition, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
            Raylib.SetShaderValue(Assets.lightingShader, cameraData.TextureSizeLoc, cameraData.TextureSize, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            Raylib.SetShaderValueMatrix(Assets.lightingShader, cameraData.ViewProjectionMatrixLoc, cameraData.ViewProjectionMatrix);
            
            Raylib.SetShaderValueTexture(Assets.lightingShader, lightData.DepthTextureLocs[i], cameraData.DepthTexture);
            
            DebugDraw.AddImGuiCallback(delegate
            {
                float ratio = ImGui.GetWindowWidth() / cameraData.DepthTexture.width;

                ImGui.Text($"Light {lightData.Type} {lightData.Position} {lightData.Direction} {lightData.CastShadows} {lightData.CameraDataCount} Width: {cameraData.DepthTexture.width} Height: {cameraData.DepthTexture.height}");
                rlImGui.ImageRect(cameraData.DepthTexture, (int)ImGui.GetWindowWidth(), (int)(cameraData.DepthTexture.height*ratio), new Rectangle(0, cameraData.DepthTexture.height, cameraData.DepthTexture.width, -cameraData.DepthTexture.height));
            });
        }
    }

    public static void addLight(Light light)
    {
        Lights.Add(light);
        light.SetIndex(Lights.Count - 1);
    }
}