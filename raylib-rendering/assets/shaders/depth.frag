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
void main()
{
//    float zNear = 0.01; // camera z near
//    float zFar = 1000.0;  // camera z far
    float z = texture(depth, fragTexCoord).x;

    // Linearize depth value
//    float depthlinear = (2.0*zNear)/(zFar + zNear - z*(zFar - zNear));
    

    // Calculate final fragment color
    finalColor = vec4(vec3(z), 1.0f);
}