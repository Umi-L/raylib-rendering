using System.Numerics;
using Raylib_cs;

namespace raylib_rendering.Lighting;

public class LightManager
{
    public static List<Light> Lights = new List<Light>();
    
    
    
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
        public Matrix4x4 VeiwProjectionMatrix;
        
        public int CameraPositionLoc;
        public int DepthTextureLoc;
        public int TextureSizeLoc;
        public int ViewProjectionMatrixLoc;
    }
    
    public struct LightData
    {
        public LightType Type;
        public Vector3 Position;
        public Vector3 Direction;
        public bool CastShadows;

        public LightCameraData[] CameraData;
        
        public int Index;
        public int TypeLoc;
        public int PositionLoc;
        public int DirectionLoc;
        public int CastShadowsLoc;
    }

    public static void UpdateLightsAndSetShaderValues(Light.DrawCallback drawCallback)
    {
        List<LightData> lightData = new List<LightData>();
        
        foreach (Light light in Lights)
        {
            lightData.Add(light.UpdateLight(drawCallback));
        }
    }

    public static void addLight(Light light)
    {
        Lights.Add(light);
        light.Index = Lights.Count - 1;
    }
}