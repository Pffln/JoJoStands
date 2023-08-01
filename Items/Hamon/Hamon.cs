using JoJoStands.Buffs.ItemBuff;
using JoJoStands.Items.CraftingMaterials;
using JoJoStands.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace JoJoStands.Items.Hamon
{
    public class Hamon : HamonDamageClass
    {
        public override bool affectedByHamonScaling => true;

        public override void SafeSetDefaults()
        {
            Item.damage = 14;
            Item.width = 32;
            Item.height = 32;        //hitbox's width and height when the Item is in the world
            Item.maxStack = 1;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.rare = 5;
            Item.shoot = ModContent.ProjectileType<HamonPunches>();
            Item.useTurn = true;
            Item.noWet = true;
            Item.useAnimation = 25;
            Item.useTime = 15;
            Item.useStyle = 5;
            Item.channel = true;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.shootSpeed = 15f;
        }

        private int healTimer = 0;

        public override void HoldItem(Player player)
        {
            HamonPlayer hamonPlayer = player.GetModPlayer<HamonPlayer>();
            hamonPlayer.learnedHamon = true;
            UI.HamonBar.ShowHamonBar();
            ChargeHamon();
            if (player.whoAmI == Main.myPlayer)
            {
                if (hamonPlayer.amountOfHamon < 5)
                    return;

                if (Main.mouseRight)
                {
                    healTimer++;
                    if (Main.rand.Next(0, 2) == 0)
                    {
                        int dustIndex = Dust.NewDust(player.position, player.width, player.height, DustID.IchorTorch, SpeedY: Main.rand.NextFloat(-0.6f, 1f));
                        Main.dust[dustIndex].noGravity = true;
                    }
                }
                if (healTimer >= 3 * 60)
                {
                    int healamount = Main.rand.Next(15, 24);
                    player.HealEffect(healamount);
                    player.statLife += healamount;
                    hamonPlayer.amountOfHamon -= 5;
                    healTimer = 0;
                }
                if (Main.mouseRightRelease)
                    healTimer = 0;
            }
            if (player.statLife <= 25)
                player.AddBuff(ModContent.BuffType<RUUUN>(), 360);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SunDroplet>(), 25)
                .Register();
        }
    }
}