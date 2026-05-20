using Deuteros.Code.Objects.Bulletins;
using Deuteros.Code.Objects.ModuleTextFrame;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects.GameData
{
    [Serializable]
    public class BaseData
    {
        public List<string> PersonNames { get; set; }
        public Dictionary<Enums.StellarBodies, Deuteros.Code.Objects.Interfaces.IPlanet> Planets { get; set; }
        public Dictionary<Enums.StellarBodies, Deuteros.Code.Objects.Star> Stars { get; set; }
        public List<Item> ItemList { get; set; }
		public FrameContainer ModuleFrameTexts { get; set; }
		public BulletinContainer BulletinTexts { get; set; }
		public Dictionary<Enums.ItemTypes, int> ResourceLevels_Survey_Multiplier { get; set; }
        public Dictionary<Enums.ItemTypes, int> ResourceRate_Per_Derrick { get; set; }
        public Color Red { get; set; } = new Color(255, 0, 0, 255);
        public Color Green { get; set; } = new Color(0, 136, 0, 255);
        public Color Blue { get; set; } = new Color(0, 34, 136, 255);
        public Color Beige { get; set; } = new Color(153, 170, 119, 255);
        public Color Dark_Beige { get; set; } = new Color(85, 102, 51, 255);
        public Color Yellow { get; set; } = new Color(255, 255, 0, 255);
        public Color LightBlue { get; set; } = new Color(170, 204, 238, 255);
    }
}
