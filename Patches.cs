using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
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
                if (GenieMod.instance.WishChosen(Wishes.Healthy))
                {
                    __result -= 1;
                }
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardData), nameof(CardData.GetValue))]
        public static void CardData__GetValue_Postfix(ref int __result)
        {
            if (GenieMod.instance.WishChosen(Wishes.Wealthy))
            {
                if (__result != -1)
                {
                    __result += 1;
                }

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mob), nameof(Mob.UpdateCard))]
        public static void Mob__UpdateCard_Postfix(Mob __instance)
        {
            if (__instance.IsAggressive && GenieMod.instance.WishChosen(Wishes.NoAggression))
            {
                __instance.IsAggressive = false;
            }
        }

        // this worked - just not entirely
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CreateCard), new Type[]
		// {	
		// 	typeof(Vector3),
		// 	typeof(CardData),
		// 	typeof(bool),
		// 	typeof(bool),
		// 	typeof(bool),
		// 	typeof(bool)
		// })]
        // public static void WorldManager__CreateCard_Postfix(ref CardData __result)
        // {
        //     if (__result.MyCardType is CardType.Humans && __result is Combatable combatable && __result is BaseVillager villager)
        //     {
        //         if (GenieMod.instance.WishChosen(Wishes.Powerful))
        //         {
        //             Debug.LogWarning("CreateCard made powerful: " + __result.Id);
        //             villager = GenieMod.instance.MadePowerful(villager);

        //             // combatable.BaseCombatStats.AttackDamage += 2;
        //             // combatable.BaseCombatStats.AttackSpeed -= (float)1.2;
        //         }

        //         if (GenieMod.instance.WishChosen(Wishes.Immortality))
        //         {
        //             Debug.LogWarning("CreateCard made immortal: " + __result.Id);
        //             villager = GenieMod.instance.MadeImmortal(villager);

        //             // SpecialHit new_special_hit = new SpecialHit();
        //             // new_special_hit.Chance = 5;
        //             // new_special_hit.HitType = SpecialHitType.LifeSteal;
        //             // new_special_hit.Target = SpecialHitTarget.Target;

        //             // combatable.BaseCombatStats.MaxHealth += 10;
        //             // combatable.BaseCombatStats.SpecialHits.Add(new_special_hit);
        //         }
        //     }
        // }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Combatable), nameof(Combatable.RealBaseCombatStats), MethodType.Getter)]
        public static void Combatable__RealBaseCombatStats_Postfix(ref Combatable __instance, ref CombatStats __result)
        {
            if (__instance.MyCardType is CardType.Humans && __instance is BaseVillager villager)
            {
                GenieMod.instance.AddToWishRecipientList(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatStats), nameof(CombatStats.InitStats))]
        public static void CombatStats__InitStats_Postfix(ref CombatStats __instance, ref CombatStats stats)
        {
            List<Combatable> list = GenieMod.instance.GetWishRecipientList();

            SpecialHit new_special_hit = new SpecialHit(); // what if it already has Lifesteal?
            new_special_hit.Chance = 5;
            new_special_hit.HitType = SpecialHitType.LifeSteal;
            new_special_hit.Target = SpecialHitTarget.Target;
            
            foreach (Combatable combatable in list)
            {
                if (GenieMod.instance.WishChosen(Wishes.Powerful))
                {
                    __instance.AttackDamage = stats.AttackDamage + 2;
                    __instance.AttackSpeed = stats.AttackSpeed - (float)1.2;
                }
                
                if (GenieMod.instance.WishChosen(Wishes.Immortality))
                {
                    __instance.MaxHealth = stats.MaxHealth + 10;
                    __instance.SpecialHits.Add(new_special_hit);
                }
                combatable.OnHealthChange(); // this refreshes the text
            }
            GenieMod.instance.ClearWishRecipientList();


            // List<Combatable> list = GenieMod.instance.GetWishRecipientList();

            // if (GenieMod.instance.WishChosen(Wishes.Powerful))
            // {
            //     foreach (Combatable combatable in list)
            //     {
            //         __instance.AttackDamage = stats.AttackDamage + 2;
            //         __instance.AttackSpeed = stats.AttackSpeed - (float)1.2;
            //     }
            // }

            // if (GenieMod.instance.WishChosen(Wishes.Immortality))
            // {
            //     SpecialHit new_special_hit = new SpecialHit(); // what if it already has Lifesteal?
            //     new_special_hit.Chance = 5;
            //     new_special_hit.HitType = SpecialHitType.LifeSteal;
            //     new_special_hit.Target = SpecialHitTarget.Target;

            //     foreach (Combatable combatable in list)
            //     {
            //         __instance.MaxHealth = stats.MaxHealth + 10;
            //         __instance.SpecialHits.Add(new_special_hit);
            //     }
            // }

            // GenieMod.instance.ClearWishRecipientList(); // to keep it refreshed
        }


        // this worked - just not entirely
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.ChangeToCard))]
        // public static void WorldManager__ChangeToCard_Postfix(ref CardData __result) // , ref GameCard card, ref string cardId
        // {
        //     if (__result.MyCardType is CardType.Humans && __result is Combatable combatable && __result is BaseVillager villager) // __result == card.CardData
        //     {
        //         if (GenieMod.instance.WishChosen(Wishes.Powerful))
        //         {
        //             Debug.LogWarning("ChangeToCard made powerful: " + __result.Id);
        //             villager = GenieMod.instance.MadePowerful(villager);


        //             // if (villager.GetOverrideEquipable().VillagerTypeOverride != null)
        //             // {

        //             // }

        //             // combatable.BaseCombatStats.AttackDamage += 2;
        //             // combatable.BaseCombatStats.AttackSpeed -= (float)1.2;
        //         }

        //         if (GenieMod.instance.WishChosen(Wishes.Immortality))
        //         {
        //             Debug.LogWarning("ChangeToCard made immortal: " + __result.Id); // same as combatable.Id

        //             villager = GenieMod.instance.MadeImmortal(villager);
        //             // villager = MadeImmortal(villager);

        //             // SpecialHit new_special_hit = new SpecialHit();
        //             // new_special_hit.Chance = 5;
        //             // new_special_hit.HitType = SpecialHitType.LifeSteal;
        //             // new_special_hit.Target = SpecialHitTarget.Target;

        //             // combatable.BaseCombatStats.MaxHealth += 10;
        //             // combatable.BaseCombatStats.SpecialHits.Add(new_special_hit);
        //         }

        //     }
        // }

    }
}