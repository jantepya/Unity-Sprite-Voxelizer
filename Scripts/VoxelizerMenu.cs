using UnityEditor;
using UnityEngine;



public class VoxilizerMenu : EditorWindow
{
    [MenuItem("Window/Voxelize Sprite")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(VoxilizerMenu));
    }

    public Sprite sprite;


    void OnGUI()
    {
        sprite = (Sprite)EditorGUILayout.ObjectField("Selected Sprite", sprite, typeof(Sprite), true);

        if (GUILayout.Button("Create"))
        {
            VoxelizerUtil.VoxelizeSprite(sprite);
        }
    }
}
