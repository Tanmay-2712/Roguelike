shader_type canvas_item;

uniform float noise_scale_1 = 10.0;
uniform float noise_scale_2 = 20.0;
uniform float noise_strength = 0.5;
uniform float brightness_base = 1.0;
uniform float contrast = 1.0;
uniform vec2 noise_offset = vec2(0.0, 0.0);
uniform float speed = 0.1;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float tint_strength = 0.2;
uniform float edge_sharpness = 1.0;

// Simple 2D noise function
float noise(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

// 2D Perlin noise
float perlin_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    float a = noise(i);
    float b = noise(i + vec2(1.0, 0.0));
    float c = noise(i + vec2(0.0, 1.0));
    float d = noise(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Fractal Brownian Motion (fBm)
float fbm(vec2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    for (int i = 0; i < 5; i++) {
        value += amplitude * perlin_noise(st * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void fragment() {
    // Calculate time-based offset
    vec2 time_offset = noise_offset + vec2(TIME * speed, TIME * speed);
    
    // Calculate two layers of noise
    float noise1 = fbm((UV + time_offset) * noise_scale_1);
    float noise2 = fbm((UV + time_offset * 1.5) * noise_scale_2);
    
    // Combine noise layers
    float combined_noise = (noise1 + noise2) * 0.5;
    
    // Apply edge sharpness
    combined_noise = pow(combined_noise, edge_sharpness);
    
    // Calculate brightness
    float brightness = brightness_base + (combined_noise - 0.5) * noise_strength;
    
    // Apply contrast
    brightness = (brightness - 0.5) * contrast + 0.5;
    
    // Clamp brightness
    brightness = clamp(brightness, 0.0, 1.0);
    
    // Get original color and apply brightness
    vec4 color = texture(TEXTURE, UV);
    color.rgb *= brightness;
    
    // Apply tint
    color.rgb = mix(color.rgb, tint_color.rgb, tint_strength * combined_noise);
    
    COLOR = color;
}