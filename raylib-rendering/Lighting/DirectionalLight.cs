using System.Numerics;
using Raylib_cs;
using raylib_rendering.Rendering;
using SpixelRenderer;

namespace raylib_rendering.Lighting;

public class DirectionalLight : Light
{
    public DirectionalLight(Vector3 positon, Vector3 lookAt, float fov)
    {
        Position = positon;
        Direction = Vector3.Normalize(lookAt - positon);

        Camera = new Camera3D(
            Position,
            lookAt,
            new Vector3(0, 1, 0),
            fov,
            CameraProjection.CAMERA_ORTHOGRAPHIC
        );
        
        LightManager.addLight(this);
        
        InitCameraLocs(1);
        
        const int width = 4000;
        const int height = 4000;
        
        depthRenderTexture = DepthTexture.LoadRenderTextureDepthTex(width, height);
        secondDepthRenderTexture = DepthTexture.LoadRenderTextureDepthTex(width, height);
        
    }

    private Camera3D Camera;
    private RenderTexture2D depthRenderTexture;
    private RenderTexture2D secondDepthRenderTexture;
    private Vector3 Position;
    private Vector3 Direction;
    
    public override unsafe LightManager.LightData UpdateLight(RenderSystem.DrawCallback drawCallback)
    {
        Camera.position = Position;
        // Camera.target = Position + Direction;
        
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
            
            Matrix4x4 viewMatrix = Raylib.GetCameraViewMatrix(cameraPtr);
            Matrix4x4 projectionMatrix = Raylib.GetCameraProjectionMatrix(cameraPtr, 1);
            
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
                        ZNear = Light.ZNear,
                        ZFar = Light.ZFar,
                        
                        CameraPosition = this.Position,
                        DepthTexture = depthTexture,
                        TextureSize = new Vector2(depthTexture.width, depthTexture.height),
                        ViewProjectionMatrix = projectionMatrix * viewMatrix,
                        ViewProjectionMatrixLoc = this.LightCameraLocs[0].ViewProjectionMatrixLoc,
                        CameraPositionLoc = this.LightCameraLocs[0].CameraPositionLoc,
                        TextureSizeLoc = this.LightCameraLocs[0].TextureSizeLoc,
                        ZNearLoc = this.LightCameraLocs[0].ZNearLoc,
                        ZFarLoc = this.LightCameraLocs[0].ZFarLoc,
                        
                    }
                }

            };
        }
    }

    public void SetPosition(Vector3 position)
    {
        this.Position = position;
     
        // set camera position
        Camera.position = position;
        
        // update dir
        this.Direction = Vector3.Normalize(Camera.target - position);
    }
}