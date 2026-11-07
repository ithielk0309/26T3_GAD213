using UnityEngine;
using UnityEngine.UI;
using System.Text;
namespace GAD213.Shotgun
{
    public class RangeHUD : MonoBehaviour
    {
        [TextArea] public string encounterLegend;
        public WeaponSystem weapon;
        public Text info;
        public GameObject wheel;
        public WheelFace face;
        public Material traceMaterial;
        public Text[] labels;
        readonly StringBuilder buffer=new StringBuilder();
        void Awake() { LayoutLabels(); }
        void LayoutLabels()
        {
            if (labels == null || !face) return;
            float radius = .49f * Mathf.Min(face.rectTransform.rect.width, face.rectTransform.rect.height);
            for (int i=0;i<labels.Length && i<4;i++)
            {
                if (!labels[i]) continue;
                var direction = new Vector2(Mathf.Sin(i*Mathf.PI*.5f), Mathf.Cos(i*Mathf.PI*.5f));
                labels[i].rectTransform.anchoredPosition = direction * radius * .64f + Vector2.down * 34;
                labels[i].rectTransform.sizeDelta = new Vector2(122,36);
                labels[i].fontSize = 14;
            }
        }
        public void ShowWheel(bool open,int hover,Shell[] shells)
        {
            if(wheel.activeSelf!=open)wheel.SetActive(open);
            if(!open)return;
            LayoutLabels();
            for(int i=0;i<4;i++){face.colours[i]=shells[i].colour;labels[i].text=(i==hover?"[ ":"")+shells[i].label+(i==hover?" ]":""); labels[i].color=i==hover?Color.white:new Color(.8f,.8f,.8f);}
            face.highlight=hover;face.SetVerticesDirty();
        }
        void LateUpdate()
        {
            if(!weapon || !weapon.enabled)return;
            var state=weapon.state;
            buffer.Clear();
            buffer.Append("Load choice: ").Append(weapon.shells[weapon.Selected].label)
                .Append("   |   Chamber: ").Append(state.Chamber?state.Chamber.label:state.SpentCasing?"Spent (pump)":"Empty");
            buffer.Append("\nTube (next first): ");
            if(state.Magazine.Count==0)buffer.Append("empty");
            foreach(var s in state.Magazine)buffer.Append(s.label).Append("   ");
            buffer.Append("\nReserves: ");
            foreach(var s in weapon.shells)buffer.Append(s.label).Append(' ').Append(state.Reserve(s)).Append("   ");
            buffer.Append("\nLive discarded: ").Append(state.LiveDiscarded).Append("   |   ").Append(state.Loading?"Inserting":state.Pumping?"Pumping":"Ready");
            buffer.Append("\nQ + mouse: choose   Hold R: load   Tap R: pump   Click: fire");
            if(!string.IsNullOrEmpty(encounterLegend))buffer.Append("\n").Append(encounterLegend);
            info.text=buffer.ToString();
        }
        public void Trace(Vector3 from,Vector3 to,Color colour)
        {
            if(!traceMaterial)return;
            var line=new GameObject("Shot trace").AddComponent<LineRenderer>();
            line.sharedMaterial=traceMaterial; line.startColor=line.endColor=colour;
            line.startWidth=.018f;line.endWidth=.008f;line.positionCount=2;
            line.SetPosition(0,from);line.SetPosition(1,to);Destroy(line.gameObject,.09f);
        }
    }
}
