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
                o.viewDir = normalize(v.vertex.xyz);

                return o;
            }

            float random3(float3 p)
            {
                return frac(
                    sin(dot(p, float3(12.9898, 78.233, 37.719))) 
                    * 43758.5453123
                );
            }

            float noise3(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);

                // smooth interpolation curve
                float3 u = f * f * (3.0 - 2.0 * f);

                // 8 corners of the cube
                float n000 = random3(i + float3(0,0,0));
                float n100 = random3(i + float3(1,0,0));
                float n010 = random3(i + float3(0,1,0));
                float n110 = random3(i + float3(1,1,0));
                float n001 = random3(i + float3(0,0,1));
                float n101 = random3(i + float3(1,0,1));
                float n011 = random3(i + float3(0,1,1));
                float n111 = random3(i + float3(1,1,1));

                // trilinear interpolation
                float nx00 = lerp(n000, n100, u.x);
                float nx10 = lerp(n010, n110, u.x);
                float nx01 = lerp(n001, n101, u.x);
                float nx11 = lerp(n011, n111, u.x);

                float nxy0 = lerp(nx00, nx10, u.y);
                float nxy1 = lerp(nx01, nx11, u.y);

                return lerp(nxy0, nxy1, u.z);
            }

            float fbm3(float3 p)
            {
                float value = 0.0;
                float amplitude = 0.5;

                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise3(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            float3 hash3(float3 p)
            {
                return frac(sin(float3(
                    dot(p, float3(127.1,311.7, 74.7)),
                    dot(p, float3(269.5,183.3,246.1)),
                    dot(p, float3(113.5,271.9,124.6))
                )) * 43758.5453);
            }

            float3 offset_voronoi_feature(in float3 feature_position) {
                float timeScaled = _Time;
                return 0.3 * sin(timeScaled + 6.2831 * feature_position);
            }

            float4 voronoi3d(float3 p)
            {
                float3 base = floor(p);
                float3 local = frac(p);

                //----------------------------------
                // PASS 1: closest feature point
                //----------------------------------

                float3 closestCell;
                float3 vecToClosest;

                float minDist = 10.0;

                [unroll]
                for(int z=-1; z<=1; z++)
                [unroll]
                for(int y=-1; y<=1; y++)
                [unroll]
                for(int x=-1; x<=1; x++)
                {
                    float3 cell = float3(x,y,z);

                    float3 feature = hash3(base + cell);

                    feature = offset_voronoi_feature(feature);

                    float3 diff = cell + feature - local;

                    float d = dot(diff,diff);

                    if(d < minDist)
                    {
                        minDist = d;
                        vecToClosest = diff;
                        closestCell = cell;
                    }
                }

                //----------------------------------
                // PASS 2: distance to edge
                //----------------------------------

                float edgeDist = 10.0;

                [unroll]
                for(int k=-2; k<=2; k++)
                [unroll]
                for(int j=-2; j<=2; j++)
                [unroll]
                for(int i=-2; i<=2; i++)
                {
                    float3 cell = closestCell + float3(i,j,k);

                    float3 feature = hash3(base + cell);
                    feature = offset_voronoi_feature(feature);

                    float3 diff = cell + feature - local;

                    float3 delta = vecToClosest - diff;

                    if(dot(delta,delta) > 0.00001)
                    {
                        float d = dot(
                            0.5 * (vecToClosest + diff),
                            normalize(diff - vecToClosest)
                        );

                        edgeDist = min(edgeDist, d);
                    }
                }

                return float4(edgeDist, vecToClosest);
            }

            float ball(in float radius, in float d) {
                return smoothstep(radius + (radius / 5.0), radius, d);
            }

            float ball_outline(in float radius, in float d) {
                return ball(radius, d) - ball(radius - (radius / 3.5), d);
            }

            float bowling_ball(in float radius, float3 to_feature) {
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

            // TODO:
            // 1. Create greater depth in the skybox (currently: working on shapingn function for depth_scale)
            // 2. Fix the depth artifacts 
            // 3. Create sharper colors
            float4 frag (v2f i) : SV_Target {
                float3 dir = i.viewDir;    

                // texcoord distortion
                float3 dir_distort = float3(fbm3(dir + float3(23., 38., 90)),
                                          fbm3(dir + float3(-30, 508, 304)),
                                          fbm3(dir));
                                    
                float3 dir_distort_2 = float3(fbm3(dir + _DistortionFactor * dir_distort + float3(133, 100, 12)), 
                                            fbm3(dir + _DistortionFactor * dir_distort + float3(321, 230, -29)),
                                            fbm3(dir + _DistortionFactor * dir_distort));
                
                // voronoi gradients
                float depth_scale = (1.0 - pow(abs(dir.y / 25), 2)) * _Scale;
                float3 domain = dir_distort_2 * depth_scale;
                float3 cell_id = floor(domain + float3(0.5, 0.5, 0.5));
                
                float4 base_map = voronoi3d(domain); // B&W map from voronoi noise
                float edge_distance = base_map.x;
                float3 to_feature = base_map.yzw;
                
                // color bands
                float band_frequency = 32.0;
                float3 voronoi_bands = edge_distance * (2. + 0.5 * sin(band_frequency * edge_distance)) * float3(1.0, 1.0, 1.0); 
                float3 voronoi_hard_bands = floor((voronoi_bands + .5) * 4.) / 4.; // turn band gradients into hard lines
                float3 outputColor = voronoi_hard_bands * _BandColor; // base band color

                if(random3(cell_id) > 0.5) {
                    float feature = bowling_ball(0.1, to_feature);
                    feature = floor(feature); // Flooring creates sharper lines
                    outputColor = lerp(outputColor, _BorderColor, feature); 
                } else {
                    float feature = bowling_pin(.2, to_feature);
                    feature = floor(feature + 0.5);
                    outputColor = lerp(outputColor, _BorderColor, feature);
                }

                // borders	
                float band_width = 0.05;
                outputColor = lerp( _BorderColor, outputColor, smoothstep( band_width, band_width + 0.01, edge_distance ) );

                return float4(outputColor, 1.0);
            }

            ENDCG
        }
    }
}
