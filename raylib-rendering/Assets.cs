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
        public static Shader colourFilterShader;
        public static Shader inlineShader;
        public static Shader displacementShader;
        
        public static ShaderProgram outlineShaderProgram;
        public static ShaderProgram paperShaderProgram;
        public static ShaderProgram colourFilterShaderProgram;
        public static ShaderProgram inlineShaderProgram;
        public static ShaderProgram testShaderProgram;
        public static ShaderProgram displacementShaderProgram;

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
            testShader = LoadShader(null, "assets/shaders/test.frag");
            depthShader = LoadShader(null, "assets/shaders/depth.frag");
            normalShader = LoadShader("assets/shaders/norm.vert", "assets/shaders/norm.frag");
            defaultShader = LoadShader((string)null, (string)null);
            outlineShader = LoadShader(null, "assets/shaders/outline.frag");
            paperShader = LoadShader(null, "assets/shaders/paper.frag");
            lightingShader = LoadShader("assets/shaders/lighting.vert", "assets/shaders/lighting.frag");
            colourFilterShader = LoadShader(null, "assets/shaders/colourFilter.frag");
            inlineShader = LoadShader(null, "assets/shaders/inline.frag");
            displacementShader = LoadShader(null, "assets/shaders/displacement.frag");
            
            unsafe
            {
                normalShader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL] = Raylib.GetShaderLocation(normalShader, "matModel");
                lightingShader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = Raylib.GetShaderLocation(lightingShader, "viewPos");
            }

            // load shaderPrograms
            outlineShaderProgram = new ShaderProgram(
                Assets.outlineShader,
                new Dictionary<string, ShaderProgramUniform>(){
                    { "outlineWidth", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 2f } },
                    { "outlineColor", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC4, value = new Vector4(0,0,0,1) } }
                }
            );
            
            inlineShaderProgram = new ShaderProgram(
                Assets.inlineShader,
                new Dictionary<string, ShaderProgramUniform>()
                {
                    { "inlineWidth", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 2f  } },
                    { "inlineColor", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC4, value = new Vector4(0,0,0,1) } }
                }
            );

            paperShaderProgram = new ShaderProgram(
                Assets.paperShader,
                new Dictionary<string, ShaderProgramUniform>()
                {
                    { "detail", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_INT, value = 10 } },
                    { "scroll", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(0, 0) } },
                    { "scale", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(1,1)} },
                    { "factor", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0.015f } },
                    { "paperColor", new ShaderProgramUniform() {DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC4, value = new Vector4(1,1,1,1)} }
                }
            );
            
            displacementShaderProgram = new ShaderProgram(
                Assets.displacementShader,
                new Dictionary<string, ShaderProgramUniform>()
                {
                    { "scroll", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(0, 0) } },
                    { "scale", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC2, value = new Vector2(1,1)} },
                    { "factor", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0.015f } },
                }
            );
            
            colourFilterShaderProgram = new ShaderProgram(
                Assets.colourFilterShader,
                new Dictionary<string, ShaderProgramUniform>()
                {
                    { "hueShift", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0f } },
                    { "saturationShift", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0.2f } },
                    { "lightnessShift", new ShaderProgramUniform() { DataType = ExtendedShaderUniformDataType.SHADER_UNIFORM_FLOAT, value = 0.2f } }
                }
            );

            testShaderProgram = new ShaderProgram(Assets.testShader, new Dictionary<string, ShaderProgramUniform>());
            



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
        
        public static Shader LoadShader(string? vertexPath, string? fragmentPath)
        {
            // load text file
            string? vertexText = (vertexPath != null) ? File.ReadAllText(vertexPath) : null;
            string? fragmentText = (fragmentPath != null) ? File.ReadAllText(fragmentPath) : null;

            if (fragmentText != null)
            {
                Console.WriteLine($"Text loaded for {fragmentPath} successfully");
            }
            
            if (vertexText != null)
            {
                Console.WriteLine($"Text loaded for {vertexPath} successfully");
            }
            
            // load from mem
            Shader shader = Raylib.LoadShaderFromMemory(vertexText, fragmentText);

            return shader;
        }
    }
}
