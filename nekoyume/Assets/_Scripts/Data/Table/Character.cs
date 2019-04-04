using System;

namespace Nekoyume.Data.Table
{
    [Serializable]
    public class Character : Row
    {
        public int Id = 0;
        public string characterName = "";
        public string characterInfo = "";
        public string characterResource = "";
        public int bookIndex = 0;
        public string size = "s";
        public Elemental.ElementalType elemental;
        public bool isBoss = false;
        public int maxLevel = 0;
        public int hp = 0;
        public int damage = 0;
        public int defense = 0;
        public float luck;
        public int lvHp = 0;
        public int lvDamage = 0;
        public int lvDefense = 0;
        public float lvLuck = 0f;
        public string skill0 = "";
        public string skill1 = "";
        public string skill2 = "";
        public string skill3 = "";
    }
}
