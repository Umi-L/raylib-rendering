#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;     // Depth texture
uniform vec4 colDiffuse;
uniform sampler2D depth;
uniform vec2 screenSize;

// Output fragment color
out vec4 finalColor;

// Function to pack bits from the first number into a vector3
vec3 packBitsIntoVector3(float x) {
    
    vec3 vector3 = vec3(0.0, 0.0, 0.0);
    
    x *= 3;
    
    if (x > 2.0){
        vector3.r = 1.0;
        vector3.g = 1.0;
        vector3.b = x-2.0;
    } else if (x > 1.0){
        vector3.r = 1.0;
        vector3.g = x-1.0;
    } else {
        vector3.r = x;
    }
    
    return vector3;
}

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
//    float zNear = 0.01; // camera z near
//    float zFar = 1000.0;  // camera z far
    float z = texture(depth, fragTexCoord).x;

    // Linearize depth value
//    float depthlinear = (2.0*zNear)/(zFar + zNear - z*(zFar - zNear));

    vec3 depth24Bit = packBitsIntoVector3(z);
    
    // Calculate final fragment color
    finalColor = vec4(depth24Bit, 1.0f);
}