using System.Numerics;
using Raylib_cs;
using SpixelRenderer;

namespace raylib_rendering.Lighting;

public class DirectionalLight : Light
{
    DirectionalLight(Vector3 positon, Vector3 direction)
    {
        Position = positon;
        Direction = direction;

        Camera = new Camera3D(
            Position,
            Position + Direction,
            new Vector3(0, 1, 0),
            100,
            CameraProjection.CAMERA_ORTHOGRAPHIC
        );
        
        
    }

    private Camera3D Camera;
    private RenderTexture2D depthRenderTexture;
    private Vector3 Position;
    private Vector3 Direction;
    
    public override LightManager.LightData UpdateLight(DrawCallback drawCallback)
    {
        Camera.position = Position;
        Camera.target = Position + Direction;
        
        Raylib.BeginTextureMode(depthRenderTexture);
        Raylib.BeginMode3D(Camera);
        drawCallback();
        Raylib.EndMode3D();
        Raylib.EndTextureMode();

        Texture2D depthTexture = DepthTexture.GetBufferFromRenderTexture(depthRenderTexture);

        return new LightManager.LightData()
        {
            Position = this.Position,
            Direction = this.Direction,
            CastShadows = true,
            Type = LightManager.LightType.Directional,
            CameraData = new LightManager.LightCameraData[1]
            {
                new LightManager.LightCameraData(){
                    CameraPosition = this.Position,
                    DepthTexture = depthTexture,
                    TextureSize = new Vector2(depthTexture.width, depthTexture.height)
                }
            }
            
        };
    }
}