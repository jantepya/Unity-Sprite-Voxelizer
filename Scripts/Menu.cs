using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Voxelizer
{
    public class Menu : EditorWindow
    {
        [MenuItem("Window/Voxelize Sprite")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(Voxelizer.Menu));
        }

        private Sprite _sprite;
        private Material _material;
        private Shader _shader;
        private bool _useMeshOptimizer;

        void OnGUI()
        {
            _sprite = (Sprite)EditorGUILayout.ObjectField("Selected Sprite", _sprite, typeof(Sprite), true);

            string debugText = null;

            if (_sprite == null)
            {
                GUI.enabled = false;
                debugText = "Need to Select a Sprite";
            }
            else
            {
                if (_sprite.texture.isReadable == false)
                {
                    GUI.enabled = false;
                    debugText = "Read/Write needs to be enabled in the sprite's Import Settings";
                }
                else if (_sprite.texture.format != TextureFormat.RGBA32)
                {   
                    debugText = "For best results, set sprite compression format to RGBA32 before converting";
                }
            }

            _material = (Material)EditorGUILayout.ObjectField("Selected Sprite", _material, typeof(Material), true);
            EditorGUILayout.Space();


            GUILayout.BeginVertical("HelpBox");
            _useMeshOptimizer = EditorGUILayout.Toggle("Use Mesh Optimizer", _useMeshOptimizer);
            EditorGUILayout.HelpBox("Unity's mesh optimizer optimezes the Mesh data to improve rendering performance. This operation can take a few seconds or more for complex meshes", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.Space();


            if (GUILayout.Button("Create"))
            {
                VoxelizeSprite(_sprite, _material, _useMeshOptimizer);
            }
            GUI.enabled = true;

            if (debugText != null)
            {
                EditorGUILayout.HelpBox(debugText, MessageType.Warning);
            }
        }

        /*
        * Create a 3D voxel GameObject from a texture
        */
        public static void VoxelizeSprite(Sprite sprite, Material material, bool useMeshOptimizer = false)
        {
            var timer = Stopwatch.StartNew();

            Mesh mesh = Voxelizer.Util.VoxelizeTexture2D(sprite.texture);

            if (useMeshOptimizer)
            {
                MeshUtility.Optimize(mesh);
            }

            timer.Stop();

            string meshName = sprite.name + "_3D";

            Debug.Log($"[VoxelizeSprite] {meshName}: Mesh created after {timer.ElapsedMilliseconds} milliseconds");

            var sprite3D = new GameObject(sprite.name + "_3D");
            
            var meshFilter = sprite3D.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = sprite3D.AddComponent<MeshRenderer>();
            meshRenderer.material = material == null ? Resources.Load<Material>("Materials/3DSpriteMaterial") : material;
        } 
    }
}
