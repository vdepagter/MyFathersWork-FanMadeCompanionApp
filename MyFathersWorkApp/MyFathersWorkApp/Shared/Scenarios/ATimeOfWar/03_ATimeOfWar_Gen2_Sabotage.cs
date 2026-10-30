namespace MyFathersWorkApp;

// Generation II "Martial" end-of-round events (port map 2.2): sabotage, messengers and duels. They run between
// MartialEndOfRound_0 and the end-of-round popup (MartialRoundEnd), with Years still at the round that ends.
public static partial class ATimeOfWar
{
    // Originally $dueltype
    private const int _DUEL_SCIENCE = 1;
    private const int _DUEL_PISTOLS = 2;

    #region Sabotage

    // Early Years: the player with the fewest VP decides
    private static void ATOWSabotageIntro1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddOnceYouAreReady(ATOWSabotageIntro1Revealed);
    }

    private static void ATOWSabotageIntro1Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Loser(globalData)));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Village"), ATOWSabotage1);
        if (globalData.PlayersNum > 2) globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Cousin"), SabotageSignIn1);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Abstain"), ATOWSabotageNO);
    }

    // Middle Years, Separatists, nothing destroyed yet: the player with the {figure} token decides (nobody may hold it,
    // as in the source - port map 7, B9)
    private static void ATOWSabotageIntro2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(ATOWSabotageIntro2Revealed);
    }

    private static void ATOWSabotageIntro2Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Loser(globalData)));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Village"), ATOWSabotage1);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Player"),  SabotageSignIn1);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Abstain"), ATOWSabotageNO);
    }

    // Middle Years, Separatists, a building fell in the Early Years: the Outpost is the target
    private static void ATOWSabotageIntro2Sep(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(ATOWSabotageIntro2SepRevealed);
    }

    private static void ATOWSabotageIntro2SepRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Loser(globalData)));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Outpost"), data =>
        {
            ATimeOfWarVars vars = Vars(data);
            vars.OutpostDestroyed = true;
            vars.ChroniclePage    = vars.SabotagedBuilding == MartialBuilding.Barracks ? 8 : 7;
            ATOWSabotageMonConfirm(data);
        });
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Player"),  SabotageSignIn1);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Sabotage_Abstain"), ATOWSabotageNO);
    }

    private static void ATOWSabotage1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).SabotagedBuilding = MartialBuilding.Barracks;
                Vars(data).ChroniclePage     = 5;
                ATOWSabotageConfirm(data);
            },
            data =>
            {
                Vars(data).SabotagedBuilding = MartialBuilding.LaborersUnion;
                Vars(data).ChroniclePage     = 6;
                ATOWSabotageConfirm(data);
            }
        ], true);
    }

    private static void ATOWSabotageConfirm(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.BuildingsDestroyed = 1;
        if (globalData.Years != Years.Early) vars.ChroniclePageMiddle = true;

        string building = globalData.GetScenarioLocalizedTag("Building_Name").FormatWithIndex(0, (int)vars.SabotagedBuilding);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, Loser(globalData))
                                                             .FormatWithReplacement(1, building)
                                                             .FormatWithReplacement(2, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(SabotageNext);
    }

    private static void ATOWSabotageMonConfirm(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.BuildingsDestroyed  = 2;
        vars.ChroniclePageMiddle = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Loser(globalData)).FormatWithReplacement(1, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(ResistEvent);
    }

    private static void ATOWSabotageNO(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ATOWSabotageNO_0);
    }

    private static void ATOWSabotageNO_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.CompulsionBack, PopUpButton.Confirm, SabotageNext);
    }

    // After the sabotage decision: Early Years the messenger, Middle Years (Separatists) the quartering
    private static void SabotageNext(GlobalData globalData)
    {
        if (globalData.Years == Years.Early) MessengerCheck(globalData);
        else ResistEvent(globalData);
    }

    private static void SabotageSignIn1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).Saboteur = player;
            SabotagePlayerChoice1(globalData);
        });
    }

    private static void SabotagePlayerChoice1(GlobalData globalData)
    {
        string saboteur = Vars(globalData).Saboteur;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SabotageTarget = player;
            SabotagePlayerConfirmed(globalData);
        }, showPlayer: player => player != saboteur);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ATOWSabotageNO], true);
    }

    private static void SabotagePlayerConfirmed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).SabotageTarget));
        globalData.ActiveWindow.AddClickHereToContinue(SabotageNext);
    }

    // Originally Sabotage1Now: no player was sabotaged this round -> End of Round
    private static void SabotageCheck(GlobalData globalData)
    {
        if (Vars(globalData).SabotageTarget == string.Empty) MartialRoundEnd(globalData);
        else Sabotage1Now(globalData);
    }

    private static void Sabotage1Now(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("Sabotage1Now#" + (int)globalData.Years, 2)));
        globalData.ActiveWindow.AddClickHereToContinue(Sabotage1Now_0);
    }

    private static void Sabotage1Now_0(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        bool           discard = vars.RandomBool(_RND_SABOTAGE_DISCARD + (int)globalData.Years);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, Player1BeenSabotaged,
            content => content.FormatWithReplacement(0, vars.SabotageTarget).FormatWithCondition(1, () => discard));
    }

    #endregion Sabotage

    #region Messengers

    // Originally AMessenger (Early Years): only if a player met the guard in Generation I (originally $rumor1)
    private static void MessengerCheck(GlobalData globalData)
    {
        if (Vars(globalData).MetGuard) AMessenger(globalData);
        else MessengerEnd(globalData);
    }

    private static void MessengerEnd(GlobalData globalData)
    {
        if (Vars(globalData).WarWinner == WarFaction.Monarchists) BarracksPenaltyCheck(globalData);
        else SabotageCheck(globalData);
    }

    // $takereact branches are dead in the source (port map 7, B2): the Gen I winner's Flag is the "yes" answer
    private static void AMessenger(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Crest(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Amessyes, Amessno], true, content => content
                                                                                                .FormatWithReplacement(0, Winner(globalData))
                                                                                                .FormatWithReplacement(1, Loser(globalData)));
    }

    private static void Amessno(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Amessno_0);
    }

    private static void Amessno_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MessengerEnd,
            content => content.FormatWithReplacement(0, Crest(globalData)));
    }

    private static void Amessyes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Crest(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(AmessyesRevealed);
    }

    private static void AmessyesRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(Amessyes2);
    }

    private static void Amessyes2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).SuspiciousBarn = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Amessyes2_0);
    }

    private static void Amessyes2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, MessengerEnd,
            content => content.FormatWithReplacement(0, Crest(globalData)));
    }

    // Middle Years, Unified Monarchists, a player met the gentleman in Generation I (originally $rumor2)
    private static void AMessenger2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(AMessenger2Revealed);
    }

    private static void AMessenger2Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [AMessYes2b, AMessno2], true);
    }

    private static void AMessYes2b(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(AMessYes2bRevealed);
    }

    // The assessment letter (originally $iou = 1): ResistSides then always ends with ResistEnd
    private static void AMessYes2bRevealed(GlobalData globalData)
    {
        int determine = WarDetermine(globalData);

        globalData.SaveToUndo();
        Vars(globalData).Assessment = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, determine.ToString()).FormatWithReplacement(1, Loser(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(AMessYes2b_0);
    }

    private static void AMessYes2b_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, AfternoonTea,
            content => content.FormatWithReplacement(0, Figure(globalData)));
    }

    private static void AMessno2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AMessno2_0);
    }

    private static void AMessno2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ExperimentABack, PopUpButton.Confirm, AfternoonTea,
            content => content.FormatWithReplacement(0, Figure(globalData)));
    }

    #endregion Messengers

    #region Duels

    // The sabotaged player answers in secret
    private static void Player1BeenSabotaged(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(Vars(globalData).SabotageTarget, Player1BeenSabotagedRevealed);
    }

    private static void Player1BeenSabotagedRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Saboteur));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [DuelRules1, DuelDenounce1], true);
    }

    private static void DuelRules1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).DuelType = _DUEL_SCIENCE;
                AcceptDuel1(data);
            },
            data =>
            {
                Vars(data).DuelType = _DUEL_PISTOLS;
                AcceptDuel1(data);
            },
            DuelDenounce1
        ], true);
    }

    // Read aloud: the hand-over line comes before the title, as in the source
    private static void DuelDenounce1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Saboteur));
        globalData.ActiveWindow.AddClickHereToContinue(DuelDenounce1_0);
    }

    private static void DuelDenounce1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithReplacement(0, Vars(globalData).Saboteur));
    }

    // The saboteur answers the challenge in secret
    private static void AcceptDuel1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(Vars(globalData).Saboteur, AcceptDuel1Revealed);
    }

    private static void AcceptDuel1Revealed(GlobalData globalData)
    {
        int duelType = Vars(globalData).DuelType;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, duelType).FormatWithCondition(1, () => duelType == _DUEL_SCIENCE));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [AcceptDuel1b, DuelDecline1], true);
    }

    private static void DuelDecline1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(DuelDecline1_0);
    }

    private static void DuelDecline1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithReplacement(0, Vars(globalData).Saboteur));
    }

    private static void AcceptDuel1b(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("AcceptDuel1b#" + (int)globalData.Years, 2)));
        globalData.ActiveWindow.AddClickHereToContinue(DuelResolution1);
    }

    // Read aloud. The source draws the field / the shot here; they are drawn on the next screens (mirror mode).
    private static void DuelResolution1(GlobalData globalData)
    {
        int duelType = Vars(globalData).DuelType;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => duelType == _DUEL_SCIENCE));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("DuelResolution1_Begin"), DuelResolution1a);
    }

    // Field of the scientific duel (originally $duelsci): 0 Engineering, 1 Chemistry, 2 Biology, 3 Occult
    private static int DuelField(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([0, 1, 2, 3], _RND_DUEL_FIELD + (int)globalData.Years);
    }

    private static string DuelFieldName(GlobalData globalData)
    {
        return globalData.GetScenarioLocalizedTag("DuelField_Name").FormatWithIndex(0, DuelField(globalData));
    }

    private static void DuelResolution1a(GlobalData globalData)
    {
        bool science = Vars(globalData).DuelType == _DUEL_SCIENCE;
        int  field   = science ? DuelField(globalData) : 4;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithIndex(0, field));

        if (science)
        {
            globalData.ActiveWindow.AddNextContent(1, false, content => content.FormatWithReplacement(0, DuelFieldName(globalData)));
            globalData.ActiveWindow.AddClickHereToContinue(DuelResolution1a_0);
        }
        else
        {
            globalData.ActiveWindow.AddNextContent(2);
            globalData.ActiveWindow.AddClickHereToContinue(DuelResolution1b);
        }
    }

    private static void DuelResolution1a_0(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AdvanceJournalTrack, PopUpButton.Confirm, DuelResolution1sci,
            content => content
                      .FormatWithReplacement(0, vars.SabotageTarget)
                      .FormatWithReplacement(1, vars.Saboteur)
                      .FormatWithReplacement(2, DuelFieldName(globalData)));
    }

    private static void DuelResolution1sci(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, DuelFieldName(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).DuelLoser = Vars(data).Saboteur;
                DuelResolution1b(data);
            },
            data =>
            {
                Vars(data).DuelLoser = Vars(data).SabotageTarget;
                DuelResolution1b(data);
            },
            ScienceDuelTiedRes
        ], true, content => content.FormatWithReplacement(0, vars.SabotageTarget).FormatWithReplacement(1, vars.Saboteur));
    }

    private static void ScienceDuelTiedRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, DuelFieldName(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(ScienceDuelTiedRes_0);
    }

    private static void ScienceDuelTiedRes_0(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithReplacement(0, vars.SabotageTarget).FormatWithReplacement(1, vars.Saboteur));
    }

    // Pistols: the shot (originally $duelshot 1-5) - 1-2 the saboteur loses, 3-5 the sabotaged player loses. Death on a
    // roll of 1 of 5 (originally $duelresult == "1", a number compared with a string - port map 7, B10).
    private static void DuelResolution1b(GlobalData globalData)
    {
        ATimeOfWarVars vars  = Vars(globalData);
        int            round = (int)globalData.Years;

        string loser = vars.DuelType == _DUEL_PISTOLS
            ? vars.RandomInRange(1, 5, _RND_DUEL_SHOT + round) < 3 ? vars.Saboteur : vars.SabotageTarget
            : vars.DuelLoser;
        bool death  = vars.RandomInRange(1, 5, _RND_DUEL_DEATH + round) == 1;
        int  detail = vars.DuelType == _DUEL_SCIENCE ? DuelField(globalData) : 4;

        globalData.SaveToUndo();
        vars.DuelLoser = loser;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => death));
        globalData.ActiveWindow.AddNextContent(death ? 1 : 2, false, content => content.FormatWithReplacement(0, loser).FormatWithIndex(1, detail));
        globalData.ActiveWindow.AddClickHereToContinue(death ? DuelResolution1b_0 : DuelResolution1b_1);
    }

    // Death
    private static void DuelResolution1b_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Bodies_Icon, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithReplacement(0, Vars(globalData).DuelLoser).FormatWithCondition(1, () => globalData.Years == Years.Early));
    }

    // Wounded pride
    private static void DuelResolution1b_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithReplacement(0, Vars(globalData).DuelLoser));
    }

    #endregion Duels
}
