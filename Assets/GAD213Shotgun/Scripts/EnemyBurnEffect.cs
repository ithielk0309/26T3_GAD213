using UnityEngine;
namespace GAD213.Shotgun
{
    // A reusable visual attached to the enemy, independent of burn damage timing.
    public class EnemyBurnEffect : MonoBehaviour
    {
        ParticleSystem flames, smoke, embers;
        Material material;
        Texture2D texture;
        bool burning;
        public void Setup(float size, float density, Material sourceMaterial)
        {
            texture=FirePatch.MakeTexture();
            material=new Material(sourceMaterial ? sourceMaterial.shader : Shader.Find("Sprites/Default"));
            material.mainTexture=texture;
            flames=Make("Body flames",density,size,.55f,new Color(1,.82f,.18f),new Color(.9f,.11f,.01f),1);
            smoke=Make("Body smoke",12,.7f,1,new Color(.2f,.18f,.16f),new Color(.08f,.08f,.08f),.3f);
            embers=Make("Body embers",18,.055f,.7f,new Color(1,.7f,.1f),new Color(1,.15f,.01f),1);
        }
        ParticleSystem Make(string label,float rate,float size,float life,Color start,Color end,float alpha)
        {
            var obj=new GameObject(label);obj.transform.SetParent(transform,false);
            var ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(life*.7f,life);
            main.startSpeed=0;main.startSize3D=true;main.startSizeX=size*.65f;main.startSizeY=size;main.startSizeZ=size;
            main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=200;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.BoxShell;shape.scale=new Vector3(.85f,1.35f,.75f);
            var emission=ps.emission;emission.rateOverTime=rate;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.y=1.4f;
            var colour=ps.colorOverLifetime;colour.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(start,0),new GradientColorKey(end,1)},new[]{new GradientAlphaKey(alpha,0),new GradientAlphaKey(alpha,.4f),new GradientAlphaKey(0,1)});colour.color=gradient;
            var growth=ps.sizeOverLifetime;growth.enabled=true;growth.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.5f),new Keyframe(.35f,1),new Keyframe(1,.1f)));
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            return ps;
        }
        public void SetBurning(bool value)
        {
            if(value==burning || !flames)return;
            burning=value;
            foreach(var ps in new[]{flames,smoke,embers})
                if(value)ps.Play();else ps.Stop(false,ParticleSystemStopBehavior.StopEmitting);
            if(value)flames.Emit(24);
        }
        void OnDisable()
        {
            burning=false;
            foreach(var ps in new[]{flames,smoke,embers})if(ps)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        void OnDestroy(){if(material)Destroy(material);if(texture)Destroy(texture);}
    }
}
