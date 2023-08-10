#version 330

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;
in vec2 fragTexCoord;
//in vec4 fragColor;
in vec3 fragNormal;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables

#define     MAX_LIGHTS              4
#define     LIGHT_DIRECTIONAL       0
#define     LIGHT_POINT             1

struct LightCameraData {
    vec3 CameraPosition;
    sampler2D DepthTexture;
    vec2 TextureSize;
    mat4 VeiwProjectionMatrix;
};

struct LightData {
    LightType Type;
    vec3 Position;
    vec3 Direction;
    bool CastShadows;

    int CameraDataCount;
    LightCameraData CameraData[];
};

// Input lighting values
uniform LightData lights[MAX_LIGHTS];
uniform vec4 ambient;
uniform vec3 viewPos;
uniform int lightsCount;

float unpack(vec3 vector3) {

    float x = 0.0;

    if (vector3.r == 1.0){
        if (vector3.g == 1.0){
            x = vector3.b + 2.0;
        } else {
            x = vector3.g + 1.0;
        }
    } else {
        x = vector3.r;
    }

    x /= 3;

    return x;
}

void main()
{
    const vec4 shadowColor = vec4(0.2, 0.2, 0.2, 1);
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    for (int i = 0; i < lightsCount; i++){
        LightData light = lights[i];

        float lightDistance = length(light.Position - fragPosition);

        for (int j = 0; j < light.CameraDataCount; j++){
            vec4 clipSpacePosition = light.CameraData[j].VeiwProjectionMatrix * vec4(fragPosition, 1.0);
            
            vec3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;
            
            vec2 fragUV = ndcSpacePosition.xy * 0.5 + 0.5;
            
            vec3 depthInLightColor = texture(light.CameraData[j].DepthTexture, fragUV).rgb;
            
            float depthInLight = unpack(depthInLightColor);
            
            if (depthInLight < lightDistance){
                finalColor = texelColor - shadowColor;
                
                // clamp final color
                finalColor.r = clamp(finalColor.r, 0.0, 1.0);
                finalColor.g = clamp(finalColor.g, 0.0, 1.0);
                finalColor.b = clamp(finalColor.b, 0.0, 1.0);
                finalColor.a = clamp(finalColor.a, 0.0, 1.0);
                
                return;
            }
        }
    }
    
    finalColor = texelColor;
}
