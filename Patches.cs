using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using System.IO;


namespace GenieNS
{
    public class Patches
    {
        public static int _oldCoinCount = 0;
        public static int _oldWishCount = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WishingWell), nameof(WishingWell.UpdateCard))]
        public static void WishingWell__GiveActualWish_Prefix(WishingWell __instance)
        {
            if (_oldWishCount != __instance.WishCount)
            {
                _oldWishCount = __instance.WishCount;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WishingWell), nameof(WishingWell.UpdateCard))]
        public static void WishingWell__GiveActualWish_Postfix(WishingWell __instance)
        {
            if (_oldWishCount != __instance.WishCount)
			{
                if (GenieMod.instance.LampGranted(_oldWishCount + 1) && !(GenieMod.instance.ReceivedLamp()))
                {
                    CardData cardData = WorldManager.instance.CreateCard(__instance.transform.position, "genie_special_magical_lamp", faceUp: true, checkAddToStack: false);
                    WorldManager.instance.CreateSmoke(__instance.transform.position);
                }
            }
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.GetCardRequiredFoodCount))]
        public static void WorldManager__GetCardRequiredFoodCount_Postfix(ref GameCard c, ref int __result)
        {
            if (c.CardData is BaseVillager baseVillager)
            {
                // Debug.LogWarning(GenieMod.instance.Wish01Chosen.ToString());
                // Debug.LogWarning(GenieMod.instance.wishedAmount.ToString());
                // Debug.LogWarning(MagicalLamp.Wish01Chosen.ToString());
                // Debug.LogWarning(MagicalLamp.wishedAmount.ToString());

                // int wishHealthy = (int)Wishes.Healthy;

                // if (Wishes.Healthy in MagicalLamp.wishesChosenDict)
                
                if (GenieMod.instance.WishChosen(MagicalLamp.wishesChosenDict, Wishes.Healthy, true))
                {
                    __result -= 1;
                }

                // if (baseVillager.GetRequiredFoodCount() > 1 && MagicalLamp.Wish01Chosen)
                // {
                //     __result -= 1;
                // }
            }

        }



    }
}