using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelizerUtil
{
    /*
     * Create a 3D voxel GameObject from a 2d sprite
     */
    public static void VoxelizeSprite(Sprite sprite)
    {
        Debug.Log(sprite.name);

        Mesh mesh = VoxelizeTexture2D(sprite.texture);

        GameObject sprite3D = new GameObject(sprite.name + " 3D");
        

        MeshFilter meshFilter = sprite3D.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        sprite3D.AddComponent<MeshRenderer>();
    }

    /*
     * Create a Mesh object from a Texture2D object
     */
    public static Mesh VoxelizeTexture2D(Texture2D texture)
    {
        int height = texture.height;
        int width = texture.width;
        Color32[] colorBuffer = texture.GetPixels32();

        List<Vector3> vertices = GenerateVertices(height, width);
        int[] triangles = GenerateTriangles(colorBuffer,  width);

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.Optimize();

        return mesh;
    }


    private static List<Vector3> GenerateVertices(int height, int width)
    {
        List<Vector3> vertices = new List<Vector3>(height * width);


        float s = 1f;

        for (int i = height; i >= 0 ; i--)
        {
            float y = -i * s;
            for (int j = 0; j <= width; j++)
            {
                float x = j * s;
                vertices.Add(new Vector3(x, y, -s)); // 0
                vertices.Add(new Vector3(x, y, s)); // 4
            }
        }

        return vertices;
    }

    private static int[] GenerateTriangles(Color32[] colorBuffer, int width)
    {
        // triangle values are indices of vertices array
        List<int> triangles = new List<int>();

        int offset = 2*(width + 1);
        int pad = 0;
        int row = 2 * width;

        // colorbuffer pixels are laid out left to right, 
        // bottom to top (i.e. row after row)
        for (int j = 0; j < 2*colorBuffer.Length; j+=2)
        {
            // ignore right-most column of vertices
            if (j > 0 && (j + pad) % row == 0)
            {
                // row = 2w + n(2w+2)
                row += ((2 * width) + 2);
                pad += 2;
            }

            if (colorBuffer[j/2].a != 0)
            {
                int i = j + pad;

                // Bottom 
                triangles.Add(i + 1);
                triangles.Add(i);
                triangles.Add(i + 2);

                triangles.Add(i + 2);
                triangles.Add(i + 3);
                triangles.Add(i + 1);

                // Top
                triangles.Add(i + offset + 2);
                triangles.Add(i + offset);
                triangles.Add(i + offset + 3);

                triangles.Add(i + offset + 3);
                triangles.Add(i + offset);
                triangles.Add(i + offset + 1);

                // Front
                triangles.Add(i + 2);
                triangles.Add(i);
                triangles.Add(i + offset);

                triangles.Add(i + 2);
                triangles.Add(i + offset);
                triangles.Add(i + offset + 2);

                // Left
                triangles.Add(i + 1);
                triangles.Add(i + offset + 1);
                triangles.Add(i + offset);

                triangles.Add(i + offset);
                triangles.Add(i);
                triangles.Add(i + 1);

                // Back
                triangles.Add(i + 1);
                triangles.Add(i + 3);
                triangles.Add(i + offset + 3);

                triangles.Add(i + offset + 3);
                triangles.Add(i + offset + 1);
                triangles.Add(i + 1);

                // Right
                triangles.Add(i + offset + 3);
                triangles.Add(i + 3);
                triangles.Add(i + offset + 2);

                triangles.Add(i + offset + 2);
                triangles.Add(i + 3);
                triangles.Add(i + 2);
            }
        }
        
        return triangles.ToArray();
    }
}
