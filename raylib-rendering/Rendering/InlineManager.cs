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
    public static List<InlineSegment> Segments = new List<InlineSegment>();

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
        
        // forach segment, set the uniform
        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            
            Assets.inlineShaderProgram.SetShaderUniform("inlineSegments[" + i + "].start", segment.Start, ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC3);
            Assets.inlineShaderProgram.SetShaderUniform("inlineSegments[" + i + "].end", segment.End, ExtendedShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
    }

    public static void ClearSegments()
    {
        Segments.Clear();
    }
}