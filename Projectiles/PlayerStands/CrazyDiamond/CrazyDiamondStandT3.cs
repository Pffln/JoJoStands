using JoJoStands.Buffs.Debuffs;
using JoJoStands.Buffs.EffectBuff;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using JoJoStands.NPCs;
using JoJoStands.DataStructures;

namespace JoJoStands.Projectiles.PlayerStands.CrazyDiamond
{
    public class CrazyDiamondStandT3 : StandClass
    {
        public override int PunchDamage => 84;
        public override int PunchTime => 10;
        public override int AltDamage => 132;
        public override int HalfStandHeight => 51;
        public override int FistWhoAmI => 12;
        public override int TierNumber => 3;
        public override StandAttackType StandType => StandAttackType.Melee;

        private bool healingFrames = false;
        private bool flickFrames = false;
        private bool resetFrame = false;
        private bool blindRage = false;
        private bool healingFramesRepeatTimerOnlyOnce = false;
        private bool returnToOwner = false;
        private bool offsetDirection = false;
        private bool restore = false;

        private int healingFramesRepeatTimer = 0;
        private int onlyOneTarget = 0;
        private int healingTargetNPC = -1;
        private int healingTargetPlayer = -1;
        private int rightClickCooldown = 0;
        private int heal = 0;

        private bool selectEntity = false;
        private bool selectEntity2 = false;
        private int messageCooldown = 0;
        private int messageCooldown2 = 0;

