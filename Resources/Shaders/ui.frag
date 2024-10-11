#version 330 core
in vec2 textCoord;

out vec4 out_color;

uniform sampler2D texture0;
uniform vec4 tintColor = vec4(1, 1, 1, 1);
uniform float alpha;

void main()
{
    vec4 color = texture(texture0, textCoord);
    color.rgb *= tintColor.rgb;
    color.a *= alpha;
    
    out_color = color;
}