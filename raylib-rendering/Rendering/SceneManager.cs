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

                Assets.Prefabs.TryGetValue(entity.id, out var model);

                Raylib.DrawModelEx(model, entity.position * scale, Vector3.Zero, 0f, entity.scale * scale, Color.WHITE);
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
                entity.position.Y = -entity.position.Y;

                entity.rotation = FlipYAndZ(entity.rotation);
                entity.scale = FlipYAndZ(entity.scale);


                Console.WriteLine($"Name: {entity.name}; Position: {entity.position}; Scale: {entity.scale}; rotation: {entity.rotation}");
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
