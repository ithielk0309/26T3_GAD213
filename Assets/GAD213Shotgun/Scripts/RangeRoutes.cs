using UnityEngine;
namespace GAD213.Shotgun
{
    // Connect visible waypoints. Re-evaluate when fire appears or expires.
    public class RangeRoutes : MonoBehaviour
    {
        public Transform[] corners;
        public virtual bool Next(Vector3 start, Vector3 goal, bool avoidFire, out Vector3 next)
        {
            next = start;
            var points = new Vector3[corners.Length + 2];
            points[0] = start; points[1] = goal;
            for(int i=0;i<corners.Length;i++) points[i+2] = corners[i].position;
            var distance = new float[points.Length];
            var first = new int[points.Length];
            var done = new bool[points.Length];
            for(int i=1;i<points.Length;i++) distance[i] = float.PositiveInfinity;
            for(int step=0;step<points.Length;step++)
            {
                int u=-1;
                for(int i=0;i<points.Length;i++)
                    if(!done[i] && !float.IsInfinity(distance[i]) && (u<0 || distance[i]<distance[u])) u=i;
                if(u<0) return false;
                if(u==1) { next=points[first[u]]; next.y=start.y; return true; }
                done[u]=true;
                for(int v=1;v<points.Length;v++)
                {
                    if(done[v]) continue;
                    var a=points[u]; var b=points[v]; a.y=b.y=start.y;
                    float length=Vector3.Distance(a,b);
                    if(distance[u]+length>=distance[v]) continue;
                    bool blocked=false;
                    if(length>.001f)
                        foreach(var h in Physics.SphereCastAll(a,.45f,(b-a)/length,length,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
                            if(h.collider.GetComponentInParent<RangeWall>()) {blocked=true;break;}
                    if(!blocked && avoidFire)
                        foreach(var fire in FirePatch.active)
                            if(fire.Across(a,b,.45f) && !(fire.Across(a,a,.45f) && !fire.Across(b,b,.45f))) {blocked=true;break;}
                    if(blocked) continue;
                    distance[v]=distance[u]+length; first[v]=u==0?v:first[u];
                }
            }
            return false;
        }
    }
}
