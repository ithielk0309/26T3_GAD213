using UnityEngine;
using UnityEngine.UI;
namespace GAD213.Shotgun
{
    // Size the mesh from the smaller axis: the ring stays circular on ultrawide screens.
    [RequireComponent(typeof(CanvasRenderer))]
    public class WheelFace : MaskableGraphic
    {
        protected override void OnEnable()
        {
            if (!GetComponent<CanvasRenderer>()) gameObject.AddComponent<CanvasRenderer>();
            base.OnEnable();
            SetVerticesDirty();
        }
        public Color[] colours = {Color.cyan,Color.blue,Color.green,Color.red};
        [System.NonSerialized] public int highlight=-1;
        static void Quad(VertexHelper mesh, Vector2 centre, Vector2 size, Color tint)
        {
            int n=mesh.currentVertCount; Vector2 half=size*.5f;
            mesh.AddVert(centre+new Vector2(-half.x,-half.y),tint,Vector2.zero);
            mesh.AddVert(centre+new Vector2(-half.x,half.y),tint,Vector2.zero);
            mesh.AddVert(centre+new Vector2(half.x,half.y),tint,Vector2.zero);
            mesh.AddVert(centre+new Vector2(half.x,-half.y),tint,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var rect=rectTransform.rect;
            float r=.49f*Mathf.Min(rect.width,rect.height);
            for(int slot=0;slot<4;slot++)
            {
                var tint=Color.Lerp(new Color(.08f,.08f,.10f),colours[slot],slot==highlight?.85f:.40f);
                for(int j=0;j<=32;j++)
                {
                    float angle=(slot*90-43+86f*j/32)*Mathf.Deg2Rad;
                    Vector2 axis=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle));
                    int index=mesh.currentVertCount;
                    mesh.AddVert(rect.center+axis*r*.32f,tint,Vector2.zero);
                    mesh.AddVert(rect.center+axis*r,tint,Vector2.zero);
                    if(j>0){mesh.AddTriangle(index-2,index-1,index);mesh.AddTriangle(index,index-1,index+1);}
                }
                // The original shell silhouette: coloured hull with a brass base.
                float a=slot*Mathf.PI*.5f;
                Vector2 centre=rect.center+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*r*.64f;
                float scale=r/166.6f;
                if(slot==highlight) Quad(mesh,centre,new Vector2(32,51)*scale,Color.white);
                Quad(mesh,centre,new Vector2(26,45)*scale,new Color(.035f,.035f,.04f));
                Quad(mesh,centre+Vector2.up*5*scale,new Vector2(22,31)*scale,colours[slot]);
                Quad(mesh,centre+new Vector2(-6,5)*scale,new Vector2(3,27)*scale,new Color(1,1,1,.5f));
                Quad(mesh,centre+Vector2.down*16*scale,new Vector2(26,10)*scale,new Color(.9f,.72f,.3f));
            }
        }
    }
}
