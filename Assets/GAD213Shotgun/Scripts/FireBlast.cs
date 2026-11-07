using UnityEngine;

namespace GAD213.Shotgun
{
    // Temporary shot visual. Damage and surface ignition remain in WeaponSystem.
    public class FireBlast : MonoBehaviour
    {
        ParticleSystem flames;
        Material material;
        Texture2D texture;
        float speed;
        public static FireBlast Create(Vector3 origin, Shader shader, float travelSpeed, float range)
        {
            var obj = new GameObject("Dragon's Breath blast");
            obj.transform.position = origin;
            var blast = obj.AddComponent<FireBlast>();
            blast.speed = Mathf.Max(1, travelSpeed);
            blast.flames = obj.AddComponent<ParticleSystem>();
            blast.flames.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = blast.flames.main;
            main.loop = false; main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 256; main.startSpeed = 0;
            var emission = blast.flames.emission; emission.enabled = false;
            var shape = blast.flames.shape; shape.enabled = false;
            var colour = blast.flames.colorOverLifetime; colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] {
                new GradientColorKey(new Color(1, .95f, .5f), 0),
                new GradientColorKey(new Color(1, .42f, .04f), .35f),
                new GradientColorKey(new Color(.65f, .09f, .015f), 1)
            }, new[] {new GradientAlphaKey(1,0),new GradientAlphaKey(.85f,.55f),new GradientAlphaKey(0,1)});
            colour.color = gradient;
            var size = blast.flames.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0,.25f),new Keyframe(.45f,1),new Keyframe(1,.15f)));
            blast.texture = new Texture2D(32,32,TextureFormat.RGBA32,false);
            blast.texture.wrapMode = TextureWrapMode.Clamp;
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                float radius = Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;
                float alpha = Mathf.Pow(Mathf.Clamp01(1-radius), .65f);
                blast.texture.SetPixel(x,y,new Color(1,1,1,alpha));
            }
            blast.texture.Apply();
            blast.material = new Material(shader ? shader : Shader.Find("Sprites/Default"));
            blast.material.mainTexture = blast.texture;
            var renderer = blast.flames.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = blast.material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            blast.flames.Play();
            Destroy(obj,range/blast.speed+.4f);
            return blast;
        }
        public void EmitRay(Vector3 direction, float distance, float width, int particles)
        {
            if(distance < .05f) return;
            for(int i=0;i<particles;i++)
            {
                float velocity = speed * Random.Range(.8f,1.2f);
                var particle = new ParticleSystem.EmitParams {
                    position = transform.position,
                    velocity = direction * velocity,
                    startLifetime = Mathf.Max(.01f,(distance-.03f)/velocity),
                    startSize = width * Random.Range(.65f,1.35f),
                    startColor = Color.white,
                    rotation = Random.Range(0,360)
                };
                flames.Emit(particle,1);
            }
        }
        void OnDestroy()
        {
            if(material) Destroy(material);
            if(texture) Destroy(texture);
        }
    }
}
