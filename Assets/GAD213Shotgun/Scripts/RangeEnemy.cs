using UnityEngine;
using UnityEngine.Events;
namespace GAD213.Shotgun
{
    [RequireComponent(typeof(Rigidbody),typeof(CapsuleCollider))]
    public class RangeEnemy : MonoBehaviour
    {
        public bool useWeakness;
        public ShellKind weakness;
        public bool avoidsFire = true;
        public float maxHealth = 100, walkSpeed = 1.7f, reach = 1.45f;
        public Transform target, visual;
        public RangeRoutes routes;
        public Animator animator;
        public GameObject burnVisual;
        [Header("Enemy burning")]
        [Min(.1f)] public float burnFlameSize = .85f;
        [Min(1)] public float burnFlameDensity = 75;
        public Material burnParticleMaterial;
        public AudioClip burningSound;
        [Range(0, 2)] public float burningVolume = .7f;
        [Min(1)] public float burningSoundDistance = 18;
        EnemyBurnEffect burnEffect;
        AudioSource burnAudio;
        [Header("Attack")]
        [Min(0)] public float attackDamage = 15;
        [Min(.1f)] public float attackCooldown = 1.2f;
        public UnityEvent onAttack;
        public float Health {get; private set;}
        Rigidbody body;
        Vector3 destination,spawn,restPos;
        Quaternion rotation,rest;
        float nextPlan,freezeUntil,burnEnd,burnDps,flashUntil,attackAt,lungeUntil;
        bool routeAvailable, dynamicHit;
        Vector3 localHit = Vector3.back;
        void Awake()
        {
            body=GetComponent<Rigidbody>(); spawn=body.position; rotation=body.rotation;
            if(visual){rest=visual.localRotation;restPos=visual.localPosition;}
            if(animator) animator.applyRootMotion=false;
            body.constraints=RigidbodyConstraints.FreezeRotation;
            ResetTarget();
        }
        public void ResetTarget()
        {
            if(!body) return;
            if(!body.isKinematic){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;}
            body.isKinematic=true; body.position=spawn; body.rotation=rotation;
            dynamicHit=routeAvailable=false; nextPlan=freezeUntil=burnEnd=flashUntil=lungeUntil=0;
            attackAt=Time.time+1; Health=maxHealth; gameObject.SetActive(true);
            if(burnVisual) burnVisual.SetActive(false);
            if(burnEffect)burnEffect.SetBurning(false);
            if(burnAudio)burnAudio.Stop();
            if(visual){visual.localRotation=rest; visual.localPosition=restPos;}
        }
        void Trigger(string name)
        {
            if(!animator || !animator.runtimeAnimatorController) return;
            foreach(var p in animator.parameters)
                if(p.name==name && p.type==AnimatorControllerParameterType.Trigger){animator.SetTrigger(name);break;}
        }
        public void TakeShot(Shell s,int hits,Vector3 direction)
        {
            if(Health<=0)return;
            float bonus=useWeakness && s.kind==weakness ? 2f : 1f;
            Health-=s.damage*hits*bonus;
            if(Health<=0){gameObject.SetActive(false);return;}
            flashUntil=Time.time+.18f;
            localHit=transform.InverseTransformDirection(direction.normalized);
            Trigger(s.stun>0?"Stun":"Hit");
            if(s.kind==ShellKind.DragonsBreath) Ignite(s);
            float push=Mathf.Min(s.maxPush,s.push*hits);
            if(push<=0 && s.stun<=0)return;
            // Navigation stops writing position before the physical impulse.
            dynamicHit=true; body.isKinematic=false;
            direction.y=0;
            body.linearVelocity=Vector3.zero;
            body.AddForce(direction.normalized*push,ForceMode.VelocityChange);
            freezeUntil=Mathf.Max(freezeUntil,Time.time+Mathf.Max(.16f,s.stun*bonus));
            lungeUntil=0; attackAt=Mathf.Max(attackAt,freezeUntil+.2f);
        }
        void Ignite(Shell s){burnDps=s.burnPerSecond*(useWeakness && weakness==ShellKind.DragonsBreath?2f:1f); burnEnd=Time.time+s.burnSeconds;}
        void ShowBurning(bool burning)
        {
            if(burnVisual && burnVisual.activeSelf)burnVisual.SetActive(false);
            if(burning && !burnEffect)
            {
                var obj=new GameObject("Enemy burning effect");obj.transform.SetParent(visual?visual:transform,false);
                burnEffect=obj.AddComponent<EnemyBurnEffect>();burnEffect.Setup(burnFlameSize,burnFlameDensity,burnParticleMaterial);
            }
            if(burnEffect)burnEffect.SetBurning(burning);
            if(burning && burningSound)
            {
                if(!burnAudio){burnAudio=gameObject.AddComponent<AudioSource>();burnAudio.playOnAwake=false;burnAudio.loop=true;burnAudio.spatialBlend=1;burnAudio.rolloffMode=AudioRolloffMode.Linear;burnAudio.minDistance=2;}
                burnAudio.volume=burningVolume;burnAudio.maxDistance=burningSoundDistance;
                if(burnAudio.clip!=burningSound)burnAudio.clip=burningSound;
                if(!burnAudio.isPlaying)burnAudio.Play();
            }
            else if(burnAudio && burnAudio.isPlaying)burnAudio.Stop();
        }
        void OnDisable(){if(burnAudio)burnAudio.Stop();}
        void FixedUpdate()
        {
            foreach(var fire in FirePatch.active)
                if(fire.fuel && fire.Across(body.position,body.position,.35f)) Ignite(fire.fuel);
            bool burning=Time.time<burnEnd;
            ShowBurning(burning);
            if(burning){Health-=burnDps*Time.fixedDeltaTime;if(Health<=0){gameObject.SetActive(false);return;}}
            if(dynamicHit)
            {
                if(Time.time<freezeUntil)return;
                if(body.linearVelocity.sqrMagnitude>.15f && Time.time<freezeUntil+.5f)return;
                body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;body.isKinematic=true;
                dynamicHit=false; nextPlan=0;
            }
            if(!target || !routes)return;
            Vector3 goal=target.position; goal.y=body.position.y;
            if((goal-body.position).sqrMagnitude<reach*reach)
            {
                if(Time.time>=attackAt && routes.Next(body.position,goal,avoidsFire,out var near) && new Vector2(near.x-goal.x,near.z-goal.z).sqrMagnitude<.05f)
                {attackAt=Time.time+attackCooldown;lungeUntil=Time.time+.2f;Trigger("Attack");var health=target.GetComponentInParent<PlayerHealth>();if(health)health.Damage(attackDamage);onAttack?.Invoke();}
                return;
            }
            if(Time.time>=nextPlan)
            {routeAvailable=routes.Next(body.position,goal,avoidsFire,out destination);nextPlan=Time.time+.18f;}
            if(!routeAvailable)return;
            var d=destination-body.position; d.y=0;
            if(d.sqrMagnitude>.002f)body.MoveRotation(Quaternion.LookRotation(d));
            body.MovePosition(Vector3.MoveTowards(body.position,destination,walkSpeed*Time.fixedDeltaTime));
        }
        void LateUpdate()
        {
            if(!visual)return;
            float lean=dynamicHit && Time.time<freezeUntil? -17f : -12f*Mathf.Clamp01((flashUntil-Time.time)/.18f);
            visual.localRotation=rest*Quaternion.Euler(-lean*localHit.z,0,lean*localHit.x);
            visual.localPosition=restPos+Vector3.forward*(.24f*Mathf.Clamp01((lungeUntil-Time.time)/.2f));
        }
    }
}
