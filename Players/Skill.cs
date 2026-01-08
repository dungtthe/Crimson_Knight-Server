using Crimson_Knight_Server.Stats;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Players
{
    public class Skill
    {
        private int templateId;
        private ClassType playerClassType;
        private byte variantId;
        public Skill(int templateId, byte variantId, ClassType playerClassType)
        {
            this.templateId = templateId;
            this.variantId = variantId;
            this.playerClassType = playerClassType;
        }

        private SkillTemplate GetTemplate()
        {
            return TemplateManager.SkillTemplates[playerClassType][templateId];
        }

        private SkillTemplate.Variant GetVariantCurent()
        {
            var template = GetTemplate();
            return template.Variants[this.variantId];
        }

        public Dictionary<StatId, Stat> GetStats()
        {
            return GetVariantCurent().Stats;
        }

        public short GetMpLost()
        {
            return GetVariantCurent().MpLost;
        }
        public int GetCooldown()
        {
            return GetVariantCurent().Cooldown;
        }
        public short GetRange()
        {
            return GetVariantCurent().Range;
        }
        public byte GetTargetCount()
        {
            return GetVariantCurent().TargetCount;
        }


        public bool IsLearned => templateId >= 0;
        public long StartTimeAttack { get; set; }
        public bool CanAttack()
        {
            if (SystemUtil.CurrentTimeMillis() - StartTimeAttack >= GetCooldown())
            {
                return true;
            }
            return false;
        }
    }
}
