using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Voxelizer
{
    public static class VoxelUtil
    {
        private const int CUBE_INDICES_COUNT = 24;

        /// <summary>
        /// Create a Mesh object from a Texture2D object
        /// </summary>
        public static Mesh VoxelizeTexture2D(Texture2D texture, bool applyColorPerVertex = false, float scale = 1.0f)
        {
            texture.filterMode = FilterMode.Point;

            if (texture.format != TextureFormat.RGBA32)
            {
                Debug.LogWarning("For best results, set sprite format to RGBA32 from Import Settings");
            }

            int height = texture.height;
            int width = texture.width;
            Color32[] colorBuffer = texture.GetPixels32();

            var mesh = new Mesh();

            GenerateVertices(ref mesh, colorBuffer, height, width, scale);
            GenerateNormals(ref mesh);

            if (mesh.vertexCount >= Int16.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            GenerateTriangles(ref mesh, colorBuffer);

            if (applyColorPerVertex)
            {
                GenerateVertexColors(ref mesh, colorBuffer);
            }

            return mesh;
        }

        /// <summary>
        /// Generate 24 vertices cube for every pixel in the texture
        /// </summary>
        private static void GenerateVertices(ref Mesh mesh, IList<Color32> colorBuffer, int height, int width, float scale = 1.0f)
        {
            if (mesh == null || colorBuffer == null) return;
            
            var vertices = new List<Vector3>(CUBE_INDICES_COUNT * height * width);

            float xStartPosition = -(width * scale / 2f);
            float yStartPosition = -(height * scale / 2f);

            for (int i = 0; i < height; i++)
            {
                float y = yStartPosition + (i * scale);
                
                for (int j = 0; j < width; j++)
                {
                    if (colorBuffer[i * width + j].a == 0)
                        continue;
                    
                    float x = xStartPosition + (j * scale);

                    Vector3[] cube = new Vector3[8];

                    // bottom
                    cube[0] = new Vector3(x, y, scale);
                    cube[1] = new Vector3(x + scale, y, scale);
                    cube[2] = new Vector3(x + scale, y, -scale);
                    cube[3] = new Vector3(x, y, -scale);

                    // top
                    cube[4] = new Vector3(x, y + scale, scale);
                    cube[5] = new Vector3(x + scale, y + scale, scale);
                    cube[6] = new Vector3(x + scale, y + scale, -scale);
                    cube[7] = new Vector3(x, y + scale, -scale);

                    vertices.AddRange(new List<Vector3>
                    {
                        cube[0], cube[1], cube[2], cube[3], // Bottom
                        cube[7], cube[4], cube[0], cube[3], // Left
                        cube[4], cube[5], cube[1], cube[0], // Front
                        cube[6], cube[7], cube[3], cube[2], // Back
                        cube[5], cube[6], cube[2], cube[1], // Right
                        cube[7], cube[6], cube[5], cube[4]  // Top
                    });
                }
            }

            mesh.SetVertices(vertices);
        }

        private static void GenerateNormals(ref Mesh mesh)
        {
            if (mesh == null || mesh.vertexCount <= 0) return;

            var normals = new List<Vector3>(mesh.vertexCount);

            var up = Vector3.up;
            var down = Vector3.down;
            var forward = Vector3.forward;
            var back = Vector3.back;
            var left = Vector3.left;
            var right = Vector3.right;

            for (int j = 0; j < mesh.vertexCount; j += CUBE_INDICES_COUNT)
            {
                normals.AddRange(new List<Vector3>
                {
                    down, down, down, down,             // Bottom
                    left, left, left, left,             // Left
                    forward, forward, forward, forward,	// Front
                    back, back, back, back,             // Back
                    right, right, right, right,         // Right
                    up, up, up, up	                    // Top
                });
            }
            
            mesh.SetNormals(normals);
        }

        private static void GenerateTriangles(ref Mesh mesh, IList<Color32> colorBuffer)
        {
            if (mesh == null || colorBuffer == null) return;
            
            // triangle values are indices of vertices array
            var triangles = new List<int>( mesh.vertexCount);

            // colorbuffer pixels are laid out left to right, 
            // bottom to top (i.e. row after row)
            int i = 0;
            for (int j = 0; j < CUBE_INDICES_COUNT * colorBuffer.Count; j += CUBE_INDICES_COUNT)
            {
                if (colorBuffer[j / CUBE_INDICES_COUNT].a != 0)
                {
                    triangles.AddRange(new int[]
                    {
                        // Bottom
                        i + 3, i + 1, i,
                        i + 3, i + 2, i + 1,

                        // Left     	
                        i + 7, i + 5, i + 4,
                        i + 7, i + 6, i + 5,

                        // Front
                        i + 11, i + 9, i + 8,
                        i + 11, i + 10, i + 9,

                        // Back
                        i + 15, i + 13, i + 12,
                        i + 15, i + 14, i + 13,

                        // Right
                        i + 19, i + 17, i + 16,
                        i + 19, i + 18, i + 17,

                        // Top
                        i + 23, i + 21, i + 20,
                        i + 23, i + 22, i + 21,
                    });
                    i += CUBE_INDICES_COUNT;
                }
            }

            mesh.SetTriangles(triangles, 0);
        }

        /// <summary>
        /// Assigns color for each vertex
        /// </summary>
        private static void GenerateVertexColors(ref Mesh mesh, IList<Color32> colorBuffer)
        {
            if (mesh == null || colorBuffer == null) return;

            var vertexColors = new List<Color32>(CUBE_INDICES_COUNT * colorBuffer.Count);

            for (int i = 0; i < colorBuffer.Count; i++)
            {
                Color32 color = colorBuffer[i];

                if (color.a == 0) continue;

                for (int k = 0; k < CUBE_INDICES_COUNT; k++)
                {
                    vertexColors.Add(color);
                }
            }

            mesh.SetColors(vertexColors);
        }


        /// <summary>
        /// Generates a Texture Map and assigns the mesh UVs accordingly
        /// </summary>
        public static Texture2D GenerateTextureMap(ref Mesh mesh, Texture2D inputTexture)
        {
            if (mesh == null || inputTexture == null) return null;

            Color32[] colorBuffer = inputTexture.GetPixels32();
            var colorMap = new Dictionary<Color32, int>();

            for (int i = 0; i < colorBuffer.Length; i++)
            {
                Color32 color = colorBuffer[i];
                
                if (color.a != byte.MinValue && !colorMap.ContainsKey(color))
                {
                    colorMap.Add(color, colorMap.Count);
                }
            }
            
            var textureMap = new Texture2D(1, colorMap.Count);

            if (colorMap.Count == 0) return textureMap;

            Color32[] colors = new Color32[colorMap.Count];

            foreach (var color in colorMap)
            {
                colors[color.Value] = color.Key;
            }

            textureMap.SetPixels32(colors);

            var uvs = new List<Vector2>(mesh.vertexCount);
            float offset = 1f / (2f * colorMap.Count);

            for (int i = 0; i < colorBuffer.Length; i++)
            {
                Color32 color = colorBuffer[i];

                if (color.a == byte.MinValue || !colorMap.ContainsKey(color)) continue;

                int index = colorMap[color];
                float v = (float) index / (float) colorMap.Count;

                for (int k = 0; k < CUBE_INDICES_COUNT; k++)
                {
                    uvs.Add(new Vector2(0, v + offset));
                }
            }

            mesh.SetUVs(0, uvs);

            textureMap.filterMode = FilterMode.Point;
            textureMap.Apply();
            
            return textureMap;
        }
    }
}
