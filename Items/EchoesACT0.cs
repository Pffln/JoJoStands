using JoJoStands.Items.CraftingMaterials;
using JoJoStands.Tiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace JoJoStands.Items
{
    public class EchoesACT0 : StandItemClass
    {
        public override int standSpeed => 2;
        public override int standType => 1;
        public override string standProjectileName => "Echoes";
        public override int standTier => 1;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Echoes (ACT 0)");
            Tooltip.SetDefault("Left-click to... throw? \nOnly Strong Will can help you!");
        }

        public override void SetDefaults()
        {
            Item.damage = 108;
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = 0;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<StandArrow>())
                .AddIngredient(ModContent.ItemType<WillToChange>())
                .AddTile(ModContent.TileType<RemixTableTile>())
                .Register();
        }
    }
}
