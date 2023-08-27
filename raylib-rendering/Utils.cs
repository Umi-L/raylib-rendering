using System.Numerics;
using Raylib_cs;

namespace raylib_rendering;

public static class Utils
{
    public static Matrix4x4 GetCameraViewProjectionMatrix(ref Camera3D camera)
    {
        // only orthographic camera is supported
        float aspect = Raylib.GetScreenWidth() / (float)Raylib.GetScreenHeight();
        double top = camera.fovy/2.0;
        double right = top*aspect;

        // Calculate projection matrix from orthographic
        Matrix4x4 matProj = Raymath.MatrixOrtho(-right, right, -top, top, 0.01f, 1000f);
    
        // Calculate view matrix from camera look at (and transpose it)
        Matrix4x4 matView = Raymath.MatrixLookAt(camera.position, camera.target, camera.up);
        
        return matProj * matView;
    }
}