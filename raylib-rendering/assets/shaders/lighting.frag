#version 330

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables

#define     MAX_LIGHTS              4
#define    MAX_LIGHT_CAMERAS       4

#define     LIGHT_DIRECTIONAL       0
#define     LIGHT_POINT             1

struct LightCameraData {
    vec3 cameraPosition;
    vec2 textureSize;
    mat4 veiwProjectionMatrix;
};

struct LightData {
    int type;
    vec3 position;
    vec3 direction;
    bool castShadows;

    int cameraDataCount;
    LightCameraData cameraData[MAX_LIGHT_CAMERAS];
};

// Input lighting values
uniform LightData lights[MAX_LIGHTS];
uniform sampler2D depthTextures[MAX_LIGHTS*MAX_LIGHT_CAMERAS];


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
    
    
    finalColor = texture(depthTextures[0], fragTexCoord);
    return;
    
    
    for (int i = 0; i < lightsCount; i++){
        
        LightData light = lights[i];

        float lightDistance = length(light.position - fragPosition);
        
        for (int j = 0; j < light.cameraDataCount; j++){
            
            vec4 clipSpacePosition = light.cameraData[j].veiwProjectionMatrix * vec4(fragPosition, 1.0);
            
            vec3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;
            
            vec2 fragUV = ndcSpacePosition.xy * 0.5 + 0.5;
            
            int shadowMapIndex = i * MAX_LIGHT_CAMERAS + j;
            vec3 depthInLightColor = texture(depthTextures[shadowMapIndex], fragUV).rgb;

            float depthInLight = unpack(depthInLightColor);
            
            finalColor =  texture(depthTextures[shadowMapIndex], fragTexCoord);
            return;
            
            if (depthInLight < lightDistance){
                finalColor = texelColor*colDiffuse*fragColor*ambient*shadowColor;
                return;
            }
        }
    }
    
    
    finalColor = texelColor*colDiffuse*fragColor*ambient;
}
