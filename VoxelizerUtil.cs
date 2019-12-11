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
        int[] triangles = GenerateTriangles(colorBuffer, vertices, width);

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);


        //mesh.Optimize();
        
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

    private static int[] GenerateTriangles(Color32[] colorBuffer, List<Vector3> vertices, int width)
    {
        List<int> triangles = new List<int>();
        width = 2*(width);

        foreach (var item in vertices)
        {
            Debug.Log(item.ToString());
        }

        for (int i = 0; i < 2*colorBuffer.Length; i+=2)
        {
            if (colorBuffer[i/2].a != 0)
            {
                triangles.Add(i + 1);
                triangles.Add(i);
                triangles.Add(i + 2);

                triangles.Add(i + 2);
                triangles.Add(i + 3);
                triangles.Add(i + 1);

                //triangles.Add(i + 2); 
                //triangles.Add(i);
                //triangles.Add(i + width);

                //triangles.Add(i + 2);
                //triangles.Add(i + width);
                //triangles.Add(i + width + 2);

                //triangles.Add(i + width + 2);
                //triangles.Add(i + width);
                //triangles.Add(i + width + 3);

                //triangles.Add(i + width + 3);
                //triangles.Add(i + width);
                //triangles.Add(i + width + 1);

                //triangles.Add(i + 1);
                //triangles.Add(i + width + 1);
                //triangles.Add(i + width);

                //triangles.Add(i + width);
                //triangles.Add(i);
                //triangles.Add(i + 1);

                //triangles.Add(i + 1);
                //triangles.Add(i);
                //triangles.Add(i + 2);

                //triangles.Add(i + 2);
                //triangles.Add(i + 3);
                //triangles.Add(i + 1);

                //triangles.Add(i + 1);
                //triangles.Add(i + 3);
                //triangles.Add(i + width + 3);

                //triangles.Add(i + width + 3);
                //triangles.Add(i + width + 2);
                //triangles.Add(i + 1);

                //triangles.Add(i + width + 3);
                //triangles.Add(i + 3);
                //triangles.Add(i + width + 2);

                //triangles.Add(i + width + 2);
                //triangles.Add(i + 3);
                //triangles.Add(i + 2);
            }
        }
        

        //triangles.Add(1);
        //triangles.Add(0);
        //triangles.Add(2);

        //triangles.Add(1);
        //triangles.Add(2);
        //triangles.Add(3);

        //triangles.Add(3);
        //triangles.Add(2);
        //triangles.Add(7);

        //triangles.Add(7);
        //triangles.Add(2);
        //triangles.Add(6);

        //triangles.Add(4);
        //triangles.Add(6);
        //triangles.Add(2);

        //triangles.Add(2);
        //triangles.Add(0);
        //triangles.Add(4);

        //triangles.Add(4);
        //triangles.Add(0);
        //triangles.Add(1);

        //triangles.Add(1);
        //triangles.Add(5);
        //triangles.Add(4);

        //triangles.Add(4);
        //triangles.Add(5);
        //triangles.Add(7);

        //triangles.Add(7);
        //triangles.Add(6);
        //triangles.Add(4);

        //triangles.Add(7);
        //triangles.Add(5);
        //triangles.Add(3);

        //triangles.Add(3);
        //triangles.Add(5);
        //triangles.Add(1);


        return triangles.ToArray();
    }
}
