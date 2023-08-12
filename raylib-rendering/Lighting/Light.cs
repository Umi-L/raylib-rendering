using Raylib_cs;
using raylib_rendering.Rendering;

namespace raylib_rendering.Lighting;

public abstract class Light
{
    public int Index;
    internal int TypeLoc;
    internal int PositionLoc;
    internal int DirectionLoc;
    internal int CastShadowsLoc;
    public int CameraDataCountLoc;
    public int[] depthTextureLocs;

    internal List<LightCameraLocData> LightCameraLocs = new List<LightCameraLocData>();

    internal struct LightCameraLocData
    {
        public int CameraPositionLoc;
        public int TextureSizeLoc;
        public int ViewProjectionMatrixLoc;
    }
    
    public abstract unsafe LightManager.LightData UpdateLight(RenderSystem.DrawCallback drawCallback);

    public void SetIndex(int index)
    {
        this.Index = index;
        
        TypeLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].type");
        PositionLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].position");
        DirectionLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].direction");
        CastShadowsLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].castShadows");
        CameraDataCountLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].cameraDataCount");
    }
    
    public void InitCameraLocs(int count)
    {
        for (int i = 0; i < count; i++)
        {

            var lightCameraLocData = new LightCameraLocData();

            lightCameraLocData.CameraPositionLoc =
                Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{this.Index}].cameraData[{i}].cameraPosition");
            
            lightCameraLocData.TextureSizeLoc =
                Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{this.Index}].cameraData[{i}].textureSize");
            
            lightCameraLocData.ViewProjectionMatrixLoc =
                Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{this.Index}].cameraData[{i}].viewProjectionMatrix");

            LightCameraLocs.Add(lightCameraLocData);
        }
        
        depthTextureLocs = new int[LightManager.MaxLightCameras];
        
        for (int i = 0; i < count; i++)
        {
            depthTextureLocs[i] = Raylib.GetShaderLocation(Assets.lightingShader, $"depthTextures[{LightManager.Lights.IndexOf(this) * LightManager.MaxLightCameras + i}]");
        }

    }
}