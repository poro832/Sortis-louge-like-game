shader_type canvas_item;

uniform vec4 color_dark : source_color = vec4(0.05, 0.03, 0.10, 1.0);
uniform vec4 color_accent : source_color = vec4(0.83, 0.69, 0.22, 1.0);
uniform float ray_intensity : hint_range(0.0, 0.3) = 0.08;

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

void fragment() {
    vec2 uv = UV;
    float t = TIME * 0.15;

    // 깊은 다크 배경
    vec4 bg = color_dark;

    // 중앙에서 퍼지는 은은한 골드 빛
    vec2 center = uv - vec2(0.5, 0.4);
    float dist = length(center);
    float glow = exp(-dist * 3.5) * 0.15;
    bg.rgb += color_accent.rgb * glow;

    // 느리게 흐르는 연기/안개
    float n1 = noise(uv * 3.0 + vec2(t, t * 0.7));
    float n2 = noise(uv * 5.0 - vec2(t * 0.5, t));
    float fog = (n1 + n2 * 0.5) * 0.03;
    bg.rgb += vec3(fog * 0.5, fog * 0.3, fog * 0.8);

    // 미세한 골드 광선 (수직)
    float ray = sin(uv.x * 40.0 + t * 2.0) * 0.5 + 0.5;
    ray *= exp(-dist * 4.0);
    bg.rgb += color_accent.rgb * ray * ray_intensity;

    // 강한 비네팅
    float vignette = smoothstep(1.0, 0.3, dist);
    bg.rgb *= mix(0.3, 1.0, vignette);

    COLOR = bg;
}
