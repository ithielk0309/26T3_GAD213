using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GAD213.Shotgun.Editor
{
    public static class BuildShotgunRange
    {
        const string Root="Assets/GAD213Shotgun/Range";
        const string ScenePath=Root+"/Shotgun Range.unity";
        static Material floor,wall,skin,hot;
        [MenuItem("Tools/GAD213/Open or Create Shotgun Range")]
        public static void OpenRange()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(File.Exists(ScenePath)){EditorSceneManager.OpenScene(ScenePath);return;}
            Create();
        }
        static Material Mat(string name,Color colour)
        {
            string path=Root+"/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat)return mat;
            var shader=UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline ?
                Shader.Find("Universal Render Pipeline/Lit"):Shader.Find("Standard");
            if(!shader)shader=Shader.Find("Standard");
            mat=new Material(shader){color=colour};AssetDatabase.CreateAsset(mat,path);return mat;
        }
        static GameObject Obj(string name,Vector3 pos,Transform parent=null)
        {
            var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=pos;return o;
        }
        static GameObject Shape(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat,Transform parent=null,bool solid=true)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;
            o.GetComponent<Renderer>().sharedMaterial=mat;
            if(!solid)Object.DestroyImmediate(o.GetComponent<Collider>());return o;
        }
        static GameObject Prefab(GameObject o,string name)
        {
            string path=Root+"/"+name+".prefab";var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var prefab=existing?existing:PrefabUtility.SaveAsPrefabAsset(o,path);Object.DestroyImmediate(o);return prefab;
        }
        static Shell ShellAsset(int i)
        {
            string[] names={"Buckshot","Slug","Beanbag","Dragon's Breath"};
            string path=Root+"/"+names[i]+".asset";
            var s=AssetDatabase.LoadAssetAtPath<Shell>(path);if(s)return s;
            s=ScriptableObject.CreateInstance<Shell>();s.kind=(ShellKind)i;s.label=names[i];
            s.colour=new[]{new Color(.95f,.55f,.18f),new Color(.15f,.72f,1),new Color(.38f,.85f,.31f),new Color(1,.23f,.12f)}[i];
            s.damage=new[]{6f,55f,3f,9f}[i];s.range=new[]{20f,65f,18f,11f}[i];
            s.pellets=i==0?9:1;s.spreadDegrees=i==0?6f:0;
            s.push=new[]{.38f,2f,7f,0f}[i];s.maxPush=new[]{1.4f,2f,7f,0f}[i];s.stun=i==2?1.5f:0;
            AssetDatabase.CreateAsset(s,path);return s;
        }
        static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 pos)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent,false);rt.sizeDelta=size;rt.anchoredPosition=pos;return rt;
        }
        static Text Label(string name,Transform parent,string value,Vector2 size,Vector2 pos,int fontSize)
        {
            var rt=Rect(name,parent,size,pos);var t=rt.gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.text=value;t.color=Color.white;
            t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;rt.gameObject.AddComponent<Shadow>();return t;
        }
        public static void Create()
        {
            if(File.Exists(ScenePath))return;
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            floor=Mat("Floor",new Color(.27f,.32f,.34f));wall=Mat("Cover",new Color(.39f,.32f,.25f));
            skin=Mat("Enemy",new Color(.34f,.54f,.65f));hot=Mat("Embers",new Color(1,.27f,.035f));
            Shape("Ignitable floor",PrimitiveType.Cube,new Vector3(0,-.25f,4),new Vector3(24,.5f,36),floor).AddComponent<IgnitableFloor>();
            MakeWall("Route divider",new Vector3(-1,1,4),new Vector3(4,2,3));
            MakeWall("West",new Vector3(-12,1,4),new Vector3(.5f,2,36));
            MakeWall("East",new Vector3(12,1,4),new Vector3(.5f,2,36));
            MakeWall("North",new Vector3(0,1,22),new Vector3(24,2,.5f));
            MakeWall("South",new Vector3(0,1,-14),new Vector3(24,2,.5f));
            var sun=Obj("Sun",Vector3.up*8).AddComponent<Light>();sun.type=LightType.Directional;
            sun.transform.rotation=Quaternion.Euler(48,-32,0);RenderSettings.ambientLight=Color.gray;
            var routes=Obj("Route corners (move with cover)",Vector3.zero).AddComponent<RangeRoutes>();
            routes.corners=new Transform[4];int k=0;
            foreach(float x in new[]{-4.2f,2.2f})foreach(float z in new[]{1.3f,6.7f})
                routes.corners[k++]=Obj("Corner "+k,new Vector3(x,1,z),routes.transform).transform;

            var player=Obj("Practice player",new Vector3(1,1,-7));
            player.layer=2;player.AddComponent<CharacterController>();var w=player.AddComponent<WeaponSystem>();
            var driver=player.AddComponent<RangePlayer>();driver.weapon=w;
            w.aim=Obj("View (add your own shotgun here)",new Vector3(0,.65f,0),player.transform).AddComponent<Camera>();
            w.aim.tag="MainCamera";w.aim.nearClipPlane=.03f;w.aim.gameObject.AddComponent<AudioListener>();
            w.audioSource=player.AddComponent<AudioSource>();w.audioSource.playOnAwake=false;
            w.shells=new Shell[4];for(int i=0;i<4;i++)w.shells[i]=ShellAsset(i);
            var disc=Obj("Fire patch",Vector3.zero);
            Shape("Scorched embers",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.035f,1),hot,disc.transform,false);
            for(int i=0;i<7;i++){
                float angle=i*2.4f;
                var spot=new Vector3(Mathf.Sin(angle)*.28f,.15f,Mathf.Cos(angle)*.28f);
                Shape("Flame",PrimitiveType.Sphere,spot,new Vector3(.085f,.42f,.085f),hot,disc.transform,false);
            }
            disc.AddComponent<FirePatch>();w.patchPrefab=Prefab(disc,"Fire patch").GetComponent<FirePatch>();

            var root=Obj("Practice enemy",Vector3.zero);root.AddComponent<CapsuleCollider>().height=2;
            var rb=root.AddComponent<Rigidbody>();rb.isKinematic=true;rb.linearDamping=4;rb.interpolation=RigidbodyInterpolation.Interpolate;
            var e=root.AddComponent<RangeEnemy>();
            e.visual=Shape("Body",PrimitiveType.Capsule,Vector3.zero,new Vector3(.9f,1,.9f),skin,root.transform,false).transform;
            e.burnVisual=Shape("Burn indicator",PrimitiveType.Sphere,new Vector3(0,.7f,0),new Vector3(.8f,.8f,.8f),hot,e.visual,false);e.burnVisual.SetActive(false);
            var prefab=Prefab(root,"Practice enemy");driver.enemies=new RangeEnemy[2];
            for(int i=0;i<2;i++){
                var enemy=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                enemy.name=i==0?"Cautious enemy":"Tank enemy";enemy.transform.position=new Vector3(i==0?-1:4,1,i==0?14:17);
                var unit=enemy.GetComponent<RangeEnemy>();unit.target=player.transform;unit.routes=routes;unit.avoidsFire=i==0;
                driver.enemies[i]=unit;PrefabUtility.RecordPrefabInstancePropertyModifications(unit);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.transform);
            }
            var canvas=new GameObject("Shotgun HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            var hud=canvas.AddComponent<RangeHUD>();hud.weapon=w;w.hud=hud;
            hud.traceMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Shot traces.mat");
            if(!hud.traceMaterial){hud.traceMaterial=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(hud.traceMaterial,Root+"/Shot traces.mat");}
            hud.info=Label("Ammo state",canvas.transform,"Load choice / chamber / tube / reserves",new Vector2(710,155),Vector2.zero,17);
            var ir=hud.info.rectTransform;ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(0,1);ir.anchoredPosition=new Vector2(15,-12);hud.info.alignment=TextAnchor.UpperLeft;
            Label("Crosshair",canvas.transform,"+",new Vector2(28,28),Vector2.zero,20);
            var wr=Rect("Shell wheel (visible in Edit mode)",canvas.transform,new Vector2(340,340),Vector2.zero);
            hud.wheel=wr.gameObject;hud.face=wr.gameObject.AddComponent<WheelFace>();hud.face.raycastTarget=false;
            hud.labels=new Text[4];
            for(int i=0;i<4;i++){
                var d=new Vector2(Mathf.Sin(i*Mathf.PI*.5f),Mathf.Cos(i*Mathf.PI*.5f));
                hud.labels[i]=Label(w.shells[i].label,wr,w.shells[i].label,new Vector2(128,55),d*108,17);
                hud.face.colours[i]=w.shells[i].colour;
            }
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();Selection.activeObject=player;
        }
        static void MakeWall(string name,Vector3 pos,Vector3 scale)
        {Shape(name,PrimitiveType.Cube,pos,scale,wall).AddComponent<RangeWall>();}
    }
}
