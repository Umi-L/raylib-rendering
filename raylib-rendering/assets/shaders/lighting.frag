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

#define pcfRadius 1

struct LightCameraData {
    vec3 cameraPosition;
    vec2 textureSize;
    mat4 viewProjectionMatrix;
    
    float zNear;
    float zFar;
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
//uniform sampler2D depthTexture;


uniform vec4 ambient;
uniform vec3 viewPos;
uniform int lightsCount;

float adaptiveBias(float distanceToLight, float bias) {
    float adjustedBias = bias * smoothstep(0.0, 1.0, distanceToLight);

    return adjustedBias;
}

float shadowCalculation(LightData light, int cameraDataIndex, int shadowMapIndex, float bias) {
    
    // Transform fragment position to light space
    vec4 clipSpacePosition = light.cameraData[cameraDataIndex].viewProjectionMatrix * vec4(fragPosition, 1.0);

    // Divide by w in clip space to get NDC space
    vec3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;

    vec3 projCoords = ndcSpacePosition * 0.5 + 0.5;

    // light distance accounting for orthographic projection
    float lightDistance = projCoords.z;

    // Assuming fragUV is in the range [-1, 1]
    vec2 fragUV = projCoords.xy;

    vec3 depthInLightColor = texture(depthTextures[shadowMapIndex], fragUV).rgb;

    float depthInLight = depthInLightColor.r;

    // Perform Percentage Closer Filtering (PCF)
    float shadow = 0.0;
    int numSamples = 0;

    for (int x = -pcfRadius; x <= pcfRadius; x++) {
        for (int y = -pcfRadius; y <= pcfRadius; y++) {
            vec2 offset = vec2(float(x), float(y)) / float(pcfRadius);

            vec2 sampleUV = fragUV + offset * (1.0 / textureSize(depthTextures[shadowMapIndex], 0));

            vec3 sampleDepthInLightColor = texture(depthTextures[shadowMapIndex], sampleUV).rgb;
            float sampleDepthInLight = sampleDepthInLightColor.r;

            if (sampleDepthInLight < lightDistance - bias) {
                shadow += 1.0;
            }

            numSamples++;
        }
    }

    shadow /= float(numSamples);

    return shadow;
}   

void main()
{

    const float shadowDarkness = 0.7;
    const float bias = 0.0002;
    
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    
//    finalColor = texelColor*colDiffuse*fragColor*ambient;
//    return;
//    
//
//    finalColor =  texture(depthTextures[0], fragTexCoord);
//    return;

    //    finalColor = vec4(0, 0, 1, 1);
//    return;
    
    
    for (int i = 0; i < lightsCount; i++){
        
        LightData light = lights[i];
        
        for (int j = 0; j < light.cameraDataCount; j++){
            int shadowMapIndex = i * MAX_LIGHT_CAMERAS + j;

            // Calculate the distance from the fragment to the light source
            float distanceToLight = length(light.cameraData[j].cameraPosition - fragPosition);

            // Calculate the adaptive bias based on the distance
            float adjustedBias = adaptiveBias(distanceToLight, bias);

            float shadowFactor = shadowCalculation(light, j, shadowMapIndex, adjustedBias);
            
            shadowFactor = 1 - shadowFactor;
            
            shadowFactor *= 0.3;
            
            vec4 shadowColor = vec4(vec3(shadowDarkness + shadowFactor), 1.0);
            
            finalColor = texelColor * colDiffuse * fragColor * ambient * (shadowColor);
            return;
        }
    }
    
    
    finalColor = texelColor*colDiffuse*fragColor*ambient;
}
