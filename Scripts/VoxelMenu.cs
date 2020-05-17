#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Voxelizer
{
    public class VoxelMenu : EditorWindow
    {
        private const string VOXEL_NAME_POST_FIX = "_3D";

        [MenuItem("Window/Voxelize Sprite")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<VoxelMenu>("Voxelizer Menu");
        }

        private Sprite _sprite;
        private float _scale = 1.0f;
        private bool _useMeshOptimizer = true;
        private bool _saveMesh;
        private bool _saveTexture;
        private bool _applyColorPerVertex;
        private bool _createNewGameObject = true;

        private void OnGUI()
        {
            _sprite = (Sprite)EditorGUILayout.ObjectField("Selected Sprite", _sprite, typeof(Sprite), true);

            string debugText = null;

            if (_sprite == null)
            {
                GUI.enabled = false;
                debugText = "Need to Select a Sprite!";
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

            EditorGUILayout.Space();

            _scale = (float)EditorGUILayout.FloatField("Scale", _scale);

            GUILayout.BeginVertical("HelpBox");
            _useMeshOptimizer = EditorGUILayout.Toggle("Use Mesh Optimizer", _useMeshOptimizer);
            EditorGUILayout.HelpBox("Unity's mesh optimizer optimizes the Mesh data to improve rendering performance. This operation can take a few seconds or more for complex meshes", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.Space();


            _saveMesh = EditorGUILayout.Toggle("Save Mesh To File", _saveMesh);
            _saveTexture = EditorGUILayout.Toggle("Save Texture To File", _saveTexture);
            _applyColorPerVertex = EditorGUILayout.Toggle("Apply Per-vertex Colors", _applyColorPerVertex);
            _createNewGameObject = EditorGUILayout.Toggle("Add Mesh to Scene", _createNewGameObject);
            EditorGUILayout.Space();


            if (_createNewGameObject == false && _saveMesh == false)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Create"))
            {
                VoxelizeSprite();
            }
            GUI.enabled = true;

            if (debugText != null)
            {
                EditorGUILayout.HelpBox(debugText, MessageType.Warning);
            }
        }

        private void VoxelizeSprite()
        {
            /////////////////////
            var timer = Stopwatch.StartNew();

            Mesh mesh = VoxelUtil.VoxelizeTexture2D(_sprite.texture, _applyColorPerVertex, _scale);

            Texture2D texture = VoxelUtil.GenerateTextureMap(ref mesh, _sprite.texture );

            if (_useMeshOptimizer)
            {
                MeshUtility.Optimize(mesh);
            }

            timer.Stop();
            ///////////////////////

            string meshName = _sprite.name + VOXEL_NAME_POST_FIX;
            mesh.name = meshName;
            Debug.Log(string.Format("[Voxelizer] {0}: Mesh created after {1} milliseconds", meshName, timer.ElapsedMilliseconds));

            if (_createNewGameObject)
            {
                CreateVoxelGameObject(mesh, texture);
            }

            if (_saveMesh)
            {
                SaveMeshToFile(mesh);
            }

            if (_saveTexture)
            {
                SaveTextureToFile(texture);
            }
        }

        private void CreateVoxelGameObject(Mesh mesh, Texture2D texture)
        {
            var sprite3D = new GameObject(_sprite.name + VOXEL_NAME_POST_FIX);

            var meshFilter = sprite3D.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = sprite3D.AddComponent<MeshRenderer>();

            if (texture != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.SetTexture("_MainTex", texture);
                meshRenderer.sharedMaterial = material;
            }
        }

        private void SaveMeshToFile(Mesh mesh)
        {
            string path = EditorUtility.SaveFilePanel("Save mesh to folder", "Assets/", mesh.name, "asset");

            path = FileUtil.GetProjectRelativePath(path);

            if (string.IsNullOrEmpty(path) == false)
            {
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("[Voxelizer] Mesh Export failed: invalid path");
            }
        }

        private void SaveTextureToFile(Texture2D texture)
        {
            texture.name = _sprite.name + VOXEL_NAME_POST_FIX;
            string path = EditorUtility.SaveFilePanel("Save texture to folder", "Assets/", texture.name, "PNG");

            path = FileUtil.GetProjectRelativePath(path);

            if (string.IsNullOrEmpty(path) == false)
            {
                byte[] _bytes = texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, _bytes);
            }
            else
            {
                Debug.LogWarning("[Voxelizer] Texture Export failed: invalid path");
            }
        }
    }
}
#endif