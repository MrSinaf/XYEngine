#version 330 core
in vec2 textCoord;
in float textTarget;

out vec4 out_color;

uniform vec4 colorTarget;
uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    vec4 color;

    if (textTarget == 0)
    color = texture(texture0, textCoord);
    else if (textTarget == 1)
    color = texture(texture1, textCoord);

    if (colorTarget.a == 1 && color.r > 0 && color.gb == vec2(0, 0)){
        float intensity = color.r;

        color.r = intensity * colorTarget.r;
        color.g = intensity * colorTarget.g;
        color.b = intensity * colorTarget.b;
    }

    out_color = color;
}