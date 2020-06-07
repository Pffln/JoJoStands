﻿using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.Utilities;
using System.Collections.Generic;

namespace JoJoStands.Items
{
    public class GratefulDeadT4 : ModItem
    {
        public override string Texture
        {
            get { return mod.Name + "/Items/GratefulDeadT1"; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Grateful Dead (Final)");
            Tooltip.SetDefault("Punch enemies to make them age and right-click to grab them!\nSpecial: Spread Gas\nMore effective on hot biomes.\nUsed in Stand Slot");
        }

        public override void SetDefaults()
        {
            item.damage = 90;
            item.width = 32;
            item.height = 32;
            item.useTime = 12;
            item.useAnimation = 12;
            item.useStyle = 5;
            item.maxStack = 1;
            item.knockBack = 2f;
            item.value = 0;
            item.noUseGraphic = true;
            item.rare = 6;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            MyPlayer mPlayer = Main.player[Main.myPlayer].GetModPlayer<MyPlayer>();
            TooltipLine tooltipAddition = new TooltipLine(mod, "Speed", "Punch Speed: " + (11 - mPlayer.standSpeedBoosts));
            tooltips.Add(tooltipAddition);
        }

        public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat)
        {
            mult *= (float)player.GetModPlayer<MyPlayer>().standDamageBoosts;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType("GratefulDeadT3"));
            recipe.AddIngredient(ItemID.ShroomiteBar, 14);
            recipe.AddIngredient(ItemID.Ichor, 20);
            recipe.AddIngredient(mod.ItemType("DeterminedLifeForce"));
            recipe.AddTile(mod.TileType("RemixTableTile"));
            recipe.SetResult(this);
            recipe.AddRecipe();
            recipe.AddIngredient(mod.ItemType("GratefulDeadT3"));
            recipe.AddIngredient(ItemID.ShroomiteBar, 14);
            recipe.AddIngredient(ItemID.CursedFlame, 20);
            recipe.AddIngredient(mod.ItemType("DeterminedLifeForce"));
            recipe.AddTile(mod.TileType("RemixTableTile"));
        }
    }
}
