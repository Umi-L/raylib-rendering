using Raylib_cs;
using SpixelRenderer;

namespace raylib_rendering.Rendering;

public class ScreenSizeRenderTexture
{
    
    public RenderTexture2D renderTexture;
    
    public ScreenSizeRenderTexture(bool depth = false)
    {

        if (depth)
        {
            renderTexture = DepthTexture.LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        }
        else
        {
            renderTexture = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        }
        
        // add resize listener
        ResizeManager.AddListener((width, height) =>
        {
            Raylib.UnloadRenderTexture(renderTexture);
            if (depth)
            {
                renderTexture = DepthTexture.LoadRenderTextureDepthTex(width, height);
            }
            else
            {
                renderTexture = Raylib.LoadRenderTexture(width, height);
            }
        });
    }
}