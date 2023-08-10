namespace raylib_rendering.Lighting;

public abstract class Light
{
    public delegate void DrawCallback();
    public abstract LightManager.LightData UpdateLight(DrawCallback drawCallback);
}