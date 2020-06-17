using System;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;


namespace JoJoStands.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class ViralArmorKabuto : ModItem
	{
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Viral Helmet (Melee)");
            Tooltip.SetDefault("A helmet created from a far-off alloy, in the style of a far-off equipment.\nStand Damage Increase: +4%\nSet Bonus: +5% Stand Damage");
        }

        public override void SetDefaults()
        {
            item.width = 26;
            item.height = 28;
            item.value = Item.buyPrice(0, 3, 50, 0);
            item.rare = 8;
            item.defense = 7;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == mod.ItemType("ViralArmorKaruta") && legs.type == mod.ItemType("ViralArmorTabi");
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<MyPlayer>().standDamageBoosts += 0.05;
        }

        public override void UpdateEquip(Player player)
        {
            MyPlayer mPlayer = player.GetModPlayer<MyPlayer>();
            mPlayer.standDamageBoosts += 0.04;
            /*for (int i = 0; i < player.inventory.Length; i++)
            {
                if (mPlayer.standType == 0 && player.inventory[i].type == item.type)
                {
                    player.inventory[i].type = mod.ItemType("ViralArmorHelmetNeutral");
                }
                if (mPlayer.standType == 2 && player.inventory[i].type == item.type)
                {
                    player.inventory[i].type = mod.ItemType("ViralArmorHelmet");
                }
            }*/
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType("ViralMeteoriteBar"), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}