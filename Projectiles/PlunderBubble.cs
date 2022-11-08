using JoJoStands.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using JoJoStands.Items;
using JoJoStands.Items.CraftingMaterials;
using System.Security.Cryptography.X509Certificates;

namespace JoJoStands.Projectiles
{
    public class PlunderBubble : ModProjectile
    {
        public const byte Plunder_None = 0;
        public const byte Plunder_Fire = 1;
        public const byte Plunder_Ichor = 2;
        public const byte Plunder_Cursed = 3;
        public const byte Plunder_Ice = 4;
        public const byte Plunder_Viral = 5;

        public readonly int[] PlunderDusts = new int[5] { DustID.Torch, DustID.Ichor, DustID.CursedTorch, DustID.IceTorch, DustID.GoldCritter };

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 5 * 60;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
            if (Main.rand.NextBool(2))
            {
                int dustIndex = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Cloud, Projectile.velocity.X * -0.5f, Projectile.velocity.Y * -0.5f);
                Main.dust[dustIndex].noGravity = true;
            }
            if (Projectile.ai[0] != Plunder_None && Main.rand.NextBool(3))
            {
                for (int i = 0; i < Main.rand.Next(1, 3 + 1); i++)
                {
                    int dustIndex = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, PlunderDusts[(int)Projectile.ai[0] - 1], Projectile.velocity.X * -0.5f, Projectile.velocity.Y * -0.5f);
                    Main.dust[dustIndex].noGravity = true;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Cloud, Projectile.velocity.X * -0.5f, Projectile.velocity.Y * -0.5f);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            MyPlayer mPlayer = player.GetModPlayer<MyPlayer>();
            int plunderType = (int)Projectile.ai[0];
            if (Main.rand.NextFloat(0, 101) <= mPlayer.standCritChangeBoosts)
                crit = true;

            if (plunderType == Plunder_None)
            {
                if (Main.rand.NextBool(5) && !player.HasBuff(ModContent.BuffType<AbilityCooldown>()))
                {
                    target.AddBuff(ModContent.BuffType<ParchedDebuff>(), 200);
                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(5));
                }
                if (Main.rand.NextBool(7) && !player.HasBuff(ModContent.BuffType<AbilityCooldown>()))
                {
                    target.AddBuff(ModContent.BuffType<Asphyxiating>(), 350);
                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(5));
                }
            }
            else
            {
                if (Main.rand.NextBool(3) && plunderType == Plunder_Fire)
                    target.AddBuff(BuffID.OnFire, 190);
                if (Main.rand.NextBool(3) && plunderType == Plunder_Ichor)
                    target.AddBuff(BuffID.Ichor, 280);
                if (Main.rand.NextBool(3) && plunderType == Plunder_Cursed)
                    target.AddBuff(BuffID.CursedInferno, 230);
                if (Main.rand.NextBool(3) && plunderType == Plunder_Ice)
                    target.AddBuff(BuffID.Frostburn, 230);
                if (Main.rand.NextBool(3) && plunderType == Plunder_Viral)
                    target.AddBuff(ModContent.BuffType<Infected>(), 280);
            }
            if (mPlayer.crackedPearlEquipped)
            {
                if (Main.rand.NextFloat(0, 101) >= 60)
                    target.AddBuff(ModContent.BuffType<Infected>(), 10 * 60);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Player player = Main.player[Projectile.owner];
            MyPlayer mPlayer = player.GetModPlayer<MyPlayer>();

            if (Main.rand.NextBool(10) && !player.HasBuff(ModContent.BuffType<AbilityCooldown>()))
            {
                target.AddBuff(BuffID.Obstructed, 220);
                player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(6));
            }
        }
    }
}