using UnityEngine;
using System.Collections.Generic;

namespace GAD213.Shotgun
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Your prefab instance (optional until ready)")]
        public Transform shotgunView, muzzle;
        [Header("Dragon's Breath visual")]
        [Min(1)] public float flameSpeed = 24;
        [Range(.1f,2)] public float flameWidth = .65f;
        [Range(1,12)] public int flameParticlesPerRay = 6;
        public bool igniteWalkableSurfaces;
        public Camera aim;
        public Shell[] shells;
        public RangeHUD hud;
        public FirePatch patchPrefab;
        [Header("Reload and pump audio")]
        public AudioSource audioSource;
        [Tooltip("Plays once when each shell starts loading.")]
        public AudioClip insertSound;
        [Range(0, 2)] public float reloadVolume = 1;
        [Tooltip("Plays once when the pump action starts.")]
        public AudioClip pumpSound;
        [Range(0, 2)] public float pumpVolume = 1;
        [Min(.1f)] public float holdTime = .27f, insertTime = .36f, pumpTime = .32f;
        public readonly PumpShotgunState<Shell> state = new PumpShotgunState<Shell>();
        readonly ReloadGesture gesture = new ReloadGesture();
        public int Selected { get; private set; }
        public bool WheelOpen { get; private set; }
        public bool Captured => Cursor.lockState == CursorLockMode.Locked;
        [Header("Shell ejection")]
        public Transform ejectionPoint;
        public Vector3 ejectionOffset = new Vector3(.055f, .015f, .12f);
        [Min(0)] public float ejectionSpeed = 1.6f;
        [Min(1)] public float shellLifetime = 6;
        Shell lastFired, ejecting;
        float ejectAt;
        Vector2 pointerOrigin;
        int wheelOpenedFrame;
        int hover;
        bool wasQ;
        void Start()
        {
            if (!aim || !hud || shells == null || shells.Length != 4 || System.Array.Exists(shells, s => !s))
            {
                Debug.LogError("Assign the camera, HUD and all four shell assets.", this);
                enabled = false; return;
            }
            state.Reset(shells, s => s.stock);
            hud.wheel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        void OnApplicationFocus(bool focus)
        {
            if (!focus) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; CancelInput(); }
        }
        void CancelInput()
        {
            WheelOpen = false; wasQ = PrototypeInput.MenuHeld;
            if (hud && hud.wheel) hud.wheel.SetActive(false);
            gesture.Reset(PrototypeInput.ReloadHeld); state.CancelLoad();
        }
        public void CancelForPause() { CancelInput(); }
        void Update()
        {
            var health=GetComponent<PlayerHealth>();
            if(Time.timeScale <= 0 || (health && health.BlockGameplay)) return;
            float now = Time.time;
            if (ejecting && now >= ejectAt)
            {
                Eject(ejecting); ejecting = null;
            }
            state.Tick(now, out _, out _, out _);
            if (!health && PrototypeInput.Pressed(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; CancelInput(); return; }
            if (!Captured && !WheelOpen && !PrototypeInput.MenuHeld)
            {
                if (PrototypeInput.FirePressed) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; CancelInput(); }
                return;
            }
            bool q = PrototypeInput.MenuHeld;
            if (q && !wasQ)
            {
                WheelOpen = true; hover = -1; state.CancelLoad();
                Cursor.lockState = CursorLockMode.Confined; Cursor.visible = true;
                pointerOrigin = new Vector2(Screen.width * .5f, Screen.height * .5f);
                PrototypeInput.CentrePointer(pointerOrigin);
                wheelOpenedFrame = Time.frameCount;
            }
            if (WheelOpen && q)
            {
                Vector2 direction = PrototypeInput.Pointer - pointerOrigin;
                float deadZone = 20f * Mathf.Max(.5f, Screen.height / 720f);
                hover = Time.frameCount == wheelOpenedFrame || direction.magnitude < deadZone ? -1 :
                    Mathf.RoundToInt(Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg / 90 + 4) % 4;
            }
            bool released = WheelOpen && !q;
            if (released) { if (hover >= 0) Selected = hover; WheelOpen = false; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            wasQ = q;
            hud.ShowWheel(WheelOpen, hover, shells);
            var action = gesture.Step(PrototypeInput.ReloadHeld, WheelOpen || released, now, holdTime);
            if (WheelOpen || released) return;
            if (action.Pump && state.BeginPump(now, pumpTime))
            {
                ejecting = state.Chamber ? state.Chamber : state.SpentCasing ? lastFired : null;
                ejectAt = now + Mathf.Max(.05f, pumpTime) * .5f;
                Sound(pumpSound, pumpVolume);
            }
            float duration = insertTime; // Loading speed is independent of clip length.
            if (action.ReloadHeld && state.BeginLoad(shells[Selected], 4, now, duration)) Sound(insertSound, reloadVolume);
            if (PrototypeInput.FirePressed && state.Fire(now, .12f, out var fired)) Shoot(fired);
        }
        void Sound(AudioClip clip, float volume = 1) { if (clip && audioSource) audioSource.PlayOneShot(clip, volume); }

        // Ignore colliders belonging to the player or the user's view model.
        bool Hit(Vector3 origin, Vector3 dir, float range, out RaycastHit nearest)
        {
            nearest = default; float best = float.PositiveInfinity;
            foreach (var h in Physics.RaycastAll(origin, dir, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Transform t = h.collider.transform;
                if (t.IsChildOf(transform) || (shotgunView && t.IsChildOf(shotgunView)) || h.distance >= best) continue;
                nearest = h; best = h.distance;
            }
            return !float.IsInfinity(best);
        }
        void Eject(Shell shell)
        {
            Transform basis = ejectionPoint ? ejectionPoint : shotgunView ? shotgunView : aim.transform;
            Vector3 position = ejectionPoint ? basis.position : basis.TransformPoint(ejectionOffset);
            var renderer = shotgunView ? shotgunView.GetComponentInChildren<Renderer>() : null;
            Material material = renderer ? renderer.sharedMaterial : hud.traceMaterial;
            Vector3 velocity = (basis.right + basis.up * .65f + basis.forward * Random.Range(-.2f,.2f)) * ejectionSpeed;
            EjectedShell.Spawn(position, basis.rotation * Quaternion.Euler(90,0,0), velocity, shell, transform, material, shellLifetime);
        }
        void Shoot(Shell shell)
        {
            lastFired = shell;
            Sound(shell.fireSound, shell.fireVolume);
            Transform view = aim.transform;
            Vector3 origin = muzzle ? muzzle.position : view.position;
            Vector3 aimPoint = Hit(view.position, view.forward, shell.range, out var crosshair)
                ? crosshair.point : view.position + view.forward * shell.range;
            Vector3 baseDir = (aimPoint - origin).normalized;
            if (Vector3.Dot(baseDir, view.forward) <= 0) { origin = view.position; baseDir = view.forward; }
            bool isFire = shell.kind == ShellKind.DragonsBreath;
            bool placedFire = false;
            var blast = isFire ? FireBlast.Create(origin, hud.traceMaterial ? hud.traceMaterial.shader : null, flameSpeed, shell.range) : null;
            var counts = new Dictionary<RangeEnemy,int>();
            for (int i = 0; i < shell.pellets; i++)
            {
                Vector2 offset = isFire && i == 0 ? Vector2.zero : Random.insideUnitCircle * Mathf.Tan(shell.spreadDegrees * Mathf.Deg2Rad);
                Vector3 dir = (baseDir + view.right * offset.x + view.up * offset.y).normalized;
                bool hit = Hit(origin, dir, shell.range, out var h);
                if (blast) blast.EmitRay(dir, hit ? h.distance : shell.range, flameWidth, flameParticlesPerRay);
                else hud.Trace(origin, hit ? h.point : origin + dir * shell.range, shell.colour);
                if (!hit) continue;
                var enemy = h.collider.GetComponentInParent<RangeEnemy>();
                if (enemy) { counts.TryGetValue(enemy, out int n); counts[enemy] = n + 1; }
                else if (isFire && !placedFire && patchPrefab && h.normal.y > .75f && (h.collider.GetComponentInParent<IgnitableFloor>() || (igniteWalkableSurfaces && UnityEngine.AI.NavMesh.SamplePosition(h.point,out var floor,.35f,UnityEngine.AI.NavMesh.AllAreas))))
                {
                    Instantiate(patchPrefab, h.point + h.normal * .025f, Quaternion.FromToRotation(Vector3.up, h.normal)).Init(shell);
                    placedFire = true;
                }
            }
            foreach (var pair in counts) pair.Key.TakeShot(shell, pair.Value, baseDir);
        }
    }
}
