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
    
    public override unsafe LightManager.LightData UpdateLight(DrawCallback drawCallback)
    {
        Camera.position = Position;
        Camera.target = Position + Direction;
        
        Raylib.BeginTextureMode(depthRenderTexture);
        Raylib.ClearBackground(Color.WHITE);
        Raylib.BeginMode3D(Camera);
        drawCallback();
        Raylib.EndMode3D();
        Raylib.EndTextureMode();

        Texture2D depthTexture = DepthTexture.GetBufferFromRenderTexture(depthRenderTexture.depth);

        fixed (Camera3D* cameraPtr = &Camera)
        {

            return new LightManager.LightData()
            {
                Position = this.Position,
                Direction = this.Direction,
                CastShadows = true,
                Type = LightManager.LightType.Directional,
                Index = this.Index,
                
                TypeLoc = this.TypeLoc,
                DirectionLoc = this.DirectionLoc,
                CastShadowsLoc = this.CastShadowsLoc,
                PositionLoc = this.PositionLoc,
                
                CameraData = new LightManager.LightCameraData[1]
                {
                    new LightManager.LightCameraData()
                    {
                        CameraPosition = this.Position,
                        DepthTexture = depthTexture,
                        TextureSize = new Vector2(depthTexture.width, depthTexture.height),
                        VeiwProjectionMatrix = Raylib.GetCameraViewMatrix(cameraPtr) *
                                               Raylib.GetCameraProjectionMatrix(cameraPtr, Camera.fovy)
                    }
                }

            };
        }
    }
}