using System.Numerics;
using Raylib_cs;

namespace raylib_rendering.Rendering;

public struct InlineSegment
{
    public Vector3 Start;
    public Vector3 End;
}

public static class InlineManager
{
    public const int MAX_INLINE_SEGMENTS = 10000;
    public static int DATA_TEXTURE_SIZE;
    public static List<InlineSegment> Segments = new List<InlineSegment>();
    public static Image inlineDataImage;
    public static Texture2D inlineDataTexture;

    public static void init()
    {
        
        
        DATA_TEXTURE_SIZE = (int) Math.Ceiling(Math.Sqrt(MAX_INLINE_SEGMENTS*3));
        inlineDataImage = Raylib.GenImageColor(DATA_TEXTURE_SIZE, DATA_TEXTURE_SIZE, Color.BLACK);
        inlineDataImage.format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R32;
    }

    public static void AddSegment(InlineSegment segment)
    {
        Segments.Add(segment);
    }
    
    public static void AddSegment(Vector3 start, Vector3 end)
    {
        Segments.Add(new InlineSegment()
        {
            Start = start,
            End = end
        });
        
        UpdateShaderUniforms();
    }
    
    public static void UpdateShaderUniforms()
    {
        Assets.inlineShaderProgram.SetShaderUniform("inlineSegmentsCount", Segments.Count, ExtendedShaderUniformDataType.SHADER_UNIFORM_INT);

        // // foreach inlineSegment
        // for (int i = 0; i < Segments.Count; i++)
        // {
        //     var segment = Segments[i];
        //
        //     Color X = new Color();
        //     
        //     for (int j = 0; i < 3; i++)
        //     {
        //         // calculate the position in the texture
        //         int x = (i+j) % DATA_TEXTURE_SIZE;
        //         int y = (i+j) / DATA_TEXTURE_SIZE;
        //         
        //         // set the pixel
        //         Raylib.ImageDrawPixel(ref inlineDataImage, i, j, new Color());
        //     }
        // }
        
        // forach segment, set the uniform
        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            
            Assets.inlineShaderProgram.SetShaderUniform("inlineSegments[" + i + "].start", segment.Start, ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC3);
            Assets.inlineShaderProgram.SetShaderUniform("inlineSegments[" + i + "].end", segment.End, ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
    }
    
    public static void DebugDrawSegments()
    {
        foreach (InlineSegment segment in Segments)
        {
            Raylib.DrawLine3D(segment.Start, segment.End, Color.GREEN);
            
            // draw the start and end points
            Raylib.DrawSphere(segment.Start, 0.1f, Color.BLUE);
            Raylib.DrawSphere(segment.End, 0.1f, Color.BLUE);
        }
    }

    public static void ClearSegments()
    {
        Segments.Clear();
    }
}