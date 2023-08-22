using System.Numerics;
using Raylib_cs;

namespace raylib_rendering;

public static class RewrittenFunctions
{
    public const int MaxMaterialMaps = 12;
    public const float Deg2Rad = (float)(Math.PI / 180.0f);
    
    // custom version of the Raylib DrawMesh function. This excludes opengl 1.1 support.
    public static unsafe void DrawMesh(Mesh mesh, Material material, Matrix4x4 transform)
    {
        
        // Bind shader program
        Rlgl.rlEnableShader(material.shader.id);

        // Send required data to shader (matrices, values)
        //-----------------------------------------------------
        // Upload to shader material.colDiffuse
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_COLOR_DIFFUSE] != -1)
        {
            float[] values = {
                material.maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color.r / 255.0f,
                material.maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color.g / 255.0f,
                material.maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color.b / 255.0f,
                material.maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color.a / 255.0f
            };

            fixed (float* ptr = values)
            {
                Rlgl.rlSetUniform(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_COLOR_DIFFUSE], ptr,
                    (int)ShaderUniformDataType.SHADER_UNIFORM_VEC4, 1);
            }
        }

        // Upload to shader material.colSpecular (if location available)
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR] != -1)
        {
            float[] values =  {
                material.maps[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR].color.r / 255.0f,
                material.maps[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR].color.g / 255.0f,
                material.maps[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR].color.b / 255.0f,
                material.maps[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR].color.a / 255.0f
            };
            
            fixed (float* ptr = values)
            {
                Rlgl.rlSetUniform(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_COLOR_SPECULAR], ptr,
                    (int)ShaderUniformDataType.SHADER_UNIFORM_VEC4, 1);
            }
        }

        // Get a copy of current matrices to work with,
        // just in case stereo render is required, and we need to modify them
        // NOTE: At this point the modelview matrix just contains the view matrix (camera)
        // That's because BeginMode3D() sets it and there is no model-drawing function
        // that modifies it, all use Rlgl.rlPushMatrix() and Rlgl.rlPopMatrix()
        Matrix4x4 matModel;
        Matrix4x4 matView = Rlgl.rlGetMatrixModelview();
        Matrix4x4 matModelView;
        Matrix4x4 matProjection = Rlgl.rlGetMatrixProjection();

        // Upload view and projection matrices (if locations available)
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_VIEW] != -1)
            Rlgl.rlSetUniformMatrix(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_VIEW], matView);
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_PROJECTION] != -1)
            Rlgl.rlSetUniformMatrix(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_PROJECTION], matProjection);

        // Model transformation matrix is sent to shader uniform location: (int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL] != -1)
            Rlgl.rlSetUniformMatrix(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MODEL], transform);

        // Accumulate several model transformations:
        //    transform: model transformation provided (includes DrawModel() params combined with model.transform)
        //    Rlgl.rlGetMatrixTransform(): Rlgl.rlgl internal transform matrix due to push/pop matrix stack
        matModel = Raymath.MatrixMultiply(transform, Rlgl.rlGetMatrixTransform());

        
        // Get model-view matrix
        matModelView = Raymath.MatrixMultiply(matModel, matView);

        // Upload model normal matrix (if locations available)
        if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_NORMAL] != -1)
            Rlgl.rlSetUniformMatrix(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_NORMAL], Raymath.MatrixTranspose(Raymath.MatrixInvert(matModel)));
        //-----------------------------------------------------

        
        // Bind active texture maps (if available)
        for (int i = 0; i < MaxMaterialMaps; i++)
        {
            if (material.maps[i].texture.id > 0)
            {
                // Select current shader texture slot
                Rlgl.rlActiveTextureSlot(i);

                // Enable texture for active slot
                if ((i == (int)MaterialMapIndex.MATERIAL_MAP_IRRADIANCE) ||
                    (i == (int)MaterialMapIndex.MATERIAL_MAP_PREFILTER) ||
                    (i == (int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP)) Rlgl.rlEnableTextureCubemap(material.maps[i].texture.id);
                else Rlgl.rlEnableTexture(material.maps[i].texture.id);

                Rlgl.rlSetUniform(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + i], &i, (int)ShaderUniformDataType.SHADER_UNIFORM_INT, 1);
            }
        }

        // Try binding vertex array objects (VAO) or use VBOs if not possible
        // WARNING: UploadMesh() enables all vertex attributes available in mesh and sets default attribute values
        // for shader expected vertex attributes that are not provided by the mesh (i.e. colors)
        // This could be a dangerous approach because different meshes with different shaders can enable/disable some attributes
        if (!Rlgl.rlEnableVertexArray(mesh.vaoId))
        {
            // Bind mesh VBO data: vertex position (shader-location = 0)
            Rlgl.rlEnableVertexBuffer(mesh.vboId[0]);
            Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_POSITION], 3, Rlgl.RL_FLOAT, 0, 0, (void *)0);
            Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_POSITION]);

            // Bind mesh VBO data: vertex texcoords (shader-location = 1)
            Rlgl.rlEnableVertexBuffer(mesh.vboId[1]);
            Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TEXCOORD01], 2, Rlgl.RL_FLOAT, 0, 0, (void *)0);
            Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TEXCOORD01]);

            if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_NORMAL] != -1)
            {
                // Bind mesh VBO data: vertex normals (shader-location = 2)
                Rlgl.rlEnableVertexBuffer(mesh.vboId[2]);
                Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_NORMAL], 3, Rlgl.RL_FLOAT, 0, 0, (void *)0);
                Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_NORMAL]);
            }

            // Bind mesh VBO data: vertex colors (shader-location = 3, if available)
            if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR] != -1)
            {
                if (mesh.vboId[3] != 0)
                {
                    Rlgl.rlEnableVertexBuffer(mesh.vboId[3]);
                    Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR], 4, Rlgl.RL_UNSIGNED_BYTE, 1, 0, (void *)0);
                    Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR]);
                }
                else
                {
                    // Set default value for defined vertex attribute in shader but not provided by mesh
                    // WARNING: It could result in GPU undefined behaviour
                    float[] value =  {
                        1.0f, 1.0f, 1.0f, 1.0f
                    };

                    fixed (float* ptr = value)
                    {
                        Rlgl.rlSetVertexAttributeDefault(
                            material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR], ptr,
                            (int)ShaderAttributeDataType.SHADER_ATTRIB_VEC4, 4);
                    }

                    Rlgl.rlDisableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR]);
                }
            }

            // Bind mesh VBO data: vertex tangents (shader-location = 4, if available)
            if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TANGENT] != -1)
            {
                Rlgl.rlEnableVertexBuffer(mesh.vboId[4]);
                Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TANGENT], 4, Rlgl.RL_FLOAT, 0, 0, (void *)0);
                Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TANGENT]);
            }

            // Bind mesh VBO data: vertex texcoords2 (shader-location = 5, if available)
            if (material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TEXCOORD02] != -1)
            {
                Rlgl.rlEnableVertexBuffer(mesh.vboId[5]);
                Rlgl.rlSetVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TEXCOORD02], 2, Rlgl.RL_FLOAT, 0, 0, (void *)0);
                Rlgl.rlEnableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_TEXCOORD02]);
            }

            if (mesh.indices != null) Rlgl.rlEnableVertexBufferElement(mesh.vboId[6]);
        }

        // WARNING: Disable vertex attribute color input if mesh can not provide that data (despite location being enabled in shader)
        if (mesh.vboId[3] == 0) Rlgl.rlDisableVertexAttribute((uint)material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VERTEX_COLOR]);

        int eyeCount = 1;
        if (Rlgl.rlIsStereoRenderEnabled()) eyeCount = 2;

        for (int eye = 0; eye < eyeCount; eye++)
        {
            // Calculate model-view-projection matrix (MVP)
            Matrix4x4 matModelViewProjection;
            if (eyeCount == 1) matModelViewProjection = Raymath.MatrixMultiply(matModelView, matProjection);
            else
            {
                // Setup current eye viewport (half screen width)
                Rlgl.rlViewport(eye * Rlgl.rlGetFramebufferWidth() / 2, 0, Rlgl.rlGetFramebufferWidth() / 2, Rlgl.rlGetFramebufferHeight());
                matModelViewProjection = Raymath.MatrixMultiply(Raymath.MatrixMultiply(matModelView, Rlgl.rlGetMatrixViewOffsetStereo(eye)),
                    Rlgl.rlGetMatrixProjectionStereo(eye));
            }

            // Send combined model-view-projection matrix to shader
            Rlgl.rlSetUniformMatrix(material.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MATRIX_MVP], matModelViewProjection);

            // Draw mesh
            if (mesh.indices != null) Rlgl.rlDrawVertexArrayElements(0, mesh.triangleCount * 3, (void *)0);
            else Rlgl.rlDrawVertexArray(0, mesh.vertexCount);
        }

        // Unbind all bound texture maps
        for (int i = 0; i < MaxMaterialMaps; i++)
        {
            if (material.maps[i].texture.id > 0)
            {
                // Select current shader texture slot
                Rlgl.rlActiveTextureSlot(i);

                // Disable texture for active slot
                if ((i == (int)MaterialMapIndex.MATERIAL_MAP_IRRADIANCE) ||
                    (i == (int)MaterialMapIndex.MATERIAL_MAP_PREFILTER) ||
                    (i == (int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP)) Rlgl.rlDisableTextureCubemap();
                else Rlgl.rlDisableTexture();
            }
        }

        // Disable all possible vertex array objects (or VBOs)
        Rlgl.rlDisableVertexArray();
        Rlgl.rlDisableVertexBuffer();
        Rlgl.rlDisableVertexBufferElement();

        // Disable shader program
        Rlgl.rlDisableShader();

        // Restore Rlgl.rlgl internal modelview and projection matrices
        Rlgl.rlSetMatrixModelview(matView);
        Rlgl.rlSetMatrixProjection(matProjection);
    }
    
    // Draw a model with extended parameters
    public static unsafe void DrawModelEx(Model model, Vector3 position, Vector3 rotationAxis, float rotationAngle, Vector3 scale, Color tint)
    {
        // Calculate transformation matrix from function parameters
        // Get transform matrix (rotation -> scale -> translation)
        Matrix4x4 matScale = Raymath.MatrixScale(scale.X, scale.Y, scale.Z);
        Matrix4x4 matRotation = Raymath.MatrixRotate(rotationAxis, rotationAngle*Deg2Rad);
        Matrix4x4 matTranslation = Raymath.MatrixTranslate(position.X, position.Y, position.Z);

        Matrix4x4 matTransform = Raymath.MatrixMultiply(Raymath.MatrixMultiply(matScale, matRotation), matTranslation);

        // Combine model transformation matrix (model.transform) with matrix generated by function parameters (matTransform)
        model.transform = Raymath.MatrixMultiply(model.transform, matTransform);

        for (int i = 0; i < model.meshCount; i++)
        {
            Color color = model.materials[model.meshMaterial[i]].maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color;

            Color colorTint = Color.WHITE;
            colorTint.r = (byte)(((color.r/255.0f)*(tint.r/255.0f))*255.0f);
            colorTint.g = (byte)(((color.g/255.0f)*(tint.g/255.0f))*255.0f);
            colorTint.b = (byte)(((color.b/255.0f)*(tint.b/255.0f))*255.0f);
            colorTint.a = (byte)(((color.a/255.0f)*(tint.a/255.0f))*255.0f);

            model.materials[model.meshMaterial[i]].maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color = colorTint;
            DrawMesh(model.meshes[i], model.materials[model.meshMaterial[i]], model.transform);
            model.materials[model.meshMaterial[i]].maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].color = color;
        }
    }
    
    public static void DrawModel(Model model, Vector3 position, float scale, Color tint)
    {
        Vector3 vScale = new Vector3(scale);
        Vector3 rotationAxis = new Vector3(0, 1, 0);

        DrawModelEx(model, position, rotationAxis, 0.0f, vScale, tint);
    }
}