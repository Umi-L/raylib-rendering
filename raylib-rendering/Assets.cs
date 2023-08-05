using Raylib_cs;
using raylib_rendering.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace raylib_rendering
{
    internal static class Assets
    {
        public static Shader testShader;
        public static Shader depthShader;
        public static Shader normalShader;
        public static Shader defaultShader;
        public static Shader outlineShader;
        public static Shader paperShader;
        public static Shader lightingShader;

        public static ShaderProgram outlineShaderProgram;
        public static ShaderProgram paperShaderProgram;

        public static Texture2D scarecrowTexture;

        public static Model scarecrowModel;
        public static Model planeModel;
        public static Model sceneModel;

        public static List<Model> models = new List<Model>();
        public static Dictionary<string, Model> Prefabs = new Dictionary<string, Model>();

        public static void Load()
        {
            Console.WriteLine("---------------- Beginning Loading Assets ----------------");

            // print cwd
            Console.WriteLine("Current working directory: " + Environment.CurrentDirectory);

            // load shaders
            testShader = Raylib.LoadShader(null, "assets/shaders/test.frag");
            depthShader = Raylib.LoadShader(null, "assets/shaders/depth.frag");
            normalShader = Raylib.LoadShader("assets/shaders/norm.vert", "assets/shaders/norm.frag");
            defaultShader = Raylib.LoadShader((string)null, (string)null);
            outlineShader = Raylib.LoadShader(null, "assets/shaders/outline.frag");
            paperShader = Raylib.LoadShader(null, "assets/shaders/paper.frag");
            lightingShader = Raylib.LoadShader("assets/shaders/lighting.vert", "assets/shaders/lighting.frag");

            unsafe
            {
                normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL] = Raylib.GetShaderLocation(normalShader, "matModel");
                lightingShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = Raylib.GetShaderLocation(lightingShader, "viewPos");
            }

            // load shaderPrograms
            outlineShaderProgram = new ShaderProgram(
                Assets.outlineShader,
                new Dictionary<string, ShaderProgramUniform>(){
                    { "outlineWidth", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 1f } },
                    { "outlineColor", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_VEC4, value = new Vector4(0,0,0,1) } }
                }
            );

            paperShaderProgram = new ShaderProgram(
                Assets.paperShader,
                new Dictionary<string, ShaderProgramUniform>()
                {
                    { "detail", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_INT, value = 10 } },
                    { "scroll", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(0, 0) } },
                    { "scale", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(1,1)} },
                    { "factor", new ShaderProgramUniform() { DataType = ShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0.15f } },
                    { "paperColor", new ShaderProgramUniform() {DataType = ShaderUniformDataType.SHADER_UNIFORM_VEC4, value = new Vector4(23f/255f,11f/255f,11f/255f,1)} }
                }
            );



            // load textures
            scarecrowTexture = Raylib.LoadTexture("assets/textures/scarecrow.png");

            // load materials


            // load models
            scarecrowModel = Raylib.LoadModel("assets/models/scarecrow.obj");
            Raylib.SetMaterialTexture(ref scarecrowModel, 0, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, ref scarecrowTexture);
            models.Add(scarecrowModel);

            planeModel = Raylib.LoadModel("assets/models/plane.obj");
            models.Add(planeModel);

            sceneModel = Raylib.LoadModel("assets/models/testScene.gltf");
            models.Add (sceneModel);

            // for every file in dir assets/prefabs
            foreach (string file in Directory.EnumerateFiles("assets/models/prefabs"))
            {
                string fixedPath = file.Replace("\\", "/");

                // split on all / or \
                string[] pathParts = fixedPath.Split('/');

                string fileName = pathParts[pathParts.Length-1];

                string[] fileParts = fileName.Split(".");

                string extension = fileParts[1];
                string fileTitle = fileParts[0];

                if (extension == "glb" || extension == "gltf")
                {
                    var model = Raylib.LoadModel(fixedPath);
                    models.Add(model);
                    Prefabs.Add(fileTitle, model);

                    Console.WriteLine($"Loaded prefab {fixedPath}");
                }
            }

            Console.WriteLine("---------------- Finished Loading Assets ----------------");
        }

        public static void SetModelsShader(Shader shader)
        {
            for (int i = 0; i < models.Count; ++i)
            {

                // foreach material in model
                for (int j = 0; j < models[i].materialCount; j++)
                {
                    Model model_ = models[i];

                    Raylib.SetMaterialShader(ref model_, j, ref shader);
                }
            }
        }

        

        public static void Unload()
        {
            Console.WriteLine("---------------- Beginning Unloading Assets ----------------");

            // unload shaders
            Raylib.UnloadShader(testShader);

            foreach (var model in models)
            {
                Raylib.UnloadModel(model);
            }

            // unload textures

            Console.WriteLine("---------------- Finished Unloading Assets ----------------");
        }
    }
}
