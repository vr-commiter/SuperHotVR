using MelonLoader;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using MyTrueGear;
using System.Threading;

namespace SuperHotVR_TrueGear
{
    public static class BuildInfo
    {
        public const string Name = "SuperHotVR_TrueGear"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "TrueGear Mod for SuperHotVR"; // Description for the Mod.  (Set as null if none)
        public const string Author = "HuangLY"; // Author of the Mod.  (MUST BE SET)
        public const string Company = "TrueGear"; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class SuperHotVR_TrueGear : MelonMod
    {
        private static bool isLeftGun = false;
        private static bool isRightGun = false;
        private static bool isLeftItem = false;
        private static bool isRightItem = false;
        private static bool isLeftPunching = false;
        private static bool isMindActive = false;
        private static VrHandController pickupHandController = null;

        private static bool canPunch = true;
        private static bool canMelee = true;

        private static TrueGearMod _TrueGear = null;

        public override void OnInitializeMelon()
        {
            _TrueGear = new TrueGearMod();
            MelonLogger.Msg("OnApplicationStart");
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(SuperHotVR_TrueGear));
        }

        public static KeyValuePair<float, float> GetAngle(Transform transform, Vector3 hitPoint)
        {
            Vector3 hitPos = hitPoint - transform.position;
            float hitAngle = Mathf.Atan2(hitPos.x, hitPos.z) * Mathf.Rad2Deg;
            if (hitAngle < 0f)
            {
                hitAngle += 360f;
            }
            float verticalDifference = hitPoint.y - transform.position.y;
            return new KeyValuePair<float, float>(hitAngle, verticalDifference);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VrPickingSystem), "PickupItem", new Type[] { typeof(VrHandController), typeof(PickupProxy), typeof(GrabTypes) })]
        public static bool VrPickingSystem_PickupItem_PrePatch(VrHandController handController, PickupProxy pickup, GrabTypes grabType)
        {
            if (pickup.CannotBePickedUpByPlayer)
            {
                return true;
            }
            if (handController.CurrentPickup == null && MonoBehaviourSingleton<OmniVRMainScene>.Instance.IsHeadSetOnPlayersHead)
            {
                if (handController.Controller == Controller.LeftController)
                {
                    MelonLogger.Msg("-------------------------------------");
                    MelonLogger.Msg("PickUpItemLeft");
                    _TrueGear.Play("PickUpItemLeft");
                }
                if (handController.Controller == Controller.RightController)
                {
                    MelonLogger.Msg("-------------------------------------");
                    MelonLogger.Msg("PickUpItemRight");
                    _TrueGear.Play("PickUpItemRight");
                }
                if (pickup.IsGunPickup)
                {
                    Gun gun = (pickup.GetPickup() as GunPickup).Gun;
                    if (gun != null && (gun is UziGun || gun is ShotGun))
                    {
                        if (handController.Controller == Controller.LeftController)
                        {
                            isLeftGun = true;
                        }
                        if (handController.Controller == Controller.RightController)
                        {
                            isRightGun = true;
                        }
                    }
                }
                else if(!pickup.IsGunPickup)
                {
                    if (handController.Controller == Controller.LeftController)
                    {
                        isLeftItem = true;
                    }
                    if (handController.Controller == Controller.RightController)
                    {
                        isRightItem = true;
                    }
                }
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VrPickupDroppingSystem), "DropItem")]
        public static bool VrPickupDroppingSystem_DropItem_PrePatch(VrHandController handController)
        {
            if (handController.CurrentPickup != null)
            {
                if (handController.CurrentPickup.IsPhysicPickupObject)
                {
                    if (handController.Controller == Controller.LeftController)
                    {
                        MelonLogger.Msg("-------------------------------------");
                        MelonLogger.Msg("ThrowItemLeft");
                        _TrueGear.Play("ThrowItemLeft");
                    }
                    if (handController.Controller == Controller.RightController)
                    {
                        MelonLogger.Msg("-------------------------------------");
                        MelonLogger.Msg("ThrowItemRight");
                        _TrueGear.Play("ThrowItemRight");
                    }
                }
                else
                {
                    if (handController.Controller == Controller.LeftController)
                    {
                        MelonLogger.Msg("-------------------------------------");
                        MelonLogger.Msg("DropItemLeft");
                        _TrueGear.Play("DropItemLeft");
                    }
                    if (handController.Controller == Controller.RightController)
                    {
                        MelonLogger.Msg("-------------------------------------");
                        MelonLogger.Msg("DropItemRight");
                        _TrueGear.Play("DropItemRight");
                    }
                }


                if (handController.CurrentPickup.IsGunPickup)
                {
                    Gun gun = (handController.CurrentPickup.GetPickup() as GunPickup).Gun;
                    if (gun != null && (gun is UziGun || gun is ShotGun))
                    {
                        if (handController.Controller == Controller.LeftController)
                        {
                            isLeftGun = false;
                        }
                        if (handController.Controller == Controller.RightController)
                        {
                            isRightGun = false;
                        }
                    }
                }
                else if (!handController.CurrentPickup.IsGunPickup)
                {
                    if (handController.Controller == Controller.LeftController)
                    {
                        isLeftItem = false;
                    }
                    if (handController.Controller == Controller.RightController)
                    {
                        isRightItem = false;
                    }
                }
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ShootingSystem), "UpdateShootingFor")]
        public static void ShootingSystem_UpdateShootingFor_PostPatch(ShootingSystem __instance, VrHandController handController)
        {
            if (__instance.PickupCanShot(handController.CurrentPickup) && handController.InteractionReady)
            {
                if (VrInputSystem.GetTriggerDown(handController.Controller))
                {
                    GunPickup gunPickup = handController.CurrentPickup.GetPickup() as GunPickup;
                    if (!(gunPickup.Gun.ammoCount == 0))
                    {
                        if (!((gunPickup.Gun is UziGun) || (gunPickup.Gun is ShotGun)))
                        {
                            if (handController.Controller == Controller.LeftController)
                            {
                                MelonLogger.Msg("-------------------------------------");
                                MelonLogger.Msg("PisotLeftHandShoot");
                                _TrueGear.Play("PisotLeftHandShoot");
                            }
                            if (handController.Controller == Controller.RightController)
                            {
                                MelonLogger.Msg("-------------------------------------");
                                MelonLogger.Msg("PisotRightHandShoot");
                                _TrueGear.Play("PisotRightHandShoot");
                            }
                        }
                    }
                    else
                    {
                        if (handController.Controller == Controller.LeftController)
                        {
                            MelonLogger.Msg("-------------------------------------");
                            MelonLogger.Msg("LeftGunNoAmmo");
                            _TrueGear.Play("LeftGunNoAmmo");
                        }
                        if (handController.Controller == Controller.RightController)
                        {
                            MelonLogger.Msg("-------------------------------------");
                            MelonLogger.Msg("RightGunNoAmmo");
                            _TrueGear.Play("RightGunNoAmmo");
                        }
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UziGun), "ShootUziBullets")]
        public static void UziGun_ShootUziBullets_PostPatch()
        {
            if (isLeftGun)
            {
                MelonLogger.Msg("-------------------------------------");
                MelonLogger.Msg("UziLeftHandShoot");
                _TrueGear.Play("UziLeftHandShoot");
            }
            if (isRightGun)
            {
                MelonLogger.Msg("-------------------------------------");
                MelonLogger.Msg("UziRightHandShoot");
                _TrueGear.Play("UziRightHandShoot");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ShotGun), "Fire")]
        public static void ShotGun_Fire_PostPatch()
        {
            if (isLeftGun)
            {
                MelonLogger.Msg("-------------------------------------");
                MelonLogger.Msg("ShotGunLeftHandShoot");
                _TrueGear.Play("ShotGunLeftHandShoot");
            }
            if (isRightGun)
            {
                MelonLogger.Msg("-------------------------------------");
                MelonLogger.Msg("ShotGunRightHandShoot");
                _TrueGear.Play("ShotGunRightHandShoot");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerActionsVR), "Kill")]
        public static bool PlayerActionsVR_Kill_PrePatch(PlayerActionsVR __instance, Vector3 killerObjectPosition)
        {
            //var angle = GetAngle(__instance.transform,killerObjectPosition);
            if (!__instance.IsDying)
            {
                MelonLogger.Msg("-------------------------------------");
                //MelonLogger.Msg($"transform :{__instance.transform.position} | killerObjectPosition :{killerObjectPosition}");
                //MelonLogger.Msg($"PlayerDeath : angle :{angle.Key} | vertical :{angle.Value}");
                MelonLogger.Msg("PlayerDeath");
                _TrueGear.Play("PlayerDeath");
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VrPunchingSystem), "PerformPunchUpdate")]
        public static bool VrPunchingSystem_PerformPunchUpdate_PrePatch(VrHandController handController)
        {
            if (handController.Controller == Controller.LeftController)
            {
                isLeftPunching = true;
            }
            if (handController.Controller == Controller.RightController)
            {
                isLeftPunching = false;
            }
            return true;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(VrPunchingSystem), "PerformPunchUpdate")]
        public static IEnumerable<CodeInstruction> PerformPunchUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var meleeeAttackReactionMethod = typeof(PejAiBodyPart).GetMethod("MeleeeAttackReaction");
            var punchingMethod = typeof(SuperHotVR_TrueGear).GetMethod("PunchingMethod");

            for (int i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand == meleeeAttackReactionMethod)
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, punchingMethod));
                    i += 1;
                }
            }
            return codes.AsEnumerable();
        }
        public static void PunchingMethod()
        {
            if (canPunch)
            { 
                canPunch = false;
                MelonLogger.Msg("------------------------");
                if (isLeftPunching)
                {
                    MelonLogger.Msg("LeftPunching");
                    _TrueGear.Play("LeftPunching");
                }
                else
                {
                    MelonLogger.Msg("RightPunching");
                    _TrueGear.Play("RightPunching");
                }
                Timer pubchTimer = new Timer(PunchTimerCallBack,null,50,Timeout.Infinite);
            }            
        }

        private static void PunchTimerCallBack(System.Object o)
        { 
            canPunch = true;
        }
        

        [HarmonyPostfix, HarmonyPatch(typeof(MindDeathWaveSystem), "DualModeDeathWaveOnTarget")]
        private static void MindDeathWaveSystem_DualModeDeathWaveOnTarget_Postfix(MindDeathWaveSystem __instance,MindDeathWaveComponent firstController, MindDeathWaveComponent secondController)
        {
            if (__instance.CheckHandsDistanace(firstController, secondController))
            {
                if (!(firstController.currEnemyTarget == null || secondController.currEnemyTarget == null) && !isMindActive)
                {
                    isMindActive = true;
                    MelonLogger.Msg("------------------------");
                    MelonLogger.Msg("StartMindDeathWave");
                    _TrueGear.StartMindDeathWave();
                }
                else if ((firstController.currEnemyTarget == null || secondController.currEnemyTarget == null) && isMindActive)
                {
                    isMindActive = false;
                    MelonLogger.Msg("------------------------");
                    MelonLogger.Msg("StopMindDeathWave");
                    _TrueGear.StopMindDeathWave();
                    return;
                }
            }
            else if (!(__instance.CheckHandsDistanace(firstController, secondController)) && isMindActive)
            {
                isMindActive = false;
                MelonLogger.Msg("------------------------");
                MelonLogger.Msg("StopMindDeathWave");
                _TrueGear.StopMindDeathWave();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ShootingSystem), "UpdateShootingFor")]
        public static void ShootingSystem_UpdateShootingFor_PrePatch(VrHandController handController)
        {
            pickupHandController = handController;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(VrHapticSystem), "SetVibration",new Type[] {typeof(Controller), typeof(string), typeof(float) })]
        public static void VrHapticSystem_SetVibration_PostPatch(VrHapticSystem __instance, Controller controller, string preset, float multiplier = 1f)
        {
            if (canPunch)
            {
                if (canMelee)
                {
                    canMelee = false;
                    if (preset == "Punch")
                    {
                        if (controller == Controller.LeftController)
                        {
                            MelonLogger.Msg("LeftHandMelee");
                            _TrueGear.Play("LeftHandMelee");
                        }
                        else
                        {
                            MelonLogger.Msg("RightHandMelee");
                            _TrueGear.Play("RightHandMelee");
                        }
                    }
                    Timer meleeTimer = new Timer(MeleeTimerCallBack,null,50,Timeout.Infinite);
                }                
            }            
        }

        private static void MeleeTimerCallBack(System.Object o)
        { 
            canMelee = true;
        }











    }
}