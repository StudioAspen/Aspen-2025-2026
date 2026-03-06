// TODO: I've set the camera background to Skybox, but now the skybox is showing on the level enter transition. 
// I've started mapping the skybox to spherical coords, but theres a big ass seam.
Shader "Custom/VoronoiSkybox"
{
    Properties
    {
        _BorderColor ("Border Color", Color) = (0.4, 0.075, 0.055, 1.0)
        _BandColor ("Band Color", Color) = (0.694, 0.475, 0.749, 1.0)
        _DistortionFactor ("Distortion Factor", Float) = 1.0
        _Scale ("Scale", Float) = 10.0
    }

    SubShader
    {
        Tags { 
            "Queue"="Background"
            "RenderType"="Background"
            "Preview"="Skybox"
        }

        LOD 100

        Pass
        {
            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)
                float4 _BorderColor;
                float4 _BandColor;
                float _DistortionFactor; 
                float _Scale;
            CBUFFER_END

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                // For skybox cube, object space position IS direction
                o.viewDir = normalize(v.vertex.xyz);

                return o;
            }

            float random(in float2 st) {
                return frac(
                    sin(
                        dot(st.xy, float2(12.9898,78.233))
                    ) * 43758.5453123
                );
            }

            // Based on Morgan McGuire @morgan3d https://www.shadertoy.com/view/4dS3Wd
            float noise(in float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            float fbm (in float2 st) {
                // Initial values
                float value = 0.0;
                float amplitude = .5;
                float frequency = 0.;
                //
                // Loop of octaves
                for (int i = 0; i < 4; i++) {
                    value += amplitude * noise(st);
                    st *= 2.;
                    amplitude *= .5;
                }
                return value;
            }

            float2 hash2(float2 p) {
                // Procedural white noise
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            float2 offset_voronoi_feature(in float2 feature_position) {
                float timeScaled = _Time * 0.2;
                return 0.1 * sin(timeScaled + 6.2831 * feature_position);
            }


            // This is from Inigo Quilez — just renamed variables to nicer names
            float3 voronoi(float2 position) {
                // Integer grid cell containing the point
                float2 cellBase = floor(position);

                // Local coordinates inside the cell (0–1 range)
                float2 localPos = frac(position);

                //----------------------------------
                // PASS 1: Find closest feature point
                //----------------------------------

                float2 closestCellOffset;     // Which neighboring cell had the closest point
                float2 vectorToClosestPoint;  // Vector from pixel to closest feature point

                float minDistSquared = 8.0;   // Large initial distance

                // Check 3x3 neighborhood of cells
                [unroll]
                for (int y = -1; y <= 1; y++)
                [unroll]
                for (int x = -1; x <= 1; x++)
                {
                    float2 neighborOffset = float2((float)x, (float)y);

                    // Random feature point inside this neighbor cell
                    float2 featureOffset = hash2(cellBase + neighborOffset);

                    featureOffset = offset_voronoi_feature(featureOffset);

                    // Vector from current pixel to feature point
                    float2 diff = neighborOffset + featureOffset - localPos;

                    float distSquared = dot(diff, diff);

                    if (distSquared < minDistSquared)
                    {
                        minDistSquared    = distSquared;
                        vectorToClosestPoint = diff;
                        closestCellOffset    = neighborOffset;
                    }
                }

                //----------------------------------
                // PASS 2: Distance to Voronoi edge
                //----------------------------------

                float minEdgeDistance = 8.0;

                // Check larger neighborhood for border distances
                [unroll]
                for (int j = -2; j <= 2; j++)
                [unroll]
                for (int i = -2; i <= 2; i++)
                {
                    float2 neighborOffset = closestCellOffset + float2((float)i, (float)j);

                    float2 featureOffset = hash2(cellBase + neighborOffset);

                    featureOffset = offset_voronoi_feature(featureOffset);

                    float2 diff = neighborOffset + featureOffset - localPos;

                    // Ignore the same feature point
                    float2 delta = vectorToClosestPoint - diff;
                    if (dot(delta, delta) > 0.00001)
                    {
                        // Distance from pixel to border between closest cell and this one
                        float edgeDistance =
                            dot(0.5 * (vectorToClosestPoint + diff),
                                normalize(diff - vectorToClosestPoint));

                        minEdgeDistance = min(minEdgeDistance, edgeDistance);
                    }
                }

                // Return:
                // x = distance to edge
                // y,z = vector to closest feature point
                return float3(minEdgeDistance, vectorToClosestPoint);
            }

            float ball(in float radius, in float d) {
                return smoothstep(radius + (radius / 5.0), radius, d);
            }

            float ball_outline(in float radius, in float d) {
                return ball(radius, d) - ball(radius - (radius / 3.5), d);
            }

            float bowling_ball(in float radius, float2 to_feature) {
                float d = length(to_feature);
                float bowling_ball = ball_outline(radius, d);

                float hole1 = length(to_feature - float2(0.02,  0.05));
                float hole2 = length(to_feature - float2(-0.02, 0.02));
                float hole3 = length(to_feature - float2(0.00,  0.08));

                float mini_radius = radius / 5.0;
                bowling_ball += ball(mini_radius, hole1);
                bowling_ball += ball(mini_radius, hole2);
                bowling_ball += ball(mini_radius, hole3);

                return bowling_ball;
            }

            float sdEllipse(in float2 p, in float2 ab) {
                p = abs(p);
                if (p.x > p.y) { p = p.yx; ab = ab.yx; }

                float l  = ab.y * ab.y - ab.x * ab.x;
                float m  = ab.x * p.x / l;   float m2 = m  * m;
                float n  = ab.y * p.y / l;   float n2 = n  * n;
                float c  = (m2 + n2 - 1.0) / 3.0;
                float c3 = c * c * c;
                float q  = c3 + m2 * n2 * 2.0;
                float d  = c3 + m2 * n2;
                float g  = m  + m  * n2;

                float co;
                if (d < 0.0)
                {
                    float h  = acos(q / c3) / 3.0;
                    float s  = cos(h);
                    float t  = sin(h) * sqrt(3.0);
                    float rx = sqrt(-c * (s + t + 2.0) + m2);
                    float ry = sqrt(-c * (s - t + 2.0) + m2);
                    co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
                }
                else
                {
                    float h  = 2.0 * m * n * sqrt(d);
                    float s  = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
                    float u  = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
                    float rx = -s - u - c * 4.0 + 2.0 * m2;
                    float ry = (s - u) * sqrt(3.0);
                    float rm = sqrt(rx * rx + ry * ry);
                    co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
                }

                float2 r = ab * float2(co, sqrt(1.0 - co * co));
                return length(r - p) * sign(p.y - r.y);
            }

            float ellipse_outline(in float2 p, in float2 ab) {
                return smoothstep(0.02, 0.0, sdEllipse(p, ab))
                    - smoothstep(0.02, 0.0, sdEllipse(p, ab * float2(0.8, 0.8)));
            }


            float bowling_pin(in float radius, float2 to_feature) {
                float d = ellipse_outline(to_feature, float2(radius, radius / 2.0));
                d += ball_outline(radius / 2.0, length(to_feature - float2(radius, 0.0)));
                d += smoothstep(0.01, 0.0, sdEllipse(to_feature - float2(radius - radius / 4.0, 0.0),
                                                    float2(radius / 4.0, radius / 2.0)));
                return d;
            }

            float2 cartToPolar(float3 coord) {
                float u = atan2(coord.x, coord.z) / (UNITY_PI * 2);
                float v = asin(coord.y) / (UNITY_PI / 2);
                return float2(u, v);
            }
            float4 frag (v2f i) : SV_Target {
                float2 st = cartToPolar(i.viewDir);
                
                // texcoord distortion
                float2 basic_fbm = float2(fbm(st + float2(23., 38.)),
                                    fbm(st));
                                    
                float2 basic_fbm_2 = float2(fbm(st + _DistortionFactor * basic_fbm + float2(133, 100)), 
                                        fbm(st + _DistortionFactor * basic_fbm + float2(321, 230)));
                
                // voronoi gradients
                float2 domain =  float2(_Scale, _Scale) * basic_fbm_2;
                // float2 domain =  float2(_Scale, _Scale) * st;
                float2 cell_id = floor(domain + float2(0.5, 0.5));
                
                float3 base_map = voronoi(domain); // B&W map from voronoi noise
                float edge_distance = base_map.x;
                float2 to_feature = base_map.yz;
                    

                float band_frequency = 32.0;
                
                // color bands
                float3 col = edge_distance * (2. + 0.5 * sin(band_frequency * edge_distance)) * float3(1.0, 1.0, 1.0); 
                col = floor((col + .5) * 4.) / 4.; // turn band gradients into hard lines
                col *= _BandColor; // base band color
                
                // shapes at voronoi feature points    
                if(random(cell_id) > 0.5) {
                    float b_b = bowling_ball(0.1, to_feature);
                    b_b = floor(b_b);
                    col = lerp(col, _BorderColor,b_b); 
                } else {
                    float bp = bowling_pin(.2, to_feature);
                    bp = floor(bp + 0.5);
                    col = lerp(col, _BorderColor, bp);
                }
                
                return float4(col, 1.0);
            }

            ENDCG
        }
    }
}
