using UnityEngine;

namespace GAD213.Shotgun
{
    public class EjectedShell : MonoBehaviour
    {
        Material plastic, brass;

        public static void Spawn(Vector3 position, Quaternion rotation, Vector3 velocity,
            Shell shell, Transform player, Material template, float lifetime)
        {
            var root = new GameObject(shell.label + " ejected shell");
            root.layer = 2; // Ignore weapon raycasts.
            root.transform.SetPositionAndRotation(position, rotation);
            var effect = root.AddComponent<EjectedShell>();
            var shader = template ? template.shader : Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Standard");
            effect.plastic = new Material(shader);
            effect.brass = new Material(shader);
            SetColour(effect.plastic, shell.colour);
            SetColour(effect.brass, new Color(.72f, .48f, .13f));
            Part(root.transform, "Coloured hull", Vector3.zero, new Vector3(.025f,.032f,.025f), effect.plastic);
            Part(root.transform, "Brass base", new Vector3(0,-.03f,0), new Vector3(.028f,.006f,.028f), effect.brass);
            var collider = root.AddComponent<CapsuleCollider>();
            collider.radius = .014f; collider.height = .072f;
            foreach (var other in player.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(collider, other);
            var body = root.AddComponent<Rigidbody>();
            body.mass = .025f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = velocity;
            body.angularVelocity = Random.onUnitSphere * 18;
            Destroy(root, Mathf.Max(1, lifetime));
        }

        static void SetColour(Material material, Color colour)
        {
            material.color = colour;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
        }
        static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name; part.layer = 2;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position; part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
        void OnDestroy()
        {
            if (plastic) Destroy(plastic);
            if (brass) Destroy(brass);
        }
    }
}
