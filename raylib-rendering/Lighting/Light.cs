using Raylib_cs;

namespace raylib_rendering.Lighting;

public abstract class Light
{
    public int Index;
    internal int TypeLoc;
    internal int PositionLoc;
    internal int DirectionLoc;
    internal int CastShadowsLoc;
    
    public delegate void DrawCallback();
    public abstract unsafe LightManager.LightData UpdateLight(DrawCallback drawCallback);

    public void SetIndex(int index)
    {
        this.Index = index;
        
        TypeLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].type");
        PositionLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].position");
        DirectionLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].direction");
        CastShadowsLoc = Raylib.GetShaderLocation(Assets.lightingShader, $"lights[{index}].castShadows");
    }
}