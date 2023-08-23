using System.Numerics;
using Raylib_cs;
using raylib_rendering;

namespace SpixelRenderer;

public static class DepthTexture
{
    private static RenderTexture2D depthRenderTexture;

    public static void Init()
    {
        depthRenderTexture = LoadRenderTextureDepthTex(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }
    
    public static unsafe RenderTexture2D LoadRenderTextureDepthTex(int width, int height)
    {
        RenderTexture2D target = new RenderTexture2D();

        target.id = Rlgl.rlLoadFramebuffer(width, height);   // Load an empty framebuffer

        if (target.id > 0)
        {
            Rlgl.rlEnableFramebuffer(target.id);

            // Create color texture (default to RGBA)
            target.texture.id = Rlgl.rlLoadTexture(null, width, height, PixelFormat.PIXELFORMAT_UNCOMPRESSED_R32, 1);
            target.texture.width = width;
            target.texture.height = height;
            target.texture.format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R32;
            target.texture.mipmaps = 1;

            // Create depth texture buffer (instead of raylib default renderbuffer)
            target.depth.id = Rlgl.rlLoadTextureDepth(width, height, false);
            target.depth.width = width;
            target.depth.height = height;
            target.depth.format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R32;       //DEPTH_COMPONENT_24BIT? Original: 19
            target.depth.mipmaps = 1;

            // Attach color texture and depth texture to FBO
            Rlgl.rlFramebufferAttach(target.id, target.texture.id, FramebufferAttachType.RL_ATTACHMENT_COLOR_CHANNEL0, FramebufferAttachTextureType.RL_ATTACHMENT_TEXTURE2D, 0);
            Rlgl.rlFramebufferAttach(target.id, target.depth.id, FramebufferAttachType.RL_ATTACHMENT_DEPTH, FramebufferAttachTextureType.RL_ATTACHMENT_TEXTURE2D, 0);

            // Check if fbo is complete with attachments (valid)
            if (Rlgl.rlFramebufferComplete(target.id)) Console.WriteLine("FBO: [ID {0}] Framebuffer object created successfully", target.id);
            else Console.Error.WriteLine("FBO: [ID {0}] Framebuffer object could not be created", target.id);

            Rlgl.rlDisableFramebuffer();
        }
        else Console.Error.WriteLine("FBO: Failed to create FBO");


        return target;
    }

    // Unload render texture from GPU memory (VRAM)
    public static void UnloadRenderTextureDepthTex(RenderTexture2D target)
    {
        if (target.id > 0)
        {
            // Color texture attached to FBO is deleted
            Rlgl.rlUnloadTexture(target.texture.id);
            Rlgl.rlUnloadTexture(target.depth.id);

            // NOTE: Depth texture is automatically
            // queried and deleted before deleting framebuffer
            Rlgl.rlUnloadFramebuffer(target.id);
        }
    }

    public static Texture2D GetBufferFromRenderTexture(Texture2D depth)
    {
        // get depth buffer
        Raylib.BeginTextureMode(depthRenderTexture);
        {
            Raylib.ClearBackground(Color.BLACK);

            Raylib.BeginShaderMode(Assets.depthShader);
            {
                Raylib.DrawTextureRec(depth, new Rectangle(0, 0, depth.width, -depth.height), Vector2.Zero, Color.WHITE);
            }
            Raylib.EndShaderMode();
        }
        Raylib.EndTextureMode();

        return depthRenderTexture.texture;
    }
}