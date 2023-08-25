using System.Numerics;
using Raylib_cs;

namespace raylib_rendering;

public static class Utils
{
    public static unsafe Matrix4x4 GetCameraViewProjectionMatrix(Camera3D camera)
    {
        Camera3D* cameraPtr = &camera;
        Matrix4x4 viewMatrix = Raylib.GetCameraViewMatrix(cameraPtr);
        Matrix4x4 projectionMatrix = Raylib.GetCameraProjectionMatrix(cameraPtr, 1);
        
        return projectionMatrix * viewMatrix;
    }
}