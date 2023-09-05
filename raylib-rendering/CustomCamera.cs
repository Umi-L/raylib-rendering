using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace raylib_rendering
{
    internal class CustomCamera
    {
        static float moveSpeed = 10f;
        static float fovSpeed = 1f;
        static float distSpeed = 0.001f;
        static float rotationSpeed = 1f;
        static float targetAngle = MathF.PI/4;
        static float currentAngle = targetAngle;
        static float targetVerticalAngle = 0;
        static float rotationDuration = 2.5f;
        static float rotationTimer = rotationDuration;

        public static void UpdateCamera(ref Camera3D camera)
        {
            var deltaTime = Raylib.GetFrameTime();
            
            // get camera facing vector
            Vector3 facing = new Vector3(
                camera.target.X - camera.position.X,
                0,
                camera.target.Z - camera.position.Z
            );
            
            // facing with y
            Vector3 trueFacing = new Vector3(
                camera.target.X - camera.position.X,
                camera.target.Y - camera.position.Y,
                camera.target.Z - camera.position.Z
            );
            
            // normalize facing vector
            facing = Vector3.Normalize(facing);
            
            // right vector
            Vector3 right = Vector3.Cross(facing, new Vector3(0, 1, 0));
            

            // check for wasd down
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                camera.position += facing * moveSpeed * deltaTime;
                camera.target += facing * moveSpeed * deltaTime;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                camera.position -= facing * moveSpeed * deltaTime;
                camera.target -= facing * moveSpeed * deltaTime;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                camera.position -= right * moveSpeed * deltaTime;
                camera.target -= right * moveSpeed * deltaTime;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                camera.position += right * moveSpeed * deltaTime;
                camera.target += right * moveSpeed * deltaTime;
            }
            
            // check for arrow key up and down
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
            {
                camera.position += trueFacing * distSpeed;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                camera.position -= trueFacing * distSpeed;

            }
            
            
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
            {
                targetAngle -= MathF.PI/2;
                rotationTimer = 0;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
            {
                targetAngle += MathF.PI/2;
                rotationTimer = 0;
            }
            
            // check for scroll wheel
            camera.fovy += Raylib.GetMouseWheelMoveV().Y * fovSpeed;
            
            // compare angle to target angle
            if (rotationTimer < rotationDuration)
            {
                // add to timer
                rotationTimer += deltaTime;
                
                // lerp angle to target angle
                currentAngle = MathFuncs.EasingFunctions.Interpolate(currentAngle, targetAngle,(rotationTimer / rotationDuration), MathFuncs.EasingFunctions.InOutSine);
                
                // lerp angle to target angle
                float distance = Vector2.Distance(new Vector2(camera.position.X, camera.position.Z), new Vector2(camera.target.X, camera.target.Z));
                
                // move camera position to point on circle with radius of distance between camera and target and angle of target angle
                camera.position = new Vector3(
                    camera.target.X + (float)Math.Sin(currentAngle) * distance,
                    camera.position.Y,
                    camera.target.Z + (float)Math.Cos(currentAngle) * distance
                );
            }
            
            
            // clamp values
            if (camera.fovy < 1)
            {
                camera.fovy = 1;
            }

            Assets.displacementShaderProgram.SetShaderUniform("scroll", new Vector2(camera.position.X, camera.position.Y)/50);
            
            if (Raylib.GetMouseWheelMoveV().Y != 0)
            {
                
                Assets.displacementShaderProgram.SetShaderUniform("scale", new Vector2(camera.fovy));
                
                Assets.displacementShaderProgram.SetShaderUniform("factor", 0.5f / (camera.fovy / 10f));

                
                // make line width smaller as the fov gets bigger
                float newLineWidth = 10f / (camera.fovy / 10f);
                
                // clamp
                newLineWidth = Math.Clamp(newLineWidth, 0.1f, 10f);
                
                Assets.outlineShaderProgram.SetShaderUniform("outlineWidth", newLineWidth);
                Assets.inlineShaderProgram.SetShaderUniform("inlineWidth", newLineWidth);
            }
            
            
            
            
        }
    }
}
