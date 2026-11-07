using UnityEngine;

namespace GAD213.Shotgun
{
    public enum ShellKind { Buckshot, Slug, Beanbag, DragonsBreath }

    [CreateAssetMenu(menuName = "GAD213/Shell")]
    public class Shell : ScriptableObject
    {
        public ShellKind kind;
        public string label;
        public Color colour = Color.white;
        [Min(0)] public int stock = 16;
        [Min(1)] public int pellets = 1;
        [Range(0, 30)] public float spreadDegrees;
        [Min(.2f)] public float range = 30;
        [Min(0)] public float damage = 10, push = 1, maxPush = 2, stun;
        [Header("Dragon's Breath")]
        [Min(0)] public float burnSeconds = 2, burnPerSecond = 8;
        [Min(.1f)] public float patchSeconds = 3, patchRadius = 1.6f;
        [Header("Firing audio")]
        public AudioClip fireSound;
        [Tooltip("Volume multiplier for this ammunition only. Above 1 boosts quiet clips.")]
        [Range(0, 10)] public float fireVolume = 1;
    }
}
