using JoJoStands.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace JoJoStands.Projectiles.PlayerStands.HierophantGreen
{
    public class HierophantGreenStandT3 : StandClass
    {
        public override int ShootTime => 20;
        public override int ProjectileDamage => 56;
        public override int HalfStandHeight => 30;
        public override int StandOffset => 0;
        public override StandAttackType StandType => StandAttackType.Ranged;
        public override string PoseSoundName => "ItsTheVictorWhoHasJustice";
        public override string SpawnSoundName => "Hierophant Green";
        public override bool CanUseSaladDye => true;

        private bool spawningField = false;
        private int numberSpawned = 0;
        private bool pointShot = false;
        private bool remoteControlled = false;
        private bool linkShotForSpecial = false;
        private Vector2 formPosition = Vector2.Zero;

        private const float MaxRemoteModeDistance = 45f * 16f;

        public override void AI()
        {
            SelectAnimation();
            UpdateStandInfo();
            UpdateStandSync();
            if (shootCount > 0)
                shootCount--;

            Player player = Main.player[Projectile.owner];
            MyPlayer mPlayer = player.GetModPlayer<MyPlayer>();
            if (mPlayer.standOut)
                Projectile.timeLeft = 2;

            Lighting.AddLight((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f), 0.6f, 0.9f, 0.3f);
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 35, Projectile.velocity.X * -0.5f, Projectile.velocity.Y * -0.5f);
            Projectile.scale = ((50 - player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldStringPointConnector>()]) * 2f) / 100f;

            if (!mPlayer.standAutoMode && !remoteControlled)
            {
                if (Main.mouseLeft && Projectile.scale >= 0.5f && Projectile.owner == Main.myPlayer)
                {
                    if (!mPlayer.canStandBasicAttack)
                        return;

                    attackFrames = true;
                    idleFrames = false;
                    if (shootCount <= 0)
                    {
                        shootCount += newShootTime;
                        Vector2 shootVel = Main.MouseWorld - Projectile.Center;
                        if (shootVel == Vector2.Zero)
                            shootVel = new Vector2(0f, 1f);

                        shootVel.Normalize();
                        shootVel *= ProjectileSpeed;

                        float numberProjectiles = 5;
                        float rotation = MathHelper.ToRadians(25);
                        float randomSpeedOffset = (100f + Main.rand.NextFloat(-6f, 6f)) / 100f;
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .2f;
                            perturbedSpeed *= randomSpeedOffset;
                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<Emerald>(), newProjectileDamage, 3f, player.whoAmI);
                            Main.projectile[proj].netUpdate = true;
                        }
                        SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                        Projectile.netUpdate = true;
                    }
                }
                if (!Main.mouseLeft && player.whoAmI == Main.myPlayer)        //The reason it's not an else is because it would count the owner part too
                {
                    idleFrames = true;
                    attackFrames = false;
                }
                if (!attackFrames)
                    StayBehind();
                else
                    GoInFront();

                if (Main.mouseRight && shootCount <= 0 && Projectile.scale >= 0.5f && !playerHasAbilityCooldown && Projectile.owner == Main.myPlayer)
                {
                    shootCount += 30;
                    Vector2 shootVel = Main.MouseWorld - Projectile.Center;
                    shootVel.Normalize();
                    shootVel *= ProjectileSpeed;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, ModContent.ProjectileType<BindingEmeraldString>(), newProjectileDamage / 2, 0f, Projectile.owner, 25);
                    Main.projectile[proj].netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(5));
                }

                if (SpecialKeyPressed() && player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldStringPointConnector>()] <= 0 && !spawningField)
                {
                    spawningField = true;
                    formPosition = Projectile.position;
                    if (JoJoStands.SoundsLoaded)
                        SoundEngine.PlaySound(new SoundStyle("JoJoStandsSounds/Sounds/SoundEffects/EmeraldSplash"), Projectile.Center);
                }
                if (SecondSpecialKeyPressedNoCooldown() && shootCount <= 0)
                {
                    shootCount += 30;
                    remoteControlled = true;
                }
            }
            if (!mPlayer.standAutoMode && remoteControlled)
            {
                mPlayer.standRemoteMode = true;
                float halfScreenWidth = (float)Main.screenWidth / 2f;
                float halfScreenHeight = (float)Main.screenHeight / 2f;
                mPlayer.standRemoteModeCameraPosition = Projectile.Center - new Vector2(halfScreenWidth, halfScreenHeight);
                if (Main.mouseLeft && Projectile.owner == Main.myPlayer)
                {
                    Projectile.velocity = Main.MouseWorld - Projectile.Center;
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 4.5f;

                    Projectile.direction = 1;
                    if (Main.MouseWorld.X < Projectile.Center.X)
                    {
                        Projectile.direction = -1;
                    }
                    Projectile.netUpdate = true;
                    Projectile.spriteDirection = Projectile.direction;
                    LimitDistance(MaxRemoteModeDistance);
                }
                if (!Main.mouseLeft && Projectile.owner == Main.myPlayer)
                {
                    Projectile.velocity *= 0.78f;
                    Projectile.netUpdate = true;
                }
                if (Main.mouseRight && Projectile.owner == Main.myPlayer)
                {
                    attackFrames = true;
                    idleFrames = false;
                    if (shootCount <= 0)
                    {
                        shootCount += newShootTime;

                        Projectile.direction = 1;
                        if (Main.MouseWorld.X < Projectile.Center.X)
                        {
                            Projectile.direction = -1;
                        }
                        Projectile.spriteDirection = Projectile.direction;

                        Vector2 shootVel = Main.MouseWorld - Projectile.Center;
                        if (shootVel == Vector2.Zero)
                        {
                            shootVel = new Vector2(0f, 1f);
                        }
                        shootVel.Normalize();
                        shootVel *= ProjectileSpeed;

                        float numberProjectiles = 5;
                        float rotation = MathHelper.ToRadians(25);
                        float randomSpeedOffset = (100f + Main.rand.NextFloat(-6f, 6f)) / 100f;
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .2f;
                            perturbedSpeed *= randomSpeedOffset;
                            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<Emerald>(), newProjectileDamage, 3f, player.whoAmI);
                            Main.projectile[proj].netUpdate = true;
                        }
                        SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                        Projectile.netUpdate = true;
                    }
                }
                else
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        attackFrames = false;
                        idleFrames = true;
                    }
                }
                if (SpecialKeyPressed() && shootCount <= 0 && Projectile.scale >= 0.5f)
                {
                    pointShot = !pointShot;
                    int connectorType = ModContent.ProjectileType<EmeraldStringPoint>();
                    if (!pointShot)
                        connectorType = ModContent.ProjectileType<EmeraldStringPointConnector>();

                    shootCount += 15;
                    Vector2 shootVel = Main.MouseWorld - Projectile.Center;
                    if (shootVel == Vector2.Zero)
                    {
                        shootVel = new Vector2(0f, 1f);
                    }
                    shootVel.Normalize();
                    shootVel *= ProjectileSpeed;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, connectorType, 0, 3f, player.whoAmI);
                    Main.projectile[proj].netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                    Projectile.netUpdate = true;
                }

                if (SecondSpecialKeyPressedNoCooldown() && shootCount <= 0)
                {
                    shootCount += 30;
                    remoteControlled = false;
                }
            }
            if (mPlayer.standAutoMode)
            {
                StayBehind();

                NPC target = FindNearestTarget(350f);
                if (target != null)
                {
                    attackFrames = true;
                    idleFrames = false;
                    Projectile.direction = 1;
                    if (target.position.X - Projectile.Center.X < 0)
                    {
                        Projectile.direction = -1;
                    }
                    Projectile.spriteDirection = Projectile.direction;
                    if (shootCount <= 0)
                    {
                        shootCount += newShootTime;
                        SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Vector2 shootVel = target.position - Projectile.Center;
                            if (shootVel == Vector2.Zero)
                            {
                                shootVel = new Vector2(0f, 1f);
                            }
                            shootVel.Normalize();
                            shootVel *= ProjectileSpeed;

                            float numberProjectiles = 5;
                            float rotation = MathHelper.ToRadians(25);
                            float randomSpeedOffset = (100f + Main.rand.NextFloat(-6f, 6f)) / 100f;
                            for (int i = 0; i < numberProjectiles; i++)
                            {
                                Vector2 perturbedSpeed = shootVel.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .2f;
                                perturbedSpeed *= randomSpeedOffset;
                                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<Emerald>(), (int)((ProjectileDamage * mPlayer.standDamageBoosts) * 0.9f), 3f, player.whoAmI);
                                Main.projectile[proj].netUpdate = true;
                            }
                            Projectile.netUpdate = true;
                        }
                    }
                }
                else
                {
                    idleFrames = true;
                    attackFrames = false;
                }
                LimitDistance(MaxRemoteModeDistance);
            }

            if (spawningField && Projectile.owner == Main.myPlayer)
            {
                float randomRadius = Main.rand.NextFloat(-20f, 21f);
                Vector2 pointPosition = formPosition + (randomRadius.ToRotationVector2() * 288f);     //33 tiles

                if (numberSpawned < 70 && shootCount <= 0 && !linkShotForSpecial)        //35 tendrils, the number spawned limit /2 is the wanted amount
                {
                    shootCount += 5;
                    numberSpawned += 1;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pointPosition, Vector2.Zero, ModContent.ProjectileType<EmeraldStringPoint>(), 0, 2f, player.whoAmI);
                    Main.projectile[proj].netUpdate = true;
                    Main.projectile[proj].tileCollide = false;
                    linkShotForSpecial = true;
                }
                if (numberSpawned < 70 && shootCount <= 0 && linkShotForSpecial)
                {
                    shootCount += 5;
                    numberSpawned += 1;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pointPosition, Vector2.Zero, ModContent.ProjectileType<EmeraldStringPointConnector>(), 0, 2f, player.whoAmI, 24f);
                    Main.projectile[proj].netUpdate = true;
                    Main.projectile[proj].tileCollide = false;
                    linkShotForSpecial = false;
                }
                if (numberSpawned >= 70)
                {
                    numberSpawned = 0;
                    spawningField = false;
                    formPosition = Vector2.Zero;
                    player.AddBuff(ModContent.BuffType<AbilityCooldown>(), mPlayer.AbilityCooldownTime(30));
                }
            }
        }

        public override void SendExtraStates(BinaryWriter writer)
        {
            writer.Write(remoteControlled);
        }

        public override void ReceiveExtraStates(BinaryReader reader)
        {
            remoteControlled = reader.ReadBoolean();
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
            if (Main.player[Projectile.owner].GetModPlayer<MyPlayer>().poseMode || spawningField)
            {
                idleFrames = false;
                attackFrames = false;
                PlayAnimation("Pose");
            }
        }

        public override void PlayAnimation(string animationName)
        {
            if (Main.netMode != NetmodeID.Server)
                standTexture = GetStandTexture("JoJoStands/Projectiles/PlayerStands/HierophantGreen", "/HierophantGreen_" + animationName);

            if (animationName == "Idle")
            {
                AnimateStand(animationName, 3, 20, true);
            }
            if (animationName == "Attack")
            {
                AnimateStand(animationName, 3, 15, true);
            }
            if (animationName == "Pose")
            {
                AnimateStand(animationName, 2, 15, true);
            }
        }
    }
}