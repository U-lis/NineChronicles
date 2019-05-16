using System;

namespace Nekoyume.Data.Table
{
    [Serializable]
    public class SkillEffect : Row
    {
        public enum SkillType
        {
            Attack,
            Buff,
            Debuff,
        }

        public enum Target
        {
            Enemy,
            Enemies,
            Self,
            Ally,
        }

        public SkillType type = SkillType.Attack;
        public Target target = Target.Enemy;
        public float multiplier = 1.0f;
    }
}
