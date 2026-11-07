using UnityEditor;
using UnityEngine;
namespace GAD213.Shotgun.Editor
{
    // Save the particle systems into the existing prefab once, so they are editable.
    [InitializeOnLoad]
    public static class BakeFloorFire
    {
        const string Root="Assets/GAD213Shotgun/Range/";
        static BakeFloorFire(){EditorApplication.delayCall+=Install;}
        [MenuItem("Tools/GAD213/Prepare Editable Floor Fire")]
        public static void Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            string path=Root+"Fire patch.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(!prefab || prefab.GetComponent<FirePatch>().flames)return;
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Soft flame.asset");
            if(!texture){texture=FirePatch.MakeTexture();AssetDatabase.CreateAsset(texture,Root+"Soft flame.asset");}
            var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"Floor flames.mat");
            if(!material){material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=texture;AssetDatabase.CreateAsset(material,Root+"Floor flames.mat");}
            var instance=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var patch=instance.GetComponent<FirePatch>();
                for(int i=instance.transform.childCount-1;i>=0;i--)
                {
                    var child=instance.transform.GetChild(i);
                    if(child.name=="Flame" || child.name=="Scorched embers") Object.DestroyImmediate(child.gameObject);
                }
                patch.BuildVisuals(material);PrefabUtility.SaveAsPrefabAsset(instance,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(instance);}
            AssetDatabase.SaveAssets();
        }
    }
}
