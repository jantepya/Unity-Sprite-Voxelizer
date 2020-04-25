using System.Collections.Generic;
using UnityEngine;

public class VoxelizerUtil
{
    /*
     * Create a 3D voxel GameObject from a 2d sprite
     */
    public static void VoxelizeSprite(Sprite sprite)
    {
        var mesh = VoxelizeTexture2D(sprite.texture);

        var sprite3D = new GameObject(sprite.name + " 3D");
        
        var meshFilter = sprite3D.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var meshRenderer = sprite3D.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/3DSpriteMaterial");
    }

    /*
     * Create a Mesh object from a Texture2D object
     */
    public static Mesh VoxelizeTexture2D(Texture2D texture)
    {
        texture.filterMode = FilterMode.Point;

        var textureFormat = texture.format;
        if (textureFormat != TextureFormat.RGBA32)
        {
            Debug.LogWarning("For best results, set sprite format to RGBA32 from Import Settings");
        }

        int height = texture.height;
        int width = texture.width;
        Color32[] colorBuffer = texture.GetPixels32();

        var mesh = new Mesh();

        var vertices = GenerateVertices(height, width);
        mesh.SetVertices(vertices);

        var normals = GenerateNormals(height, width);
        mesh.SetNormals(normals);

        var vertexColors = GenerateColors(colorBuffer, height, width);
        mesh.SetColors(vertexColors);

        var triangles = GenerateTriangles(colorBuffer, width);
        mesh.SetTriangles(triangles, 0);

        return mesh;
    }


    /*
     * Generate 24 vertices cube for every pixel in the texture
     */
    private static List<Vector3> GenerateVertices(int height, int width)
    {
        List<Vector3> vertices = new List<Vector3>(24*(height * width));

        float scale = 1f;

        for (int i = height-1; i >= 0 ; i--)
        {
            float y = -i * scale;
            for (int j = 0; j < width; j++)
            {
                float x = j * scale;

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

        return vertices;
    }

    private static int[] GenerateTriangles(IList<Color32> colorBuffer, int width)
    {
        // triangle values are indices of vertices array
        List<int> triangles = new List<int>(colorBuffer.Count);

        // colorbuffer pixels are laid out left to right, 
        // bottom to top (i.e. row after row)
        for (int i = 0; i < 24*colorBuffer.Count; i+=24)
        {
            if (colorBuffer[i/24].a != 0)
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
            }
        }
        
        return triangles.ToArray();
    }

    private static List<Vector3> GenerateNormals(int height, int width)
    {
        List<Vector3> normals = new List<Vector3>();

        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 forward = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        for (int i = height - 1; i >= 0; i--)
        {
            for (int j = 0; j < width; j++)
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
        }

        return normals;
    }

    private static List<Color32> GenerateColors(IList<Color32> colorBuffer, int height, int width)
    {
        List<Color32> vertexColors = new List<Color32>(24 * (height * width));
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Color32 c = colorBuffer[j + i * width];
                for (int k = 0; k < 24; k++)
                {
                    vertexColors.Add(c);
                }
            }
        }

        return vertexColors;
    }

}
