using UnityEngine;
using UnityEngine.SceneManagement;
namespace GAD213.Shotgun
{
    [DefaultExecutionOrder(-100)]
    public class PlayerHealth : MonoBehaviour
    {
        [Min(1)] public float maxHealth = 100;
        [Min(0)] public float damageGraceSeconds = .25f;
        public float Current { get; private set; }
        public bool Paused { get; private set; }
        public bool Dead => Current <= 0;
        public bool BlockGameplay => Paused || Time.frameCount == resumeFrame;
        int resumeFrame = -1;
        float nextDamage;
        void Awake(){Current=maxHealth;Time.timeScale=1;AudioListener.pause=false;}
        void Update()
        {
            if(PrototypeInput.Pressed(KeyCode.Escape) && !Dead) SetPaused(!Paused);
        }
        public void Damage(float amount)
        {
            if(Paused || Dead || amount<=0 || Time.time<nextDamage)return;
            Current=Mathf.Max(0,Current-amount);nextDamage=Time.time+damageGraceSeconds;
            if(Dead)SetPaused(true);
        }
        public void HealFull(){if(!Dead){Current=maxHealth;nextDamage=0;}}
        public void SetPaused(bool value)
        {
            Paused=value;Time.timeScale=value?0:1;AudioListener.pause=value;
            if(value)
            {
                var weapon=GetComponent<WeaponSystem>();if(weapon)weapon.CancelForPause();
            }
            else resumeFrame=Time.frameCount;
            Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;
            Cursor.visible=value;
        }
        void OnGUI()
        {
            if(!Paused)return;
            var old=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3(Screen.width/2f,Screen.height/2f,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.Box(new Rect(-200,-165,400,330),Dead?"YOU WERE DEFEATED":"PAUSED");
            GUI.Label(new Rect(-165,-120,330,35),"HP: "+Mathf.CeilToInt(Current)+" / "+Mathf.CeilToInt(maxHealth));
            GUI.enabled=!Dead;
            if(GUI.Button(new Rect(-150,-65,300,50),"Resume"))SetPaused(false);
            GUI.enabled=true;
            if(GUI.Button(new Rect(-150,0,300,50),"Restart scene"))
            {Time.timeScale=1;AudioListener.pause=false;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
            if(GUI.Button(new Rect(-150,65,300,50),"Quit game"))
            {
                Time.timeScale=1;AudioListener.pause=false;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying=false;
#else
                Application.Quit();
#endif
            }
            GUI.matrix=old;
        }
        void OnDestroy(){Time.timeScale=1;AudioListener.pause=false;}
    }
}
