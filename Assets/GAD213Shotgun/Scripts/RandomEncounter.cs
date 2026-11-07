using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAD213.Shotgun
{
    public class RandomEncounter : MonoBehaviour
    {
        public WeaponSystem weapon;
        public RangeRoutes routes;
        public RangeEnemy[] enemyTypes;
        public Transform[] spawnPoints;
        [Min(1)] public int minEnemies = 4, maxEnemies = 7;
        [Min(0)] public float preparationSeconds = 8;
        public bool healBetweenRounds = true;
        PlayerHealth health;
        [Min(0)] public float minimumPlayerDistance = 5;
        public bool repeatable;
        public int seed = 210;
        public bool refillBetweenRounds = true;
        readonly List<RangeEnemy> live = new List<RangeEnemy>();
        System.Random random;
        int round;
        float begins;
        bool running, preparing;
        void Start() { health=weapon?weapon.GetComponent<PlayerHealth>():null; random = new System.Random(repeatable ? seed : Environment.TickCount); Status(); }
        void Update()
        {
            if (!weapon || !weapon.Captured || Time.timeScale <= 0) return;
            if (PrototypeInput.Pressed(KeyCode.T)) { Clear(); running = preparing = false; if(health)health.HealFull(); }
            if (!running && !preparing && PrototypeInput.Pressed(KeyCode.Space)) BeginRound();
            if (preparing && Time.time >= begins)
            {
                preparing = false; running = true;
                foreach (var enemy in live) if (enemy) enemy.target = weapon.transform;
            }
            if (running)
            {
                bool any = false;
                foreach (var enemy in live) if (enemy && enemy.gameObject.activeSelf) any = true;
                if (!any) running = false;
            }
            Status();
        }
        void BeginRound()
        {
            if (enemyTypes == null || enemyTypes.Length != 4 || Array.Exists(enemyTypes, e => !e) || spawnPoints == null) return;
            Clear(); if(healBetweenRounds && health)health.HealFull();
            var available = new List<Transform>();
            Physics.SyncTransforms();
            foreach (var point in spawnPoints)
            {
                if (!point || Vector3.Distance(point.position, weapon.transform.position) < minimumPlayerDistance) continue;
                if (!Physics.Raycast(point.position + Vector3.up * 2, Vector3.down, out var ground, 4, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || !ground.collider.GetComponent<IgnitableFloor>()) continue;
                if (Physics.CheckCapsule(point.position + Vector3.up * .55f, point.position + Vector3.up * 1.45f, .45f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                available.Add(point);
            }
            int count = Math.Min(available.Count, random.Next(Math.Max(1, minEnemies), Math.Max(minEnemies, maxEnemies) + 1));
            for (int i = 0; i < count; i++)
            {
                int choice = random.Next(available.Count);
                var point = available[choice]; available.RemoveAt(choice);
                var enemy = Instantiate(enemyTypes[random.Next(enemyTypes.Length)], point.position + Vector3.up, Quaternion.identity, transform);
                enemy.target = null; enemy.routes = routes;
                if (enemy.onAttack == null) enemy.onAttack = new UnityEngine.Events.UnityEvent();
                live.Add(enemy);
            }
            if (live.Count == 0) { Debug.LogWarning("No clear spawn points. Move away from the spawn markers and try again.", this); return; }
            round++;
            if (refillBetweenRounds) foreach (var shell in weapon.shells) weapon.state.TopUp(shell, shell.stock);
            preparing = true; begins = Time.time + preparationSeconds;
        }
        void Clear()
        {
            foreach (var enemy in live) if (enemy) { enemy.gameObject.SetActive(false); Destroy(enemy.gameObject); }
            live.Clear();
            foreach (var fire in new List<FirePatch>(FirePatch.active)) if (fire) { fire.gameObject.SetActive(false); Destroy(fire.gameObject); }
        }
        void Status()
        {
            if (!weapon || !weapon.hud) return;
            string phase = preparing ? "Prepare: " + Mathf.CeilToInt(begins-Time.time) + "s" : running ? "Enemies active" : "Space: start next encounter";
            weapon.hud.encounterLegend = "Amber: Buckshot | Blue: Slug | Green: Pepperbag stun | Red: fire\nRound " + round + " | " + phase + " | T: clear";
        }
        void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = Color.cyan;
            foreach (var p in spawnPoints) if (p) { Gizmos.DrawWireSphere(p.position + Vector3.up, .5f); Gizmos.DrawLine(p.position, p.position + Vector3.up * 2); }
        }
    }
}
