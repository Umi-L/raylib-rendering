using System.Numerics;
using Raylib_cs;

namespace raylib_rendering.Lighting;

public class LightManager
{
    public static List<Light> lights;
    
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
    }
    
    public struct LightData
    {
        public LightType Type;
        public Vector3 Position;
        public Vector3 Direction;
        public bool CastShadows;

        public LightCameraData[] CameraData;
    }

    public static void UpdateLightsAndSetShaderValues(Light.DrawCallback drawCallback)
    {
        List<LightData> lightData = new List<LightData>();
        
        foreach (Light light in lights)
        {
            lightData.Add(light.UpdateLight(drawCallback));
        }
        
        
    }

    public static void addLight(Light light)
    {
        lights.Add(light);
    }
}