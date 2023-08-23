using Raylib_cs;

namespace raylib_rendering.Rendering;

public static class TextureBindingManager
{
    public static List<RewrittenFunctions.TextureBindingInfo> TextureBindings = new List<RewrittenFunctions.TextureBindingInfo>();
    
    public static void BindTexture(uint textureId, int shaderLoc)
    {
        var textureBindingInfo = new RewrittenFunctions.TextureBindingInfo();
        textureBindingInfo.Id = textureId;
        textureBindingInfo.Loc = shaderLoc;
        
        TextureBindings.Add(textureBindingInfo);
    }

    public static void Clear()
    {
        TextureBindings.Clear();
    }
}