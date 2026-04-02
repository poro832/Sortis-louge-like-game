shader_type canvas_item;

uniform vec4 color_top : source_color = vec4(0.08, 0.05, 0.18, 1.0);
uniform vec4 color_bottom : source_color = vec4(0.04, 0.04, 0.12, 1.0);
uniform vec4 color_accent : source_color = vec4(0.61, 0.35, 0.71, 1.0);
uniform float time_scale : hint_range(0.0, 2.0) = 0.25;

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
    float t = TIME * time_scale;

    // 그라디언트 배경
    vec4 bg = mix(color_top, color_bottom, uv.y);

    // 느리게 흐르는 성운/안개 (2레이어)
    float n1 = noise(uv * 3.0 + vec2(t * 0.6, t * 0.3));
    float n2 = noise(uv * 6.0 - vec2(t * 0.4, t * 0.7));
    float fog = (n1 * 0.6 + n2 * 0.4);

    // 보라빛 성운 — 눈에 보이는 강도로
    bg.rgb += color_accent.rgb * fog * 0.08;

    // 중앙 은은한 발광
    vec2 center = uv - vec2(0.5, 0.45);
    float dist = length(center);
    float glow = exp(-dist * 3.0) * 0.1;
    bg.rgb += color_accent.rgb * glow;

    // 미세한 별 반짝임
    float star = hash(floor(uv * 80.0));
    star = step(0.985, star) * (sin(t * 3.0 + star * 100.0) * 0.5 + 0.5);
    bg.rgb += vec3(star * 0.4);

    // 비네팅
    float vignette = smoothstep(0.9, 0.35, dist);
    bg.rgb *= mix(0.35, 1.0, vignette);

    COLOR = bg;
}
