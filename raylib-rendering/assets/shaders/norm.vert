#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec3 vertexNormal;

// Input uniform values
uniform mat4 mvp;
uniform mat4 matModel;

// Output vertex attributes (to fragment shader)
out vec3 modNorm;

void main()
{

    vec3 normalizedNormal = (vec3(vec4(vertexNormal,1)*matModel) + vec3(1)) / vec3(2);

//    modNorm = vec3(vec4(normalizedNormal,1)*matModel);
    modNorm = normalizedNormal;
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}
