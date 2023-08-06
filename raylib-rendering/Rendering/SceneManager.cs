using Newtonsoft.Json;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ImGuiNET;
using static System.Formats.Asn1.AsnWriter;

namespace raylib_rendering.Rendering
{
    public class Scene
    {
        public SceneEntity[] entities;

        public void Draw(float scale)
        {
            foreach (SceneEntity entity in entities)
            {
                if (entity.id == null) continue;

                bool success = Assets.Prefabs.TryGetValue(entity.id, out var model);
                
                if (!success) throw new Exception($"Prefab with id {entity.id} not found");

                // rotation to quaternion
                Quaternion q = Quaternion.CreateFromYawPitchRoll(entity.rotation.Y, entity.rotation.X, entity.rotation.Z);
                
                // quaternion to axis angle
                float angle = (float)(2 * Math.Acos(q.W));

                double funnyNumber = Math.Sqrt(1 - q.W * q.W);
                
                if (double.IsNaN(funnyNumber) || funnyNumber == 0) funnyNumber = 1;

                Vector3 axis = new Vector3(
                    (float)(q.X / funnyNumber),
                    (float)(q.Y / funnyNumber),
                    (float)(q.Z / funnyNumber)
                );
                
                // convert angle to euler
                angle = angle * (float)(180 / Math.PI);
                
                // Console.WriteLine($"Angle: {angle}; Axis: {axis} id {entity.id} q.w {q.W} funnyNumber {funnyNumber}");
                
                Raylib.DrawModelEx(model, entity.position * scale, axis, angle, entity.scale * scale, Color.WHITE);
            }
        }

        public void DebugInfo()
        {
            foreach (SceneEntity entity in entities)
            {
                
                Vector3 rotationEuler = new Vector3(
                    (float)(entity.rotation.X * (180 / Math.PI)),
                    (float)(entity.rotation.Y * (180 / Math.PI)),
                    (float)(entity.rotation.Z * (180 / Math.PI))
                );
                
                ImGui.Text($"Name: {entity.id} Position: {entity.position} Rotation: {rotationEuler} Scale: {entity.scale}");
            }
        }
    }

    public class SceneEntity
    {
        public string name;
        public string? id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    internal class SceneManager
    {
        public static Scene LoadScene(string path)
        {
            string jsonData = File.ReadAllText(path);
            Scene? scene = JsonConvert.DeserializeObject<Scene>(jsonData);

            for (int i = 0; i < scene.entities.Length; i++)
            {
                SceneEntity entity = scene.entities[i];

                //swap y and z on all vec3s because blender
                entity.position = FlipYAndZ(entity.position);
                entity.position.Y = entity.position.Y;
                entity.position.X = entity.position.X;
                entity.position.Z = -entity.position.Z;
                
                entity.rotation = FlipYAndZ(entity.rotation);
                entity.scale = FlipYAndZ(entity.scale);

                // entity.rotation.Y += (float)Math.PI/2; // maybe fixes rotation?

                // Console.WriteLine($"Name: {entity.name}; Position: {entity.position}; Scale: {entity.scale}; rotation: {entity.rotation}");
                
                
            }

            return scene;
        }

        public static Vector3 FlipYAndZ(Vector3 input)
        {
            float oldz = input.Z;
            input.Z = input.Y;
            input.Y = oldz;

            return input;
        }
        
    }
}
