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

        Mesh mesh = new Mesh();

        List<Vector3> vertices = GenerateVertices(height, width);
        mesh.SetVertices(vertices);


        int[] triangles = GenerateTriangles(colorBuffer, width);
        mesh.SetTriangles(triangles, 0);

        //foreach (var v in vertices)
        //{
        //    Debug.Log(v.ToString());
        //}

        mesh.Optimize();

        //List<Vector3> normals = GenerateNormals(colorBuffer, width);
        //mesh.SetNormals(normals);


        //List<Color32> vertexColors = GenerateColors(colorBuffer, height, width);
        //mesh.SetColors(vertexColors);


        return mesh;
    }


    /*
     * 
     */
    private static List<Vector3> GenerateVertices(int height, int width)
    {
        List<Vector3> vertices = new List<Vector3>(24*(height * width));

        float s = 1f;

        for (int i = height-1; i >= 0 ; i--)
        {
            float y = -i * s;
            for (int j = 0; j < width; j++)
            {
                float x = j * s;

                Vector3[] cube = new Vector3[8];

                // bottom
                cube[0] = new Vector3(x, y, s);
                cube[1] = new Vector3(x + s, y, s);
                cube[2] = new Vector3(x + s, y, -s);
                cube[3] = new Vector3(x, y, -s);

                // top
                cube[4] = new Vector3(x, y + s, s);
                cube[5] = new Vector3(x + s, y + s, s);
                cube[6] = new Vector3(x + s, y + s, -s);
                cube[7] = new Vector3(x, y + s, -s);

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

    private static int[] GenerateTriangles(Color32[] colorBuffer, int width)
    {
        // triangle values are indices of vertices array
        List<int> triangles = new List<int>();

        //int offset = 2*(width + 1);
        //int pad = 0;
        //int row = 2 * width;

        // colorbuffer pixels are laid out left to right, 
        // bottom to top (i.e. row after row)
        for (int i = 0; i < 24*colorBuffer.Length; i+=24)
        {
            if (colorBuffer[i/24].a != 0)
            {
                Debug.Log(i);
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

                //// Bottom 
                //triangles.Add(i + 3);
                //triangles.Add(i + 1);
                //triangles.Add(i);

                //triangles.Add(i + 3);
                //triangles.Add(i + 2);
                //triangles.Add(i + 1);

                //// Top
                //triangles.Add(i + offset + 2);
                //triangles.Add(i + offset);
                //triangles.Add(i + offset + 3);

                //triangles.Add(i + offset + 3);
                //triangles.Add(i + offset);
                //triangles.Add(i + offset + 1);

                //// Front
                //triangles.Add(i + 2);
                //triangles.Add(i);
                //triangles.Add(i + offset);

                //triangles.Add(i + 2);
                //triangles.Add(i + offset);
                //triangles.Add(i + offset + 2);

                //// Left
                //triangles.Add(i + 1);
                //triangles.Add(i + offset + 1);
                //triangles.Add(i + offset);

                //triangles.Add(i + offset);
                //triangles.Add(i);
                //triangles.Add(i + 1);

                //// Back
                //triangles.Add(i + 1);
                //triangles.Add(i + 3);
                //triangles.Add(i + offset + 3);

                //triangles.Add(i + offset + 3);
                //triangles.Add(i + offset + 1);
                //triangles.Add(i + 1);

                //// Right
                //triangles.Add(i + offset + 3);
                //triangles.Add(i + 3);
                //triangles.Add(i + offset + 2);

                //triangles.Add(i + offset + 2);
                //triangles.Add(i + 3);
                //triangles.Add(i + 2);
            }
        }
        
        return triangles.ToArray();
    }

    private static List<Vector3> GenerateNormals(Color32[] colorBuffer, int width)
    {
        List<Vector3> normals = new List<Vector3>();

        int offset = 2 * (width + 1);
        int pad = 0;
        int row = 2 * width;

        // colorbuffer pixels are laid out left to right, 
        // bottom to top (i.e. row after row)
        for (int j = 0; j < 2 * colorBuffer.Length; j += 2)
        {
            // ignore right-most column of vertices
            if (j > 0 && (j + pad) % row == 0)
            {
                // index of last columns -> 2w + n(2w+2)
                row += ((2 * width) + 2);
                pad += 2;
            }

            if (colorBuffer[j / 2].a != 0)
            {
                int i = j + pad;

                // Bottom 
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));

                // Top
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));

                // Front
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));

                // Left
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));

                // Back
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));

                // Right
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));
            }
        }

        return normals;
    }

    private static List<Color32> GenerateColors(Color32[] colorBuffer, int height, int width)
    {
        List<Color32> vertexColors = new List<Color32>();
        for (int i = height - 1; i >= 0; i--)
        {
            for (int j = 0; j < width; j++)
            {
                if (colorBuffer[j + i * width].a != 0)
                {
                    vertexColors.Add(colorBuffer[j + i * width]);
                    vertexColors.Add(colorBuffer[j + i * width]);

                    vertexColors.Add(colorBuffer[j + i * width]);
                    vertexColors.Add(colorBuffer[j + i * width]);

                    vertexColors.Add(colorBuffer[j + i * width]);
                    vertexColors.Add(colorBuffer[j + i * width]);
                }
            }
        }

        return vertexColors;
    }

}
