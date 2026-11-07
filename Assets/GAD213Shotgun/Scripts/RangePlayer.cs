using UnityEngine;
namespace GAD213.Shotgun
{
    [RequireComponent(typeof(CharacterController))]
    public class RangePlayer : MonoBehaviour
    {
        public WeaponSystem weapon;
        public RangeEnemy[] enemies;
        float pitch,vertical;
        void Update()
        {
            if(PrototypeInput.Pressed(KeyCode.T)) foreach(var e in enemies) if(e)e.ResetTarget();
            if(!weapon.Captured || PrototypeInput.MenuHeld)return;
            var look=PrototypeInput.Look*1.8f;
            transform.Rotate(0,look.x,0);
            pitch=Mathf.Clamp(pitch-look.y,-80,80);
            weapon.aim.transform.localRotation=Quaternion.Euler(pitch,0,0);
            var axes=Vector2.ClampMagnitude(PrototypeInput.Movement,1);
            var cc=GetComponent<CharacterController>();
            vertical=cc.isGrounded?-2:vertical+Physics.gravity.y*Time.deltaTime;
            cc.Move((transform.forward*axes.y*4+transform.right*axes.x*4+Vector3.up*vertical)*Time.deltaTime);
        }
    }
}
