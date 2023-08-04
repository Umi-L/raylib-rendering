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

// Output fragment color
out vec4 finalColor;

void main()
{
//    finalColor = texture(depth, fragTexCoord);
//    finalColor = texture(normals, fragTexCoord);
//    finalColor = texture(texture0, fragTexCoord);
//    finalColor = vec4(1.0, 0.0, 0.0, 1.0);
}