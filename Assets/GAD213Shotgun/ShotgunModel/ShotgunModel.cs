using UnityEngine;
namespace GAD213.Shotgun
{
    public class ShotgunModel : MonoBehaviour
    {
        public Transform pump;
        [Min(0)] public float pumpTravel = .105f;
        [Min(0)] public float recoilDistance = .045f;
        [Min(0)] public float recoilAngle = 4;
        WeaponSystem weapon;
        Vector3 rest, pumpRest;
        Quaternion restRotation;
        bool pumping, spent;
        float pumpStarted, recoil;
        void Start()
        {
            weapon=GetComponentInParent<WeaponSystem>();
            rest=transform.localPosition;restRotation=transform.localRotation;
            if(pump)pumpRest=pump.localPosition;
        }
        void LateUpdate()
        {
            if(!weapon)return;
            bool now=weapon.state.Pumping;
            if(now && !pumping)pumpStarted=Time.time;
            pumping=now;
            if(pump)
            {
                float t=now?Mathf.Clamp01((Time.time-pumpStarted)/Mathf.Max(.01f,weapon.pumpTime)):0;
                pump.localPosition=pumpRest-Vector3.forward*(Mathf.Sin(t*Mathf.PI)*pumpTravel);
            }
            bool fired=weapon.state.SpentCasing;
            if(fired && !spent)recoil=1;
            spent=fired;
            recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*6);
            transform.localPosition=rest-Vector3.forward*(recoil*recoilDistance);
            transform.localRotation=restRotation*Quaternion.Euler(-recoil*recoilAngle,0,0);
        }
    }
}
