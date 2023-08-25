#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

#define MAX_INLINE_SEGMENTS 10

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform sampler2D depth;
uniform sampler2D normals;
uniform vec2 screenSize;

uniform float inlineWidth;
uniform vec4 inlineColor;
uniform mat4 viewProjectionMatrix;

struct InlineSegment {
    vec3 start;
    vec3 end;
};

uniform InlineSegment inlineSegments[MAX_INLINE_SEGMENTS];
uniform int inlineSegmentsCount;

// Output fragment color
out vec4 finalColor;

vec2 drawLineWithPercentage(vec2 p1, vec2 p2, float thickness) {
    vec2 uv = gl_FragCoord.xy / screenSize.xy;

    float a = abs(distance(p1, uv));
    float b = abs(distance(p2, uv));
    float c = abs(distance(p1, p2));

    if (a >= c || b >= c) {
        return vec2(0.0, 0.0); // Return fully transparent and 0 percentage
    }

    float p = (a + b + c) * 0.5;
    float h = 2.0 / c * sqrt(p * (p - a) * (p - b) * (p - c));

    float alpha = mix(1.0, 0.0, smoothstep(0.5 * thickness, 1.5 * thickness, h));
    float percentage = distance(p1, uv) / c; // Calculate the percentage along the line

    return vec2(alpha, percentage);
}

float OrthographicDistanceFromCamera(vec3 position) {
    vec4 clipSpacePosition = viewProjectionMatrix * vec4(position, 1.0);
    vec3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;
    vec3 projCoords = ndcSpacePosition * 0.5 + 0.5;
    return projCoords.z;
}

void main()
{
    
    float thicknessUV = inlineWidth / screenSize.x;
    
    // foreach line to draw
    for (int i = 0; i < inlineSegmentsCount; i++) {
        
        InlineSegment lineSegment = inlineSegments[i];
        
        vec4 p1 = vec4(lineSegment.start, 1.0);
        vec4 p2 = vec4(lineSegment.end, 1.0);
        
        // Transform p1 to clip space
        vec4 clipSpacePositionP1 = viewProjectionMatrix * p1;
        vec4 clipSpacePositionP2 = viewProjectionMatrix * p2;

        // Divide by w in clip space to get NDC space
        vec3 ndcSpacePositionP1 = clipSpacePositionP1.xyz / clipSpacePositionP1.w;
        vec3 ndcSpacePositionP2 = clipSpacePositionP2.xyz / clipSpacePositionP2.w;
        
        // Transform to screen space
        vec3 projCoordsP1 = ndcSpacePositionP1 * 0.5 + 0.5;
        vec3 projCoordsP2 = ndcSpacePositionP2 * 0.5 + 0.5;
        
        // draw line
        vec2 line = drawLineWithPercentage(projCoordsP1.xy, projCoordsP2.xy, thicknessUV);
        
        // if line crosses texel
        if (line.x > 0.0){
            
//            // draw percentage
//            finalColor = vec4(line.y, line.y, line.y, 1.0);
//            return;

            // check depth at current fragment
            float depthValue = texture(depth, fragTexCoord).r;
            
            // get position along line using percentage
            vec3 position = mix(lineSegment.start, lineSegment.end, line.y);
            
            // get distance from camera
            float distance = OrthographicDistanceFromCamera(position);
            
            // check if depth is closer than current fragment
            if (depthValue > distance) {
                finalColor = inlineColor;
                return;
            }
        }
    }
    
    finalColor = colDiffuse * texture(texture0, fragTexCoord);
}