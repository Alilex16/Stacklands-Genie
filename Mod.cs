using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using System.IO;


namespace GenieNS
{
    public class GenieMod : Mod
    {
        public static GenieMod instance;

        public bool LampGranted(int _wishCount)
        {
            int rand = UnityEngine.Random.Range(_wishCount - 1, 20);
            if (rand == 19 || _wishCount == 5) // 50% chance to get it before the 5th wish
            {
                return true;
            }
            return false;
        }

        public bool ReceivedLamp()
        {
            foreach (GameCard item in WorldManager.instance.AllCards)
            {
                if (item.CardData.Id == "genie_special_magical_lamp")
                {
                    return true;
                }
            }
            return false;
        }

        public void Awake()
        {
            instance = this;

            // MagicalLamp.instance.LoseAllWishes();
            
            Harmony.PatchAll(typeof(Patches));

            SokLoc.instance.LoadTermsFromFile(System.IO.Path.Combine(this.Path, "localization.tsv"));
        }

        public override void Ready()
        {
            WorldManager.instance.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "genie_special_villager_genie", 0.5f));

            Logger.Log("Ready!");
        }

        // public bool WishChosen(Dictionary<Wishes, bool> dictionary, Wishes expectedKey, bool expectedValue) // not needed anymore
        // {
        //     bool actualValue;
        //     if (!dictionary.TryGetValue(expectedKey, out actualValue))
        //     {
        //         return false;
        //     }
        //     return actualValue == expectedValue;
        // }

        public bool WishChosen(Wishes expectedKey)
        {
            if (string.IsNullOrEmpty(MagicalLamp.wishesChosenNames))
            {
                return false;
            }

            List<string> wishedList = MagicalLamp.wishesChosenNames.Split(',').ToList();

            foreach (string wish in wishedList)
            {
                if (wish == expectedKey.ToString())
                {
                    return true;
                }
            }

            return false;
        }



        public List<Combatable> TheList = new List<Combatable>();

        public void AddToCombatableList(Combatable combatable)
        {
            if (!TheList.Contains(combatable))
            {
                TheList.Add(combatable);
                // Debug.LogWarning("Added combatable Id to list : " + combatable.Id);
            }
        }

        public List<Combatable> GetCombatableList()
        {
            return TheList;
        }

        public void ClearCombatableList()
        {
            TheList.Clear();
        }


        // public CombatStats MadePowerful(CombatStats combatStats)
        // {
        //     CombatStats _new_combat_stats = combatStats;
            
        //     _new_combat_stats.AttackDamage = combatStats.AttackDamage + 2;
        //     _new_combat_stats.AttackSpeed -= (float)1.2;
            
        //     return _new_combat_stats;
        // }








        // public Combatable MadePowerful(Combatable combatable)
        // {
        //     Combatable _new_combatable = combatable;

        //     combatable.BaseCombatStats.AttackDamage += 2;
        //     combatable.BaseCombatStats.AttackSpeed -= (float)1.2;

        //     return _new_combatable;
        // }

        // public Combatable MadeImmortal(Combatable combatable)
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


        // public BaseVillager MadePowerful(BaseVillager combatable)
        // {
        //     BaseVillager _new_combatable = combatable;

        //     combatable.BaseCombatStats.AttackDamage += 2;
        //     combatable.BaseCombatStats.AttackSpeed -= (float)1.2;

        //     return _new_combatable;
        // }

        // public BaseVillager MadeImmortal(BaseVillager combatable)
        // {
        //     BaseVillager _new_combatable = combatable;

        //     SpecialHit new_special_hit = new SpecialHit(); // what if it already has Lifesteal?
        //     new_special_hit.Chance = 5;
        //     new_special_hit.HitType = SpecialHitType.LifeSteal;
        //     new_special_hit.Target = SpecialHitTarget.Target;

        //     _new_combatable.BaseCombatStats.MaxHealth += 10;
        //     _new_combatable.BaseCombatStats.SpecialHits.Add(new_special_hit);

        //     return _new_combatable;
        // }

        // public int GetWishesDone()
        // {
        //     return MagicalLamp.wishedAmount;
        // }


    }

    // public static class StringUtil
    // {
    //     public static string JoinFilter(string separator, IEnumerable<string> strings)
    //     {
    //         return string.Join(separator, strings.Where(s => !string.IsNullOrEmpty(s)));
    //     }
    //     public static string JoinFilter(string separator, params string[] str)
    //     {
    //         return string.Join(separator, str?.Where(s => !string.IsNullOrEmpty(s)));
    //     }
    // }

    // public class WishesDoneTest
    // {
    //     public WishesDoneTest()
    //     {
    //         MagicalLamp.wishesChosenDict = new Dictionary<Wishes, bool>();
    //     }
    // }

    public class Genie : CardData
    {
        protected override bool CanHaveCard(CardData otherCard)
        {
            return false;
        }
        public override void UpdateCard()
        {
            base.UpdateCard();
            GenieMovement();
        }

        private void GenieMovement()
        {
            MyGameCard.TargetPosition += Vector3.left * 0.001f * Mathf.Cos(Time.time);
            MyGameCard.TargetPosition += Vector3.forward * 0.0005f * Mathf.Cos(Time.time * 0.5f);
        }
    }

    public class GenieFree : Villager
    {
        public override int GetRequiredFoodCount()
        {
            return 0;
        }
    }
    
    public class MagicalLamp : CardData
    {
        [ExtraData("intro_genie")]
        public static bool introDone = false;

        [ExtraData("wishes_done")]
        public static int wishedAmount = 0;

        [ExtraData("last_wish_moon")]
        public static int moonCountWishMade = 0;

        [ExtraData("wishes_chosen")]
	    [HideInInspector]
        public static string wishesChosenNames = ""; // saved as EG "Healthy,Wealthy,WorldPeace"

	    public bool inCutscene;

        private List<Wishes> wishesChosenList = new List<Wishes>(); // need this??


        // public void awake()
        // {
        //     LoseAllWishes();
        // }

        public override void OnDestroyCard()
        {
            LoseAllWishes(); // test this more

            if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
            {
                LostAllWishesPrompt("label_lamp_lost_all_wishes");
            }
            // Destroy(this);
        }
        
        public void LoseAllWishes()
        {
            wishedAmount = 0;
            moonCountWishMade = 0;
            wishesChosenNames = string.Empty;

            foreach (GameCard item in WorldManager.instance.AllCards)
            {
                if (item.CardData.Id == "genie_special_genie" || item.CardData.Id == "genie_special_villager_genie")
                {
                    item.DestroyCard();
                }
            }
        }

        public void LostAllWishesPrompt(string term)
        {
            ModalScreen.instance.Clear();

            ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_lamp_destroyed"));
            
            ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
            {
                // LostAllWishes(); // have to press Okey twice when it's here..
                GameCanvas.instance.CloseModal();
            });

            // LostAllWishes(); 

            GameCanvas.instance.OpenModal();
        }


        protected override bool CanHaveCard(CardData otherCard)
        {
            return false;
        }

        public override void Clicked()
        {
            if (wishedAmount >= 3) // can rub until you did all your 3 wishes
            {
                // add pop-up that you've already made your 3 wishes and the Genie refuses to come out ; if freed Genie: The lamp no longer has a prisoner; rubbing it has no effect
                return;
            }

		    if (!inCutscene)
            {
				TryStartGenieCutscene(GenieIntro(this));
            }

            base.Clicked();
        }

        private void TryStartGenieCutscene(IEnumerator cutscene)
        {
            inCutscene = true;
			WorldManager.instance.QueueCutscene(cutscene);
        }

        public static IEnumerator DramaticDots()
        {
            Cutscenes.Title = ".";
            yield return new WaitForSeconds(0.2f);
            Cutscenes.Title = "..";
            yield return new WaitForSeconds(0.2f);
            Cutscenes.Title = "...";
            yield return new WaitForSeconds(0.2f);
        }

        
        public static IEnumerator GenieIntro(MagicalLamp lamp)
        {
            GameCanvas.instance.SetScreen<CutsceneScreen>();
            Cutscenes.Text = "";

            if (!introDone)
            {
                Cutscenes.Title = SokLoc.Translate("label_magical_lamp_rubbing");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_rubbing_01"));
                yield return new WaitForSeconds(1.5f);
                DramaticDots();
                Cutscenes.Title = SokLoc.Translate("label_magical_lamp_rubbing");
                GameCamera.instance.Screenshake = 0.5f;
                // yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_rubbing_02"));
                Cutscenes.Text = SokLoc.Translate("label_rubbing_02");
                // DramaticDots();
                // yield return new WaitForSeconds(1.0f);
                Cutscenes.Text = "";
                Cutscenes.Title = SokLoc.Translate("label_magical_lamp_rubbing");
                yield return new WaitForSeconds(0.5f);
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_rubbing_03"));
                yield return new WaitForSeconds(1.5f);
            }

            // after intro has been done, the there should be an option to not rub the lamp (in case of accidental clicks)

            Cutscenes.Title = SokLoc.Translate("label_lamp_cutscene_title");
            GameCamera.instance.TargetCardOverride = lamp;
            GameCamera.instance.Screenshake = 0.8f;
            yield return new WaitForSeconds(2f);

            yield return DramaticDots();
            
            // working example
            // yield return GenieMod.introDone ? Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro_done")) : Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro"));

            if (!introDone)
            {
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro"));
            }

            yield return new WaitForSeconds(0.5f);
            GameCamera.instance.Screenshake = 0.3f;
            
			GameCard gameCard = Cutscenes.FindOrCreateGameCard("genie_special_genie", lamp.transform.position);

            WorldManager.instance.CreateSmoke(lamp.transform.position);

			if (gameCard != null)
			{
				gameCard.SendIt();
            }

			GameCamera.instance.TargetCardOverride = gameCard;

            yield return new WaitForSeconds(0.2f);

            WorldManager.instance.CreateSmoke(gameCard.transform.position);

            yield return DramaticDots();

            yield return introDone ? Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro_done")) : Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_who_are_you"));

			// Cutscenes.Title = "";
            Cutscenes.Title = "Genie";

            if (!introDone)
            {
                // Cutscenes.Title = "Genie";
			    Cutscenes.Text = SokLoc.Translate("label_genie_introduction");
            }
            else
            {
                Cutscenes.Text = SokLoc.Translate("label_genie_recurring", LocParam.Create("count", GetWishCountDescription()));
            }


            // add some more flavor here during the intro?
            // -- really, I can ask for anything? Haha you can try! There's a few rules. What about an infinite amount of wishes?

            if (moonCountWishMade == WorldManager.instance.CurrentMonth) // already made a wish this month
            {
                // Cutscenes.Title = "Genie";
                Cutscenes.Text = SokLoc.Translate("label_genie_already_wished");
                yield return new WaitForSeconds(0.8f);
            }
            else
            {
                string[] AvailableWishList = GetAvailableWishes();
                // yield return Cutscenes.WaitForAnswer(SokLoc.Translate("label_genie_exit"), SokLoc.Translate("label_genie_wish_healthy"), SokLoc.Translate("label_genie_wish_wealthy"), SokLoc.Translate("label_genie_wish_world_peace"), SokLoc.Translate("label_genie_wish_no_aggression"), SokLoc.Translate("label_genie_wish_powerful"), SokLoc.Translate("label_genie_wish_immortality"), SokLoc.Translate("label_genie_wish_freedom"));
                yield return Cutscenes.WaitForAnswer(AvailableWishList);

                if (WorldManager.instance.ContinueButtonIndex == 0) // first option; always available - no wish being made
                {
                    //Cutscenes.Title = "Genie";
                    Cutscenes.Text = SokLoc.Translate("label_genie_take_time");
                }
                else
                {
                    foreach(Wishes wish in Enum.GetValues(typeof(Wishes)))
                    {
                        if ((int)wish == WorldManager.instance.ContinueButtonIndex - 1)
                        {
                            List<string> wishedList = new List<string>();

                            if (wishedAmount != 0)
                            {
                                wishedList = wishesChosenNames.Split(',').ToList();
                            }

                            if (wishedList.Contains(wish.ToString())) // already wished this
                            {
                                Cutscenes.Text = SokLoc.Translate("label_genie_wish_already_chosen");
                                break;
                            }

                            if (wish == Wishes.Freedom) // setting the Genie free : different Text
                            {
                                Cutscenes.Text = SokLoc.Translate("label_genie_set_free");
                                yield return new WaitForSeconds(1.8f);
                                WorldManager.instance.ChangeToCard(gameCard, "genie_special_villager_genie");
                            }

                            // if wish is powerful/immortal -> cycle through all current cards and update

                            Cutscenes.Text = SokLoc.Translate("label_genie_wish_granted");
                            // add notification about what actually changed!

                            wishedList.Add(wish.ToString());
                            wishesChosenNames = String.Join(",", wishedList);
                            //wishesChosenNames = StringUtil.JoinFilter(",", wishedList);  /// 

                            // wishesChosenDict.Add(wish, true); // remove
                            wishedAmount++;
                            moonCountWishMade = WorldManager.instance.CurrentMonth;

                            Debug.LogWarning(wishesChosenNames);
                            Debug.LogWarning(wishedAmount.ToString());
                            break;
                        }
                    }
                   
                    // int wishHealthy = (int)Wishes.Healthy;

                    // if (wishHealthy == WorldManager.instance.ContinueButtonIndex)
                    // {
                    //     wishesChosenDict.Add(wish, true);
                    //     // break;
                    // }
                }
            }
            
            //Cutscenes.Title = "Genie";
            //Cutscenes.Title = "";
            //Cutscenes.Text = "";

            yield return new WaitForSeconds(1.2f);

            if (gameCard != null)
            {
                WorldManager.instance.CreateSmoke(gameCard.transform.position);
                gameCard.DestroyCard();
            }

            introDone = true;
			StopIntro();
		    lamp.inCutscene = false;

        }

        public static string GetWishCountDescription()
        {
            string wish_count_description = "";
            if (wishedAmount == 0)
            {
                wish_count_description = "first";
            }
            else if (wishedAmount == 1)
            {
                wish_count_description = "second";
            }
            else if (wishedAmount == 2)
            {
                wish_count_description = "third and last";
            }

            return wish_count_description;
        }

        public static string[] GetAvailableWishes()
        {
            string[] AvailableWishes = [];

            AvailableWishes = [SokLoc.Translate("label_genie_exit"), SokLoc.Translate("label_genie_wish_healthy"), SokLoc.Translate("label_genie_wish_wealthy"), SokLoc.Translate("label_genie_wish_world_peace"), SokLoc.Translate("label_genie_wish_no_aggression"), SokLoc.Translate("label_genie_wish_powerful"), SokLoc.Translate("label_genie_wish_immortality")];
            
            if (wishedAmount == 2)
            {
                AvailableWishes = AvailableWishes.Append(SokLoc.Translate("label_genie_wish_freedom")).ToArray();
            }
            
            return AvailableWishes;
        }
        
        // public static IEnumerator GenieRecurring(MagicalLamp lamp)
        // {
        //     yield return new WaitForSeconds(0.5f);
        // }

        private static void StopIntro(bool keepCameraPosition = false)
        {
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCamera.instance.TargetCardOverride = null;
            if (keepCameraPosition)
            {
                GameCamera.instance.KeepCameraAtCurrentPos();
            }
            GameCanvas.instance.SetScreen<GameScreen>();
			// WorldManager.instance.currentAnimationRoutine = null;
            WorldManager.instance.currentAnimation = null;
        }

        public override void UpdateCard()
        {
            base.UpdateCard();
        }


        public override void UpdateCardText()
        {
            descriptionOverride = SokLoc.Translate("genie_special_magical_lamp_warning_description");

            if (WorldManager.instance.HoveredCard == MyGameCard)
            {
                descriptionOverride = SokLoc.Translate("genie_special_magical_lamp_description");
            }
        }





        // public void RubbingMagicalLampPrompt(string term)
        // {
        //     ModalScreen.instance.Clear();
        //     if (WorldManager.instance.CurrentBoard.Id == "main")
        //     {
        //         ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_rubbing_01"));
        //     }

        //     ModalScreen.instance.AddOption(SokLoc.Translate("label_rubbing_try_harder"), delegate
        //     {
        //         ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_rubbing_02"));
        //         GameCanvas.instance.CloseModal();
        //     });

        //     ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
        //     {
        //         GameCanvas.instance.CloseModal();
        //     });
        //     GameCanvas.instance.OpenModal();
        // }
    }



}