using System.Numerics;
using Raylib_cs;
using raylib_rendering.Rendering;
using SpixelRenderer;

namespace raylib_rendering.Lighting;

public class DirectionalLight : Light
{
    public DirectionalLight(Vector3 positon, Vector3 direction)
    {
        Position = positon;
        Direction = direction;

        Camera = new Camera3D(
            Position,
            Position + Direction,
            new Vector3(0, 1, 0),
            200,
            CameraProjection.CAMERA_ORTHOGRAPHIC
        );
        
        InitCameraLocs(1);
        
        depthRenderTexture = DepthTexture.LoadRenderTextureDepthTex(1024, 1024);
        secondDepthRenderTexture = DepthTexture.LoadRenderTextureDepthTex(1024, 1024);
        
        LightManager.addLight(this);
    }

    private Camera3D Camera;
    private RenderTexture2D depthRenderTexture;
    private RenderTexture2D secondDepthRenderTexture;
    private Vector3 Position;
    private Vector3 Direction;
    
    public override unsafe LightManager.LightData UpdateLight(RenderSystem.DrawCallback drawCallback)
    {
        Camera.position = Position;
        Camera.target = Position + Direction;
        
        Raylib.BeginTextureMode(depthRenderTexture);
        {
            Raylib.ClearBackground(Color.WHITE);
            
            Raylib.BeginMode3D(Camera);
            {
                drawCallback();
            }
            Raylib.EndMode3D();
        }
        Raylib.EndTextureMode();

        // get depth buffer
        Raylib.BeginTextureMode(secondDepthRenderTexture);
        {
            Raylib.ClearBackground(Color.BLACK);

            Raylib.BeginShaderMode(Assets.depthShader);
            {
                Raylib.DrawTextureRec(depthRenderTexture.depth, new Rectangle(0, 0, depthRenderTexture.depth.width, -depthRenderTexture.depth.height), Vector2.Zero, Color.WHITE);
            }
            Raylib.EndShaderMode();
        }
        Raylib.EndTextureMode();
        
        Texture2D depthTexture = secondDepthRenderTexture.texture;
        
        

        fixed (Camera3D* cameraPtr = &Camera)
        {

            return new LightManager.LightData()
            {
                Position = this.Position,
                Direction = this.Direction,
                CastShadows = true,
                Type = LightManager.LightType.Directional,
                
                TypeLoc = this.TypeLoc,
                DirectionLoc = this.DirectionLoc,
                CastShadowsLoc = this.CastShadowsLoc,
                PositionLoc = this.PositionLoc,
                CameraDataCount = 1,
                CameraDataCountLoc = this.CameraDataCountLoc,
                DepthTextureLocs = this.depthTextureLocs,
                
                CameraData = new LightManager.LightCameraData[1]
                {
                    new LightManager.LightCameraData()
                    {
                        CameraPosition = this.Position,
                        DepthTexture = depthTexture,
                        TextureSize = new Vector2(depthTexture.width, depthTexture.height),
                        ViewProjectionMatrix = Raylib.GetCameraViewMatrix(cameraPtr) *
                                               Raylib.GetCameraProjectionMatrix(cameraPtr, Camera.fovy),
                        
                        ViewProjectionMatrixLoc = this.LightCameraLocs[0].ViewProjectionMatrixLoc,
                        CameraPositionLoc = this.LightCameraLocs[0].CameraPositionLoc,
                        TextureSizeLoc = this.LightCameraLocs[0].TextureSizeLoc,
                        
                    }
                }

            };
        }
    }
}