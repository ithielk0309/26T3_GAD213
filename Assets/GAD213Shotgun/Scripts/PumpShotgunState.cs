using System;
using System.Collections.Generic;

namespace GAD213.Shotgun
{
    public struct PumpResult<T>
        where T : class
    {
        public T EjectedLive;
        public bool EjectedSpent;
        public T Chambered;
    }

    // Tracks ammunition and action timers. Sounds and input are handled by PrototypeShotgun.
    public sealed class PumpShotgunState<T>
        where T : class
    {
        private readonly List<T> magazine = new List<T>();
        private readonly Dictionary<T, int> reserves = new Dictionary<T, int>();
        private T pendingLoad;
        private float actionEndTime;
        private float nextAllowedActionTime;
        public IReadOnlyList<T> Magazine
        {
            get
            {
                return magazine;
            }
        }

        public T Chamber { get; private set; }
        public bool SpentCasing { get; private set; }

        public bool Loading
        {
            get
            {
                return pendingLoad != null;
            }
        }

        public bool Pumping { get; private set; }
        public int LiveDiscarded { get; private set; }
        public int Pumps { get; private set; }

        public void Reset(IEnumerable<T> types, Func<T, int> stock)
        {
            magazine.Clear();
            reserves.Clear();
            foreach (T type in types)
            {
                if (type != null)
                {
                    reserves[type] = Math.Max(0, stock(type));
                }
            }

            Chamber = null;
            pendingLoad = null;
            SpentCasing = false;
            Pumping = false;
            actionEndTime = 0f;
            nextAllowedActionTime = 0f;
            LiveDiscarded = 0;
            Pumps = 0;
        }

        public void TopUp(T type, int minimum)
        {
            if(type != null && reserves.ContainsKey(type)) reserves[type] = Math.Max(reserves[type],minimum);
        }

        public int Reserve(T type)
        {
            int count;
            return type != null && reserves.TryGetValue(type, out count) ? count : 0;
        }

        public bool BeginLoad(T choice, int capacity, float now, float duration)
        {
            if (Loading || Pumping || now < nextAllowedActionTime || magazine.Count >= capacity || Reserve(choice) <= 0)
            {
                return false;
            }

            pendingLoad = choice;
            reserves[choice]--;
            actionEndTime = now + Math.Max(0.05f, duration);
            return true;
        }

        public bool CancelLoad()
        {
            if (!Loading)
            {
                return false;
            }

            reserves[pendingLoad]++;
            pendingLoad = null;
            return true;
        }

        public bool BeginPump(float now, float duration)
        {
            if (Loading || Pumping || now < nextAllowedActionTime)
            {
                return false;
            }

            Pumping = true;
            actionEndTime = now + Math.Max(0.05f, duration);
            return true;
        }

        public void Tick(float now, out T loaded, out bool pumpFinished, out PumpResult<T> result)
        {
            loaded = null;
            pumpFinished = false;
            result = default(PumpResult<T>);
            if (now < actionEndTime)
            {
                return;
            }

            if (Loading)
            {
                loaded = pendingLoad;
                magazine.Insert(0, loaded); // Last loaded is next to chamber.
                pendingLoad = null;
            }
            else if (Pumping)
            {
                result.EjectedLive = Chamber;
                result.EjectedSpent = SpentCasing;
                if (Chamber != null)
                {
                    LiveDiscarded++;
                }

                Chamber = null;
                SpentCasing = false;
                if (magazine.Count > 0)
                {
                    Chamber = magazine[0];
                    magazine.RemoveAt(0);
                }

                result.Chambered = Chamber;
                Pumping = false;
                Pumps++;
                pumpFinished = true;
            }
        }

        public bool Fire(float now, float recovery, out T fired)
        {
            fired = null;
            if (Loading || Pumping || now < nextAllowedActionTime || Chamber == null || SpentCasing)
            {
                return false;
            }

            fired = Chamber;
            Chamber = null;
            SpentCasing = true; // A shot never feeds the next shell automatically.
            nextAllowedActionTime = now + Math.Max(0f, recovery);
            return true;
        }
    }
}

