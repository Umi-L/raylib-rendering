using System.Numerics;
using Raylib_cs;

namespace raylib_rendering;

public static class Utils
{
    public static unsafe Matrix4x4 GetCameraViewProjectionMatrix(ref Camera3D camera)
    {
        fixed (Camera3D* cameraPtr = &camera)
        {

            Matrix4x4 viewMatrix = Raylib.GetCameraViewMatrix(cameraPtr);
            Matrix4x4 projectionMatrix = Raylib.GetCameraProjectionMatrix(cameraPtr, 1);
            
            return projectionMatrix * viewMatrix;
        }

    }
}