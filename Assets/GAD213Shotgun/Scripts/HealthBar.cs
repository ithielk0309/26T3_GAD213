using UnityEngine;
using UnityEngine.UI;
namespace GAD213.Shotgun
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class HealthBar : MaskableGraphic
    {
        public PlayerHealth player;
        public Text label;
        public Color healthyColour=new Color(.22f,.85f,.35f);
        public Color hurtColour=new Color(.95f,.2f,.13f);
        float last=-1;
        void Update()
        {
            float health=player?player.Current:0;
            if(health!=last){last=health;SetVerticesDirty();}
            if(label && player)label.text="HP  "+Mathf.CeilToInt(player.Current)+" / "+Mathf.CeilToInt(player.maxHealth);
        }
        static void Box(VertexHelper mesh,Rect r,Color colour)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(r.xMin,r.yMin),colour,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin,r.yMax),colour,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMax),colour,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMin),colour,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;Box(mesh,r,new Color(.06f,.07f,.09f,.95f));
            float fraction=Application.isPlaying && player?Mathf.Clamp01(player.Current/Mathf.Max(1,player.maxHealth)):1;
            var inner=new Rect(r.x+3,r.y+3,(r.width-6)*fraction,r.height-6);
            Box(mesh,inner,Color.Lerp(hurtColour,healthyColour,fraction));
        }
    }
}
