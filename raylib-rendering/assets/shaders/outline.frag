#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform sampler2D depth;
uniform sampler2D normals;
uniform vec2 screenSize;

uniform float outlineWidth;
uniform vec4 outlineColor;

// Output fragment color
out vec4 finalColor;

bool colorDistance(float threshold, vec4 color1, vec4 color2) {

    float distance = sqrt(pow(color1.x - color2.x, 2) + pow(color1.y - color2.y, 2) + pow(color1.z - color2.z, 2));
    if (distance > threshold)
        return false;

    return true;
}

float sampleDepth(vec2 uv) {
    return texture(depth, uv).r;
}



bool outlineNormal(float width)
{
    float normalThreshold = 0.5;

    float halfScaleFloor = floor(width * 0.5);
    float halfScaleCeil = ceil(width * 0.5);

    vec2 _MainTex_TexelSize = vec2(1.0 / screenSize.x, 1.0 / screenSize.y);

    vec2 bottomLeftUV = fragTexCoord - vec2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
    vec2 topRightUV = fragTexCoord + vec2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;  
    vec2 bottomRightUV = fragTexCoord + vec2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
    vec2 topLeftUV = fragTexCoord + vec2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

    vec3 bottomLeftNormal = texture(normals, bottomLeftUV).rgb;
    vec3 topRightNormal = texture(normals, topRightUV).rgb;
    vec3 bottomRightNormal = texture(normals, bottomRightUV).rgb;
    vec3 topLeftNormal = texture(normals, topLeftUV).rgb;

    vec3 normalFiniteDifference0 = topRightNormal - bottomLeftNormal;
    vec3 normalFiniteDifference1 = bottomRightNormal - topLeftNormal;

    float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));

    if (edgeNormal > normalThreshold) {
        return true;
    }
    return false;
}

bool outlineDepth(float width) {

    float depthThreshold = 0.5;

    float halfScaleFloor = floor(width * 0.5);
    float halfScaleCeil = ceil(width * 0.5);

    vec2 _MainTex_TexelSize = vec2(1.0 / screenSize.x, 1.0 / screenSize.y);

    vec2 bottomLeftUV = fragTexCoord - vec2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
    vec2 topRightUV = fragTexCoord + vec2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;  
    vec2 bottomRightUV = fragTexCoord + vec2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
    vec2 topLeftUV = fragTexCoord + vec2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

    float bottomLeftDepth = sampleDepth(bottomLeftUV);
    float topRightDepth = sampleDepth(topRightUV);
    float bottomRightDepth = sampleDepth(bottomRightUV);
    float topLeftDepth = sampleDepth(topLeftUV);

//    depthThreshold = depthThreshold * bottomLeftDepth;

    float depthFiniteDifference0 = topRightDepth - bottomLeftDepth;
    float depthFiniteDifference1 = bottomRightDepth - topLeftDepth;

    float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;

    if (edgeDepth > depthThreshold) {
        return true;
    }
    return false;
}

void main()
{
//    const float outlineWidth = 2.0;
//    const vec3 outlineColor = vec3(0);

    finalColor = texture(texture0, fragTexCoord);

    bool normalOutline = outlineNormal(outlineWidth);
    bool depthOutline = outlineDepth(outlineWidth);

    if (normalOutline || depthOutline) {
		finalColor = outlineColor;
	}

//    if (normalOutline && depthOutline) {
//		finalColor = vec4(1, 1, 0, 1);
//	} else if (depthOutline){
//        finalColor = vec4(0, 1, 0, 1);
//    } else if (normalOutline) {
//		finalColor = vec4(1, 0, 0, 1);
//    }
}