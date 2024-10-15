#version 330 core
in vec2 textCoord;

out vec4 out_color;

uniform sampler2D texture0;
uniform vec4 tintColor = vec4(1, 1, 1, 1);
uniform float alpha;

uniform int param;

void useSDF()
{
    float threshold = 0.05;
    out_color = vec4(tintColor.rgb, smoothstep(0.5 - threshold, 0.5 + threshold, texture(texture0, textCoord).r));
}

void useTint()
{
    out_color = tintColor;
}

void main()
{
    switch (param){
        case 1:
            useTint();
            break;
        case 2:
            useSDF();
            break;
        default :
            vec4 color = texture(texture0, textCoord);
            color.rgb *= tintColor.rgb;
            color.a *= alpha;
        
            out_color = color;
            break;
    }
}