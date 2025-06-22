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
            int min = _wishCount - 1;
            int max = 20;
            int rand = UnityEngine.Random.Range(min, max);
            
            // Debug.LogWarning($"Random Number between {min.ToString()} and {max.ToString()}: " + rand.ToString());

            if (rand >= 19 || _wishCount == 5) // 50% chance to get it before the 5th wish
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
            WorldManager.instance.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "genie_special_villager_genie", 0.5f));

            Logger.Log("Ready!");
        }

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


        public List<Combatable> WishRecipientList = new List<Combatable>();

        public void AddToWishRecipientList(Combatable combatable)
        {
            if (!WishRecipientList.Contains(combatable))
            {
                WishRecipientList.Add(combatable);
            }
        }

        public List<Combatable> GetWishRecipientList()
        {
            return WishRecipientList;
        }

        public void ClearWishRecipientList()
        {
            WishRecipientList.Clear();
        }

        public void ResetEquipment(CardData cardData)
        {
		    List<Equipable> allEquipables = cardData.GetAllEquipables();
            foreach (Equipable item in allEquipables)
            {
                if (item != null && !string.IsNullOrEmpty(item.Id))
                {
                    cardData.MyGameCard.Unequip(item);
                    item.MyGameCard.SendIt();
                }
            }            
        }

    }

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


        public override void OnDestroyCard()
        {
            LoseAllWishes();
        }
        
        public void LoseAllWishes()
        {
            introDone = false;
            wishedAmount = 0;
            moonCountWishMade = 0;
            wishesChosenNames = string.Empty;
            GenieMod.instance.ClearWishRecipientList();

		    CardData cardData = WorldManager.instance.GetCard("genie_special_villager_genie");
            if (cardData != null)
            {
                GenieMod.instance.ResetEquipment(cardData);
		        cardData.MyGameCard.DestroyCard();
            }

            if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
            {
                LostAllWishesPrompt("label_lamp_lost_all_wishes");
            }
        }

        private void LostAllWishesPrompt(string term)
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
            if (wishedAmount >= 3)
            {
                AllWishesDonePrompt("label_lamp_done_all_wishes");
                return;
            }

		    if (!inCutscene)
            {
				TryStartGenieCutscene(GenieIntro(this));
            }

            // base.Clicked(); // so it doesn't hop
        }

        private void AllWishesDonePrompt(string term)
        {
            ModalScreen.instance.Clear();

            if (GenieMod.instance.WishChosen(Wishes.Freedom))
            {
                ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_lamp_max_wishes_freed_genie"));
            }
            else
            {
                ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_lamp_max_wishes"));
            }

            ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
            {
                GameCanvas.instance.CloseModal();
            });
            GameCanvas.instance.OpenModal();
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
                Cutscenes.Title = SokLoc.Translate("label_lamp_cleaning_title");
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_cleaning1"));
                yield return new WaitForSeconds(1.0f);
                
                Cutscenes.Title = SokLoc.Translate("label_lamp_cleaning_title");
                GameCamera.instance.Screenshake = 0.2f;
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_cleaning2"));
                yield return new WaitForSeconds(1.0f);

                GameCamera.instance.Screenshake = 0.5f;

                Cutscenes.Text = "";
                Cutscenes.Title = SokLoc.Translate("label_lamp_cleaning_title");
                yield return new WaitForSeconds(0.5f);
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_cleaning3"));
                yield return new WaitForSeconds(1.0f);
            }

            Cutscenes.Title = SokLoc.Translate("label_lamp_cutscene_title");
            GameCamera.instance.TargetCardOverride = lamp;
            GameCamera.instance.Screenshake = 0.8f;
            yield return new WaitForSeconds(2f);

            yield return DramaticDots();
            
            if (!introDone)
            {
                yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_lamp_intro"));
            }

            yield return new WaitForSeconds(0.5f);
            // GameCamera.instance.Screenshake = 0.3f;
            
			GameCard gameCard = null;

            if (moonCountWishMade != WorldManager.instance.CurrentMonth) // if you haven't already made a wish this month
            {
                GameCamera.instance.Screenshake = 0.3f;

                gameCard = Cutscenes.FindOrCreateGameCard("genie_special_genie", lamp.transform.position);

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
            }

            Cutscenes.Title = "Genie";

            if (!introDone)
            {
			    Cutscenes.Text = SokLoc.Translate("label_genie_introduction1");

                yield return new WaitForSeconds(0.5f);
                yield return DramaticDots();
				yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_genie_introduction2"));

                Cutscenes.Title = "Genie";
                Cutscenes.Text = SokLoc.Translate("label_genie_introduction3");

                yield return new WaitForSeconds(2.0f);
                yield return DramaticDots();
				yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_genie_introduction4"));

                Cutscenes.Title = "Genie";
                Cutscenes.Text = SokLoc.Translate("label_genie_introduction5");
				yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));

			    Cutscenes.Text = SokLoc.Translate("label_genie_first_wish");
            }
            else
            {
                Cutscenes.Text = SokLoc.Translate("label_genie_recurring", LocParam.Create("count", GetWishCountDescription()));
            }

            if (moonCountWishMade == WorldManager.instance.CurrentMonth) // already made a wish this month
            {
                // Cutscenes.Title = "Genie";
                Cutscenes.Text = SokLoc.Translate("label_genie_already_wished");
                yield return new WaitForSeconds(0.8f);
            }
            else
            {
                string[] AvailableWishList = GetAvailableWishes();
                yield return Cutscenes.WaitForAnswer(AvailableWishList);

                if (WorldManager.instance.ContinueButtonIndex == 0) // first option; always available - no wish being made
                {
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

                            if (wishedList.Contains(wish.ToString())) // already wished this before
                            {
                                Cutscenes.Text = SokLoc.Translate("label_genie_wish_already_chosen");
                                break;
                            }

                            if (wish == Wishes.Healthy)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_healthy");
                            }

                            if (wish == Wishes.Wealthy)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_wealthy");
                            }

                            if (wish == Wishes.WorldPeace)
                            {
                                if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
                                {
                                    Cutscenes.Text = SokLoc.Translate("label_genie_already_peaceful");
                                    break;
                                }
                                else
                                {
                                    WorldManager.instance.CurrentRunOptions.IsPeacefulMode = true;
                                    Cutscenes.Text = SokLoc.Translate("label_result_wishes_world_peace");
                                }
                            }

                            if (wish == Wishes.NoAggression)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_no_aggression");
                            }

                            if (wish == Wishes.Powerful)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_powerful");
                            }

                            if (wish == Wishes.Immortality)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_immortality");
                            }

                            if (wish == Wishes.Freedom)
                            {
                                Cutscenes.Text = SokLoc.Translate("label_genie_set_free");
                                yield return new WaitForSeconds(2.0f);
                                WorldManager.instance.ChangeToCard(gameCard, "genie_special_villager_genie");
                                
                                Cutscenes.Text = SokLoc.Translate("label_result_wishes_freedom");
                            }

                            Cutscenes.Title = SokLoc.Translate("label_genie_wish_granted");

				            yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));

                            wishedList.Add(wish.ToString());
                            wishesChosenNames = String.Join(",", wishedList);

                            wishedAmount++;
                            moonCountWishMade = WorldManager.instance.CurrentMonth;

                            Debug.LogWarning(wishesChosenNames);
                            Debug.LogWarning(wishedAmount.ToString());
                            break;
                        }
                    }
                }
            }
            
            yield return new WaitForSeconds(1.2f);

            if (gameCard != null && gameCard.CardData.Id != "genie_special_villager_genie")
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
                
                if (wishedAmount > 0)
                {
                    string new_description;
                    List<string> wishedList = new List<string>();
                    wishedList = wishesChosenNames.Split(',').ToList();
                    new_description = String.Join("<BR>", wishedList);
                    string extra_text = SokLoc.Translate("label_lamp_wishes_done_description");
                    new_description = $"{descriptionOverride}{extra_text}{new_description}";
                    descriptionOverride = new_description;
                }
            }
        }

    }

}