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

        [ExtraData("intro_genie")] ////// 
        public static bool introDone = false;


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
            
            Harmony.PatchAll(typeof(Patches));

            SokLoc.instance.LoadTermsFromFile(System.IO.Path.Combine(this.Path, "localization.tsv"));
        }

        public override void Ready()
        {
            Logger.Log("Ready!");
        }

        
        public bool WishChosen(Dictionary<Wishes, bool> dictionary, Wishes expectedKey, bool expectedValue)
        {
            bool actualValue;
            if (!dictionary.TryGetValue(expectedKey, out actualValue))
            {
                return false;
            }
            return actualValue == expectedValue;
        }


    }

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


    public class MagicalLamp : CardData
    {
        [ExtraData("wishes_done")]
        public static int wishedAmount = 0;

        [ExtraData("last_wish_moon")]
        public static int moonCountWishMade = 0;

	    public bool inCutscene;

        
        // [ExtraData("wishes_chosen")] // this cannot serialize, because it's not an int or string
        public static Dictionary<Wishes, bool> wishesChosenDict = new Dictionary<Wishes, bool>();

        [ExtraData("wishes_chosen")]
	    [HideInInspector]
        public static string wishesChosenNames = ""; // saved as EG "Healthy,Wealthy,WorldPeace"

        private List<Wishes> wishesChosenList = new List<Wishes>();


        public override void OnDestroyCard()
        {
            LostAllWishes();
            if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
            {
                LostAllWishesPrompt("label_lamp_lost_all_wishes");
            }
        }
        
        public void LostAllWishes()
        {
            wishesChosenNames = string.Empty;
            wishedAmount = 0;
            // find Abu and Genie, destroy them
        }

        public void LostAllWishesPrompt(string term)
        {
            ModalScreen.instance.Clear();

            ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_lamp_destroyed"));
            
            ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
            {
                GameCanvas.instance.CloseModal();
            });
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

            if (!GenieMod.introDone)
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

            if (!GenieMod.introDone)
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

            yield return GenieMod.introDone ? Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro_done")) : Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_who_are_you"));

			Cutscenes.Title = "";
            var _name = gameCard.CardData.FullName;
            if (_name != null)
            {
                Cutscenes.Title = "Genie";
			    Cutscenes.Text = SokLoc.Translate("label_genie_cutscene_text", LocParam.Create("genie", gameCard.CardData.FullName));  // first time only
            }

            
            if (moonCountWishMade == WorldManager.instance.CurrentMonth)
            {
                // already made a wish this month
            }
            
            string[] AvailableWishList = GetAvailableWishes();
			// yield return Cutscenes.WaitForAnswer(SokLoc.Translate("label_genie_exit"), SokLoc.Translate("label_genie_wish_healthy"), SokLoc.Translate("label_genie_wish_wealthy"), SokLoc.Translate("label_genie_wish_world_peace"), SokLoc.Translate("label_genie_wish_no_aggression"), SokLoc.Translate("label_genie_wish_powerful"), SokLoc.Translate("label_genie_wish_immortality"), SokLoc.Translate("label_genie_wish_freedom"));
            yield return Cutscenes.WaitForAnswer(AvailableWishList);
            
            Cutscenes.Title = "Genie";
            //Cutscenes.Title = "";
            Cutscenes.Text = "";

            GenieMod.introDone = true;

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

                        // if you already have the wish; break

                        Cutscenes.Text = SokLoc.Translate("label_genie_wish_granted");

                        wishesChosenNames += string.Join(",", wish.ToString());
                        wishesChosenDict.Add(wish, true);
                        wishedAmount++;

                        Debug.LogWarning(wishesChosenNames);
                        Debug.LogWarning(wishedAmount.ToString());
                        break;
                    }
                }

                moonCountWishMade = WorldManager.instance.CurrentMonth;
            
                // disable Wish choosing for 1 moon

                    // int wishHealthy = (int)Wishes.Healthy;

                    // if (wishHealthy == WorldManager.instance.ContinueButtonIndex)
                    // {
                    //     wishesChosenDict.Add(wish, true);
                    //     // break;
                    // }
            }

            yield return new WaitForSeconds(0.8f);
            WorldManager.instance.CreateSmoke(gameCard.transform.position);
            gameCard.DestroyCard();



            // works, but too much code; make it easier
            // else if (WorldManager.instance.ContinueButtonIndex == 1) // second option
            // {
            //     // GenieMod.instance.Wish01Chosen = true;
            //     Wish01Chosen = true;
            //     // GenieMod.instance.wishedAmount++;
            //     wishedAmount++;
            // }

			StopIntro();
		    lamp.inCutscene = false;

        }

        public static string[] GetAvailableWishes()
        {
            string[] AvailableWishes = [];

            AvailableWishes = [SokLoc.Translate("label_genie_exit"), SokLoc.Translate("label_genie_wish_healthy"), SokLoc.Translate("label_genie_wish_wealthy"), SokLoc.Translate("label_genie_wish_world_peace"), SokLoc.Translate("label_genie_wish_no_aggression"), SokLoc.Translate("label_genie_wish_powerful"), SokLoc.Translate("label_genie_wish_immortality"), SokLoc.Translate("label_genie_wish_best_friend")];
            
            if (wishedAmount == 2)
            {
                AvailableWishes = AvailableWishes.Append(SokLoc.Translate("label_genie_wish_freedom")).ToArray();
            }
            
            return AvailableWishes;
        }

        // what about a list of available wishes?
        private bool IsWishAvailable()
        {
            return true;
        }
        
        public static IEnumerator GenieRecurring(MagicalLamp lamp)
        {
            yield return new WaitForSeconds(0.5f);
        }

        private static void StopIntro(bool keepCameraPosition = false)
        {
            Cutscenes.Text = "";
            Cutscenes.Title = "";
            GameCamera.instance.TargetPositionOverride = null;
            GameCamera.instance.CameraPositionDistanceOverride = null;
            GameCamera.instance.TargetCardOverride = null;
            // CutsceneScreen.instance.IsAdvisorCutscene = false;
            // CutsceneScreen.instance.IsEndOfMonthCutscene = false;
            // CutsceneScreen.instance.CheckAdvisorCutscene();
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