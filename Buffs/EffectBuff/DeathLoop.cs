using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using JoJoStands.Buffs.Debuffs;

namespace JoJoStands.Buffs.EffectBuff
{
    public class DeathLoop : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Death Loop");
            Description.SetDefault("The targetted enemy will go through endless deaths upon death...");
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;    
        }
    }
}