        public override void AI()
        {
            SelectAnimation();
            UpdateStandInfo();
            UpdateStandSync();
            if (shootCount > 0)
                shootCount--;
            Player player = Main.player[Projectile.owner];
            MyPlayer mPlayer = player.GetModPlayer<MyPlayer>();
            if (rightClickCooldown > 0)
                rightClickCooldown--;
            if (mPlayer.standOut)
                Projectile.timeLeft = 2;

            Rectangle rectangle = Rectangle.Empty;

            if (Projectile.owner == player.whoAmI)
                rectangle = new Rectangle((int)(Main.MouseWorld.X - 10), (int)(Main.MouseWorld.Y - 10), 20, 20);

            mPlayer.crazyDiamondRestorationMode = restore;
            if (player.HasBuff(ModContent.BuffType<BlindRage>()))
                blindRage = true;
            if (!player.HasBuff(ModContent.BuffType<BlindRage>()))
                blindRage = false;

            if (blindRage) 
            {
                Punch();
                player.direction = Projectile.spriteDirection;
                Projectile.Center = new Vector2(player.Center.X, player.Center.Y);
            }

            if (!mPlayer.standAutoMode && !blindRage)
            {
                if (Main.mouseLeft && Projectile.owner == Main.myPlayer && !flickFrames && !healingFrames && onlyOneTarget == 0)
                {
                    Punch();
                }
                else
                {
                    if (player.whoAmI == Main.myPlayer)
                        attackFrames = false;
                }
                if (!attackFrames && !healingFrames && onlyOneTarget == 0)
                    StayBehind();
                if (flickFrames)
                    StayBehindWithAbility();
                if (SpecialKeyPressedNoCooldown() && !healingFrames && !flickFrames && onlyOneTarget == 0 && Projectile.owner == Main.myPlayer)
                {
                    restore = !restore;
                    if (restore)
                        Main.NewText("Restoration Mode: Active");
                    else
                        Main.NewText("Restoration Mode: Disabled");
                }
                bool message = false; //select target message
                bool message2 = false; //target too far message
                if (Projectile.owner == player.whoAmI)
                {
                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC npc = Main.npc[n];
                        if (npc.active && !npc.hide && !npc.immortal)
                        {
                            if (npc.Hitbox.Intersects(rectangle))
                            {
                                message = true;
                                if (Vector2.Distance(Projectile.Center, npc.Center) <= 200f)
                                    message2 = true;
                            }
                        }
                    }
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        for (int p = 0; p < Main.maxPlayers; p++)
                        {
                            Player otherPlayer = Main.player[p];
                            if (otherPlayer.active)
                            {
                                if (otherPlayer.Hitbox.Intersects(rectangle))
                                {
                                    message = true;
                                    if (Vector2.Distance(Projectile.Center, otherPlayer.Center) <= 200f)
                                        message2 = true;
                                }
                            }
                        }
                    }
                }
                if (!restore)
                {
                    if (Main.mouseRight && shootCount <= 0 && Projectile.owner == Main.myPlayer && !healingFrames && onlyOneTarget == 0)
                    {
                        int bulletIndex = GetPlayerAmmo(player);
                        if (bulletIndex != -1)
                        {
                            Item bulletItem = player.inventory[bulletIndex];
                            if (bulletItem.shoot != -1)
                            {
                                flickFrames = true;
                                if (Projectile.frame == 2)
                                {
                                    shootCount += 40;
                                    Main.mouseLeft = false;
                                    Vector2 shootVel = Main.MouseWorld - (Projectile.Center - new Vector2(0, 18f));
                                    if (shootVel == Vector2.Zero)
                                        shootVel = new Vector2(0f, 1f);

                                    shootVel.Normalize();
                                    shootVel *= 12f;

                                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2 (0, 18f), shootVel, bulletItem.shoot, (int)(AltDamage * mPlayer.standDamageBoosts), bulletItem.knockBack, Projectile.owner, Projectile.whoAmI);
                                    Main.projectile[proj].GetGlobalProjectile<JoJoGlobalProjectile>().kickedByStarPlatinum = true;
                                    Main.projectile[proj].netUpdate = true;
                                    Projectile.netUpdate = true;
                                    SoundStyle item41 = SoundID.Item41;
                                    item41.Pitch = 2.8f;
                                    SoundEngine.PlaySound(item41, player.Center);
                                    if (bulletItem.type != ItemID.EndlessMusketPouch)
                                        player.ConsumeItem(bulletItem.type);
                                }
                            }
                        }
                    }
                    if (SecondSpecialKeyPressed() && onlyOneTarget == 0 && Projectile.owner == Main.myPlayer) 
                    {
                        player.AddBuff(ModContent.BuffType<BlindRage>(), 600);
                        EmoteBubble.NewBubble(1, new WorldUIAnchor(player), 600);
                        SoundEngine.PlaySound(new SoundStyle("JoJoStands/Sounds/GameSounds/PoseSound")); 
                    }
                }
                if (restore)
                {
                    if (Main.mouseRight && rightClickCooldown == 0 && mPlayer.crazyDiamondDestroyedTileData.Count > 0 && Projectile.owner == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(new SoundStyle("JoJoStands/Sounds/GameSounds/RestoreSound"));
                        rightClickCooldown += 180;
                    }
                    if (rightClickCooldown == 10)
                    {
                        rightClickCooldown -= 1;
                        mPlayer.crazyDiamondDestroyedTileData.ForEach(DestroyedTileData.Restore);
                        mPlayer.crazyDiamondMessageCooldown = 0;
                        mPlayer.crazyDiamondDestroyedTileData.Clear();
                    }
                    if (SecondSpecialKeyPressed() && onlyOneTarget == 0 && Projectile.owner == Main.myPlayer)
                    {
                        for (int n = 0; n < Main.maxNPCs; n++)
                        {
                            NPC npc = Main.npc[n];
                            if (npc.active && !npc.hide && !npc.immortal)
                            {
                                if (npc.Hitbox.Intersects(rectangle))
                                {
                                    if (Vector2.Distance(Projectile.Center, npc.Center) > 200f && messageCooldown2 == 0 && !message2)
                                        selectEntity2 = true;
                                    if (Vector2.Distance(Projectile.Center, npc.Center) <= 200f && !healingFrames && onlyOneTarget < 1)
                                    {
                                        onlyOneTarget += 1;
                                        healingTargetNPC = npc.whoAmI;
                                    }
                                }
                                if (!npc.Hitbox.Intersects(rectangle) && messageCooldown == 0 && !message)
                                    selectEntity = true;
                            }
                        }
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            for (int p = 0; p < Main.maxPlayers; p++)
                            {
                                Player otherPlayer = Main.player[p];
                                if (otherPlayer.active)
                                {
                                    if (otherPlayer.Hitbox.Intersects(rectangle))
                                    {
                                        if (Vector2.Distance(Projectile.Center, otherPlayer.Center) > 200f && messageCooldown2 == 0 && !message2)
                                            selectEntity2 = true;
                                        if (Vector2.Distance(Projectile.Center, otherPlayer.Center) <= 200f && onlyOneTarget < 1 && !healingFrames && otherPlayer.whoAmI != player.whoAmI)
                                        {
                                            onlyOneTarget += 1;
                                            healingTargetPlayer = otherPlayer.whoAmI;
                                        }
                                    }
                                    if (!otherPlayer.Hitbox.Intersects(rectangle) && messageCooldown == 0 && !message)
                                        selectEntity = true;
                                }
                            }
                        }
                    }
                    if (onlyOneTarget > 0)
                    {
                        float offset = 0f;
                        float offset2 = 0f;
                        if (offsetDirection)
                            offset = -60f*Projectile.spriteDirection;
                        if (Projectile.spriteDirection == -1)
                            offset2 = 24f;
                        if (healingTargetNPC != -1)
                        {
                            NPC npc = Main.npc[healingTargetNPC];
                            if (!healingFrames && !returnToOwner)
                            {
                                if (npc.Center.X > Projectile.Center.X)
                                    Projectile.spriteDirection = 1;
                                if (npc.Center.X < Projectile.Center.X)
                                    Projectile.spriteDirection = -1;
                                Projectile.velocity = npc.Center - Projectile.Center;
                                Projectile.velocity.Normalize();
                                Projectile.velocity *= 6f;
                                if (Vector2.Distance(Projectile.Center, npc.Center) <= 20f)
                                {
                                    if (Projectile.spriteDirection != player.direction)
                                        offsetDirection = true;
                                    Projectile.frame = 0;
                                    healingFrames = true;
                                }
                                Projectile.netUpdate = true;
                            }
                            if (healingFrames && !returnToOwner)
                            {
                                Projectile.position = new Vector2(npc.Center.X - 10f - offset - offset2, npc.Center.Y - 20f);
                                if (Projectile.frame == 0 && !healingFramesRepeatTimerOnlyOnce)
                                {
                                    healingFramesRepeatTimer += 1;
                                    healingFramesRepeatTimerOnlyOnce = true;
                                }
                                if (Projectile.frame != 0)
                                    healingFramesRepeatTimerOnlyOnce = false;
                                if (healingFramesRepeatTimer >= 4)
                                {
                                    offsetDirection = false;
                                    healingFrames = false;
                                    healingFramesRepeatTimerOnlyOnce = false;
                                    onlyOneTarget = 0;
                                    healingFramesRepeatTimer = 0;
                                    int heal = npc.lifeMax - npc.life;
                                    if (npc.HasBuff(ModContent.BuffType<MissingOrgans>()))
                                        heal = 0;
                                    npc.AddBuff(ModContent.BuffType<Restoration>(), 360);
                                    if (npc.townNPC && heal > 0)
                                        npc.GetGlobalNPC<JoJoGlobalNPC>().crazyDiamondFullHealth = true;
                                    else
                                        npc.lifeMax = npc.life;
                                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(45));
                                }
                                Projectile.netUpdate = true;
                            }
                            if (Vector2.Distance(npc.Center, player.Center) > 200f)
                            {
                                returnToOwner = true;
                                healingFrames = false;
                            }
                        }
                        if (healingTargetPlayer != -1)
                        {
                            Player otherPlayer = Main.player[healingTargetPlayer];
                            if (!healingFrames && !returnToOwner)
                            {
                                if (otherPlayer.Center.X > Projectile.Center.X)
                                    Projectile.spriteDirection = 1;
                                if (otherPlayer.Center.X < Projectile.Center.X)
                                    Projectile.spriteDirection = -1;
                                Projectile.velocity = otherPlayer.Center - Projectile.Center;
                                Projectile.velocity.Normalize();
                                Projectile.velocity *= 6f;
                                if (Vector2.Distance(Projectile.Center, otherPlayer.Center) <= 20f)
                                {
                                    if (Projectile.spriteDirection != player.direction)
                                        offsetDirection = true;
                                    Projectile.frame = 0;
                                    healingFrames = true;
                                }
                                Projectile.netUpdate = true;
                            }
                            if (healingFrames && !returnToOwner)
                            {
                                Projectile.position = new Vector2(otherPlayer.Center.X - 10f - offset - offset2, otherPlayer.Center.Y - 20f);
                                if (Projectile.frame == 0 && !healingFramesRepeatTimerOnlyOnce)
                                {
                                    healingFramesRepeatTimer += 1;
                                    healingFramesRepeatTimerOnlyOnce = true;
                                }
                                if (Projectile.frame != 0)
                                    healingFramesRepeatTimerOnlyOnce = false;
                                if (healingFramesRepeatTimer >= 4)
                                {
                                    offsetDirection = false;
                                    healingFrames = false;
                                    healingFramesRepeatTimerOnlyOnce = false;
                                    onlyOneTarget = 0;
                                    healingFramesRepeatTimer = 0;
                                    heal = otherPlayer.statLifeMax - otherPlayer.statLife;
                                    if (otherPlayer.HasBuff(ModContent.BuffType<MissingOrgans>()))
                                        heal = 0;
                                    if (otherPlayer.whoAmI != player.whoAmI)
                                    {
                                        if (heal > 0)
                                            otherPlayer.Heal(heal);
                                        otherPlayer.AddBuff(ModContent.BuffType<Restoration>(), 360);
                                    }
                                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(45));
                                }
                                Projectile.netUpdate = true;
                            }
                            if (Vector2.Distance(otherPlayer.Center, player.Center) > 200f)
                            {
                                returnToOwner = true;
                                healingFrames = false;
                            }
                        }
                        if (returnToOwner)
                        {
                            if (Projectile.velocity.X < 0)
                                Projectile.spriteDirection = -1;
                            else
                                Projectile.spriteDirection = 1;
                            Projectile.velocity = player.Center - Projectile.Center;
                            Projectile.velocity.Normalize();
                            Projectile.velocity *= 6f + player.moveSpeed;
                            idleFrames = true;
                            if (Vector2.Distance(Projectile.Center, player.Center) <= 20f)
                            {
                                returnToOwner = false;
                                offsetDirection = false;
                                healingFrames = false;
                                healingFramesRepeatTimerOnlyOnce = false;
                                onlyOneTarget = 0;
                                healingFramesRepeatTimer = 0;
                            }
                            Projectile.netUpdate = true;
                        }
                        if (onlyOneTarget == 0)
                        {
                            healingTargetNPC = -1;
                            healingTargetPlayer = -1;
                        }
                    }
                }
            }
            if (selectEntity2)
            {
                Main.NewText("Target too far");
                selectEntity2 = false;
                messageCooldown2 += 90;
            }
            if (selectEntity)
            {
                Main.NewText("Select target with mouse");
                selectEntity = false;
                messageCooldown += 90;
            }
            if (messageCooldown > 0)
                messageCooldown--;
            if (messageCooldown2 > 0)
                messageCooldown2--;
            if (restore)
                Lighting.AddLight(Projectile.position, 11);
            if (mPlayer.standAutoMode && onlyOneTarget == 0)
            {
                returnToOwner = false;
                healingFrames = false;
                onlyOneTarget = 0;
                healingFramesRepeatTimer = 0;
                healingFramesRepeatTimerOnlyOnce = false;
                BasicPunchAI();
            }
            if (player.teleporting)
                Projectile.position = player.position;

        }

        private int GetPlayerAmmo(Player player)
        {
            int ammoType = -1;
            for (int i = 54; i < 58; i++)
            {
                Item Item = player.inventory[i];

                if (Item.ammo == AmmoID.Bullet && Item.stack > 0)
                {
                    ammoType = i;
                    break;
                }
            }
            if (ammoType == -1)
            {
                for (int i = 0; i < 54; i++)
                {
                    Item Item = player.inventory[i];
                    if (Item.ammo == AmmoID.Bullet && Item.stack > 0)
                    {
                        ammoType = i;
                        break;
                    }
                }
            }
            return ammoType;
        }

        public override void SelectAnimation()
        {
            if (attackFrames)
            {
                idleFrames = false;
                PlayAnimation("Attack");
            }
            if (idleFrames)
            {
                attackFrames = false;
                PlayAnimation("Idle");
            }
            if (flickFrames)
            {
                if (!resetFrame)
                {
                    resetFrame = true;
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
                idleFrames = false;
                attackFrames = false;
                PlayAnimation("Flick");
            }
            if (healingFrames)
            {
                idleFrames = false;
                attackFrames = false;
                PlayAnimation("Heal");
            }
            if (Main.player[Projectile.owner].GetModPlayer<MyPlayer>().posing)
            {
                idleFrames = false;
                attackFrames = false;
                PlayAnimation("Pose");
            }
        }

        public override void AnimationCompleted(string animationName)
        {
            if (resetFrame && animationName == "Flick")
            {
                idleFrames = true;
                flickFrames = false;
                resetFrame = false;
            }
        }

        public override void PlayAnimation(string animationName)
        {
            MyPlayer mPlayer = Main.player[Projectile.owner].GetModPlayer<MyPlayer>();
            string pathAddition = "";
            if (restore)
                pathAddition = "Restoration_";

            if (Main.netMode != NetmodeID.Server)
                standTexture = GetStandTexture("JoJoStands/Projectiles/PlayerStands/CrazyDiamond", "/CrazyDiamond_" + pathAddition + animationName);

            if (animationName == "Idle")
            {
                AnimateStand(animationName, 4, 12, true);
            }
            if (animationName == "Attack")
            {
                AnimateStand(animationName, 4, newPunchTime, true);
            }
            if (animationName == "Flick")
            {
                AnimateStand(animationName, 4, 10, false);
            }
            if (animationName == "Pose")
            {
                AnimateStand(animationName, 4, 12, true);
            }
            if (animationName == "Heal")
            {
                AnimateStand(animationName, 4, 12, true);
            }
        }
        public override void SendExtraStates(BinaryWriter writer)
        {
            writer.Write(healingFrames);
            writer.Write(flickFrames);
            writer.Write(blindRage);
            writer.Write(returnToOwner);
            writer.Write(restore);
            writer.Write(onlyOneTarget);
            writer.Write(healingTargetNPC);
            writer.Write(healingTargetPlayer);
            writer.Write(rightClickCooldown);
            writer.Write(healingFramesRepeatTimer);
        }

        public override void ReceiveExtraStates(BinaryReader reader)
        {
            healingFrames = reader.ReadBoolean();
            flickFrames = reader.ReadBoolean();
            blindRage = reader.ReadBoolean();
            returnToOwner = reader.ReadBoolean();
            restore = reader.ReadBoolean();
            onlyOneTarget = reader.ReadInt32();
            healingTargetNPC = reader.ReadInt32();
            healingTargetPlayer = reader.ReadInt32();
            rightClickCooldown = reader.ReadInt32();
            healingFramesRepeatTimer = reader.ReadInt32();
        }
    }
}