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
                // int wishHealthy = (int)Wishes.Healthy;

                // if (Wishes.Healthy in MagicalLamp.wishesChosenDict)
                
                // if (GenieMod.instance.WishChosen(MagicalLamp.wishesChosenDict, Wishes.Healthy, true))
                if (GenieMod.instance.WishChosen(Wishes.Healthy))
                {
                    __result -= 1;
                }
            }
        }
        
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardValue), nameof(CardValue.CardValue))]
        // public static void CardValue__CardValue_Postfix(ref int baseValue)
        // {
        //     BaseValue = baseValue + 1;
        // }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardData), nameof(CardData.GetValue))]
        public static void CardData__GetValue_Postfix(ref int __result)
        {
            if (GenieMod.instance.WishChosen(Wishes.Wealthy))
            {
                if (__result != 0)
                {
                    __result += 1;
                }

            }
        }


        // resets on loading a savegame?
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mob), nameof(Mob.UpdateCard))]
        public static void Mob__UpdateCard_Postfix(Mob __instance)
        {
            if (GenieMod.instance.WishChosen(Wishes.NoAggression))
            {
                if (__instance.IsAggressive)
                {
                    __instance.IsAggressive = false;
                }
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




        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.ChangeToCard))]
        // public static void WorldManager__ChangeToCard_Prefix(ref GameCard card)
        // {
        //     if (card.CardData.MyCardType is CardType.Humans && card.CardData is Combatable combatable && card.CardData is BaseVillager villager)
        //     {
        //         Debug.LogWarning("Prefix is: " + card.CardData.Id);

        //         if (GenieMod.instance.WishChosen(Wishes.Powerful))
        //         {
        //             Debug.LogWarning("Prefix: Wishes.Powerful is true");
        //             card.CardData.BaseCombatStats.AttackDamage += 2;
        //             card.CardData.BaseCombatStats.AttackSpeed -= (float)1.2;
        //         }

        //         if (GenieMod.instance.WishChosen(Wishes.Immortality))
        //         {
        //             Debug.LogWarning("Prefix: Wishes.Immortality is true");
        //             SpecialHit new_special_hit = new SpecialHit();
        //             new_special_hit.Chance = 5;
        //             new_special_hit.HitType = SpecialHitType.LifeSteal;
        //             new_special_hit.Target = SpecialHitTarget.Target;

        //             card.CardData.BaseCombatStats.MaxHealth += 10;
        //             card.CardData.BaseCombatStats.SpecialHits.Add(new_special_hit);
        //         }
        //     }
        // }



        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(BaseVillager), nameof(BaseVillager.OnEquipItem))]
        // public static void BaseVillager__OnEquipItem_Postfix(ref Equipable equipable, ref BaseVillager __instance)
        // {
		//     if (__instance.CanOverrideCardFromEquipment && !string.IsNullOrEmpty(equipable.VillagerTypeOverride) && equipable.VillagerTypeOverride != __instance.Id)
        //     {
        //         Debug.LogWarning("OnEquipItem made powerful: " + __instance.Id); // villager -> NOT SWORDSMAN?!
        //         __instance = MadeImmortal(__instance);
        //     }
        // }


        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Combatable), nameof(Combatable.RealBaseCombatStats), MethodType.CombatStats)] // , nameof(Combatable.RealBaseCombatStats)
        
        // [HarmonyTargetMethod]
        // [HarmonyPatch(typeof(Combatable), nameof(Combatable.RealBaseCombatStats))]
        


        // this runs continously.....
        [HarmonyPostfix] // should be prefix? // cannot change the __result in a Getter
        [HarmonyPatch(typeof(Combatable), nameof(Combatable.RealBaseCombatStats), MethodType.Getter)] // , nameof(Combatable.RealBaseCombatStats)
        public static void Combatable__RealBaseCombatStats_Postfix(ref Combatable __instance, ref CombatStats __result)
        {
            // CombatStats old_result = __result;
            // CombatStats new_result = __result;
            // CombatStats old_result = __instance.BaseCombatStats;
            // CombatStats new_result = __instance.BaseCombatStats;

            if (__instance.MyCardType is CardType.Humans && __instance is BaseVillager villager)
            {
                GenieMod.instance.AddToCombatableList(__instance);


                // add the instance to a list kept in MOD. Then Init will read if instance is there, then add the stats to it




                // Debug.LogWarning("RealBaseCombatStats Id : " + __instance.Id);

                // Debug.LogWarning("RealBaseCombatStats Att dmg : " + __instance.BaseCombatStats.AttackDamage.ToString());
                // Debug.LogWarning("RealBaseCombatStats Att dmg : " + __result.AttackDamage.ToString());

                // if (__instance.BaseCombatStats != __result) //

                // __result.AttackDamage = old_result.AttackDamage + 2;


                // __instance.BaseCombatStats.AttackDamage = __result.AttackDamage + 2;


                
                // Debug.LogWarning("old_result Att dmg : " + old_result.AttackDamage.ToString());
                // Debug.LogWarning("new_result Att dmg : " + __instance.BaseCombatStats.AttackDamage.ToString());


                // if (new_result.AttackDamage != old_result.AttackDamage)
                // {
                // Debug.LogWarning("old_result Att dmg : " + old_result.AttackDamage.ToString());
                // Debug.LogWarning("cur_result Att dmg : " + __result.AttackDamage.ToString());

                    //__result = new_result;
                // }

                
                // Debug.LogWarning("RealBaseCombatStats Max HP : " + villager.BaseCombatStats.MaxHealth.ToString());
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatStats), nameof(CombatStats.InitStats))]
        public static void CombatStats__InitStats_Postfix(ref CombatStats __instance, ref CombatStats stats)
        {
            var list = GenieMod.instance.GetCombatableList();


            if (GenieMod.instance.WishChosen(Wishes.Powerful))
            {
                foreach (Combatable combatable in list)
                {
                    __instance.AttackDamage = stats.AttackDamage + 2;
                    __instance.AttackSpeed = stats.AttackSpeed - (float)1.2;
                }
            }

            if (GenieMod.instance.WishChosen(Wishes.Immortality))
            {
                SpecialHit new_special_hit = new SpecialHit(); // what if it already has Lifesteal?
                new_special_hit.Chance = 5;
                new_special_hit.HitType = SpecialHitType.LifeSteal;
                new_special_hit.Target = SpecialHitTarget.Target;

                foreach (Combatable combatable in list)
                {
                    __instance.MaxHealth = stats.MaxHealth + 10;
                    __instance.SpecialHits.Add(new_special_hit);
                }
            }

            GenieMod.instance.ClearCombatableList(); // to keep it refreshed




            // var parent = __instance.GetType().BaseType; // gets gameobject
            // Combatable combat_parent = parent.GetComponent<Combatable>();
            
            // Combatable parent_combatable = __instance.gameObject.GetComponentInParent<Combatable>();

            // Debug.LogWarning("CombatStats.InitStats Id : " + parent.ToString());
            // Debug.LogWarning("CombatStats.InitStats Id : " + combat_parent);

            //Debug.LogWarning("CombatStats.InitStats Id : " + __instance.ToString());
            // Debug.LogWarning("AD before : " + __instance.AttackDamage.ToString());
            // __instance.AttackDamage += 2;
            // Debug.LogWarning("AD after : " + __instance.AttackDamage.ToString());
        }




        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(BaseVillager), nameof(BaseVillager.OnEquipItem))]
        // public static void BaseVillager__OnEquipItem_Postfix(ref Equipable equipable, ref BaseVillager __instance)
        // {
		//     if (__instance.CanOverrideCardFromEquipment && !string.IsNullOrEmpty(equipable.VillagerTypeOverride) && equipable.VillagerTypeOverride != __instance.Id)
        //     {
        //         Debug.LogWarning("OnEquipItem : " + __instance.Id); // villager -> NOT VillagerTypeOverride (swordsman/ninja etc)
        //         Debug.LogWarning("OnEquipItem VillagerTypeOverride : " + equipable.VillagerTypeOverride);
        //         __instance = GenieMod.instance.MadePowerful(__instance);
        //         __instance = GenieMod.instance.MadeImmortal(__instance);
        //     }
        // }




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







        // public static Combatable MadeImmortal(Combatable combatable)
        // {
        //     Combatable _new_combatable = combatable;

        //     SpecialHit new_special_hit = new SpecialHit(); // what if it already has Lifesteal?
        //     new_special_hit.Chance = 5;
        //     new_special_hit.HitType = SpecialHitType.LifeSteal;
        //     new_special_hit.Target = SpecialHitTarget.Target;

        //     _new_combatable.BaseCombatStats.MaxHealth += 10;
        //     _new_combatable.BaseCombatStats.SpecialHits.Add(new_special_hit);

        //     return _new_combatable;
        // }

        // public static BaseVillager MadeImmortal(BaseVillager combatable)
        // {
        //     BaseVillager _new_combatable = combatable;

        //     SpecialHit new_special_hit = new SpecialHit();
        //     new_special_hit.Chance = 5;
        //     new_special_hit.HitType = SpecialHitType.LifeSteal;
        //     new_special_hit.Target = SpecialHitTarget.Target;

        //     _new_combatable.BaseCombatStats.MaxHealth += 10;
        //     _new_combatable.BaseCombatStats.SpecialHits.Add(new_special_hit);

        //     return _new_combatable;
        // }

        
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.GetCardPrefab))]
        // public static void WorldManager__GetCardPrefab_Postfix(ref CardData __result)
        // {
        //     if (__result.MyCardType is CardType.Humans && __result is Combatable villager)
        //     {
        //         if (GenieMod.instance.WishChosen(Wishes.Powerful))
        //         {        
        //             SpecialHit new_special_hit = new SpecialHit();
        //             new_special_hit.Chance = 5;
        //             new_special_hit.HitType = SpecialHitType.LifeSteal;
        //             new_special_hit.Target = SpecialHitTarget.Target;

        //             villager.BaseCombatStats.AttackDamage += 2;
        //             villager.BaseCombatStats.AttackSpeed -= (float)1.2;
        //             villager.BaseCombatStats.SpecialHits.Add(new_special_hit);
        //         }
        //     }
        // }



        // instead of here.. on card creation. ANd when obtaining the Wish. Should work on-load as well then.
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(BaseVillager), nameof(Combatable.UpdateCard))]
        // public static void BaseVillager__UpdateCard_Postfix(ref BaseVillager __instance)
        // {
        //     // make sure to only have the stats updated once..

        //     //SpecialHit new_special_hit = new SpecialHit(SpecialHit.Chance = 0.05f, SpecialHit.HitType = SpecialHitType.LifeSteal, SpecialHit.Target = SpecialHitTarget.Self);
        //     if (GenieMod.instance.WishChosen(Wishes.Powerful))
        //     {
        //         SpecialHit new_special_hit = new SpecialHit();
        //         new_special_hit.Chance = 0.05f;
        //         new_special_hit.HitType = SpecialHitType.LifeSteal;
        //         new_special_hit.Target = SpecialHitTarget.Self;

        //         __instance.BaseCombatStats.AttackDamage += 2;
        //         __instance.BaseCombatStats.AttackSpeed -= (float)1.2;
        //         __instance.BaseCombatStats.SpecialHits.Add(new_special_hit);

        //     }

        //     // Debug.LogWarning(__instance.ToString());

        // }

    }
}