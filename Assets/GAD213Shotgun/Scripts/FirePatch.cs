using System.Collections.Generic;
using UnityEngine;
namespace GAD213.Shotgun
{
    public class FirePatch : MonoBehaviour
    {
        public static readonly List<FirePatch> active = new List<FirePatch>();
        [Header("Floor fire appearance")]
        [Min(.1f)] public float flameHeight = 1.1f;
        [Min(1)] public float flameDensity = 65;
        [Min(0)] public float smokeDensity = 12;
        public Color flameColour = new Color(1,.48f,.035f,1);
        public Material particleMaterial;
        public ParticleSystem flames, smoke, sparks;
        [Header("Floor burning audio")]
        public AudioClip burningSound;
        [Range(0, 2)] public float burningVolume = .7f;
        [Min(1)] public float burningSoundDistance = 18;
        AudioSource burnAudio;
        public Shell fuel { get; private set; }
        public float Radius { get; private set; }
        float end;
        bool fading;
        Material ownedMaterial;
        Texture2D ownedTexture;
        public void Init(Shell s)
        {
            fuel=s; end=Time.time+s.patchSeconds; Radius=s.patchRadius;
            transform.localScale=Vector3.one;
            if(!flames)
            {
                foreach(var renderer in GetComponentsInChildren<MeshRenderer>()) renderer.enabled=false;
                if(!particleMaterial)
                {
                    ownedTexture=MakeTexture(); ownedMaterial=new Material(Shader.Find("Sprites/Default"));
                    ownedMaterial.mainTexture=ownedTexture; particleMaterial=ownedMaterial;
                }
                BuildVisuals(particleMaterial);
            }
            Configure();
            flames.Play(true); flames.Emit(12); smoke.Play(); sparks.Play();
            if(burningSound)
            {
                burnAudio=gameObject.AddComponent<AudioSource>();burnAudio.playOnAwake=false;burnAudio.loop=true;
                burnAudio.spatialBlend=1;burnAudio.rolloffMode=AudioRolloffMode.Linear;burnAudio.minDistance=2;burnAudio.maxDistance=burningSoundDistance;
                burnAudio.clip=burningSound;burnAudio.volume=burningVolume;burnAudio.Play();
            }
        }
        void OnEnable() { active.Add(this); }
        void OnDisable() { active.Remove(this); if(burnAudio)burnAudio.Stop(); }
        void Update()
        {
            if(!fuel || Time.time>=end) {Destroy(gameObject);return;}
            if(burnAudio)burnAudio.volume=burningVolume*Mathf.Clamp01((end-Time.time)/.35f);
            if(!fading && Time.time>=end-.35f)
            {
                fading=true;
                foreach(var system in new[]{flames,smoke,sparks}) if(system) system.Stop(false,ParticleSystemStopBehavior.StopEmitting);
            }
        }
        void OnDestroy() {if(ownedMaterial)Destroy(ownedMaterial);if(ownedTexture)Destroy(ownedTexture);}
        public static Texture2D MakeTexture()
        {
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false) {name="Soft flame",wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float r=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),1.3f)));
            }
            texture.Apply();return texture;
        }
        public void BuildVisuals(Material material)
        {
            particleMaterial=material;
            flames=Make("Rising flames",material,.45f,.7f,new Color(1,.9f,.35f),new Color(.8f,.09f,.01f),false);
            smoke=Make("Drifting smoke",material,.8f,1.3f,new Color(.22f,.2f,.18f),new Color(.09f,.09f,.09f),true);
            sparks=Make("Rising embers",material,.35f,.7f,new Color(1,.7f,.08f),new Color(1,.15f,.01f),false);
            Configure();
        }
        ParticleSystem Make(string label,Material material,float minLife,float maxLife,Color start,Color finish,bool smoky)
        {
            var obj=new GameObject(label);obj.transform.SetParent(transform,false);
            var ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(minLife,maxLife);
            main.startSpeed=0;main.maxParticles=350;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.rotation=new Vector3(90,0,0);shape.radiusThickness=1;
            var colour=ps.colorOverLifetime;colour.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(start,0),new GradientColorKey(finish,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(smoky?.25f:1,.15f),new GradientAlphaKey(0,1)});colour.color=gradient;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.45f),new Keyframe(.4f,1),new Keyframe(1,smoky?1.5f:.1f)));
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            return ps;
        }
        public void Configure()
        {
            float radius=fuel?fuel.patchRadius:1.6f;
            Tune(flames,radius,flameDensity,.32f,flameHeight,flameHeight*1.4f);
            Tune(smoke,radius*.8f,smokeDensity,.65f,.65f,.8f);
            Tune(sparks,radius,10,.035f,.07f,2.5f);
            if(flames){var main=flames.main;main.startColor=flameColour;}
        }
        static void Tune(ParticleSystem ps,float radius,float rate,float width,float height,float rise)
        {
            if(!ps)return;
            var main=ps.main;main.startSize3D=true;main.startSizeX=width;main.startSizeY=height;main.startSizeZ=width;
            var shape=ps.shape;shape.radius=radius;
            var emission=ps.emission;emission.rateOverTime=rate;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;
            velocity.x=new ParticleSystem.MinMaxCurve(-.12f,.12f);velocity.y=new ParticleSystem.MinMaxCurve(rise*.7f,rise);velocity.z=new ParticleSystem.MinMaxCurve(-.12f,.12f);
        }
        public bool Across(Vector3 a,Vector3 b,float margin)
        {
            Vector3 p=transform.position;a.y=b.y=p.y;var d=b-a;
            float t=Mathf.Clamp01(Vector3.Dot(p-a,d)/Mathf.Max(.0001f,d.sqrMagnitude));
            return (a+d*t-p).sqrMagnitude<=(Radius+margin)*(Radius+margin);
        }
    }
}
