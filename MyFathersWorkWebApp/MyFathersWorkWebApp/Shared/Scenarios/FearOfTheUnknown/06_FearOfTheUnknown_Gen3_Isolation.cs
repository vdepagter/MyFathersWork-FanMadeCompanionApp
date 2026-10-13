namespace MyFathersWorkWebApp;

public static partial class FearOfTheUnknown
{
    public static void Isolation(GlobalData globalData)
    {
        // Round 10 -> Isolation
        // Round 11 -> Isolation2
        // Round 12 -> Isolation3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.Isolation;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       risingPricesSection = "RisingPrices";
        GameplayHubSection risingPrices        = globalData.ActiveHub.AddSection(risingPricesSection, true);
        risingPrices.ReplaceShouldShow(() => globalData.Years == Years.Early);
        risingPrices.AddDefaultContent(risingPricesSection);

        const string       verificationSection = "Verification";
        GameplayHubSection verification        = globalData.ActiveHub.AddSection(verificationSection, true);
        verification.ReplaceShouldShow(() => vars.Destroyed == 0 && globalData.GetActivePlayers().Any(player => !vars.Verified[player]));
        verification.AddDefaultContent(verificationSection);
        verification.AddSpecialClickHere(verificationSection, CitizenshipInspection);
        verification.AddNextContent(1, verificationSection, true);

        const string       brazenOfferSection = "BrazenOffer";
        GameplayHubSection brazenOffer        = globalData.ActiveHub.AddSection(brazenOfferSection, true);
        brazenOffer.ReplaceShouldShow(() => globalData.Years is Years.Middle or Years.Late);
        brazenOffer.AddDefaultContent(brazenOfferSection);
        brazenOffer.AddSpecialClickHere(brazenOfferSection, SmugglingConsequences);
        brazenOffer.AddNextContent(1, brazenOfferSection);

        const string       experimentSection = "Experiment";
        GameplayHubSection experiment        = globalData.ActiveHub.AddSection(experimentSection);
        experiment.ReplaceShouldShow(() => globalData.Years == Years.Middle);
        experiment.AddDefaultContent(experimentSection, content => content
                                                                  .FormatWithIndex(0, vars.Experiment)
                                                                  .FormatWithCondition(1, () => globalData.PlayersNum >= 3)
                                                                  .FormatWithCondition(2, () => globalData.PlayersNum == 2));

        const string       churchSection = "Church";
        GameplayHubSection church        = globalData.ActiveHub.AddSection(churchSection);
        church.AddSpecialClickHere(churchSection, Church);
        church.AddDefaultContent(churchSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Isolation_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Isolation_0);
    }

    private static void Isolation_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => CaravanCheck,
            Years.Middle => ControlledExperiment,
            Years.Late   => NeglectedByTheCaravan,
            _            => _ => { }
        });
    }

    #region Intro

    private static void IsolationIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Path = FearOfTheUnknownHubId.Isolation;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));

        // Source: IsoBricksRes rewards the wall builders only when Gen2fate == "witch", which is never set - the reward is never shown
        globalData.ActiveWindow.AddClickHereToContinue(MinorRecompense);
    }

    // Originally WitchIsolationResolution: the heir of an executed witch
    private static void MinorRecompense(GlobalData globalData)
    {
        string witch = Witch(globalData);

        if (Vars(globalData).Cursed != 1 || witch == FearOfTheUnknownVars.SUSPECT)
        {
            ExcellenceDistrusted(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, witch));
        globalData.ActiveWindow.AddClickHereToContinue(MinorRecompense_0);
    }

    private static void MinorRecompense_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, ExcellenceDistrusted,
            content => content.FormatWithReplacement(0, Witch(globalData)));
    }

    // Originally Creepy1stPlayer
    private static void ExcellenceDistrusted(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(ExcellenceDistrusted_0);
    }

    private static void ExcellenceDistrusted_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, ObsessionsResolve);
    }

    private static void IsolationSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, Isolation, content => content
                                                                                                                                                         .FormatWithReplacement(0, Vars(globalData).Tracker.ToString())
                                                                                                                                                         .FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    // The Town Hall was destroyed by the Collaborative Experiment: the Village Chronicle shows the ruins
    private static void IsolationLateSetup(GlobalData globalData)
    {
        if (Vars(globalData).Destroyed == 0)
        {
            Isolation(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Isolation,
            content => content.FormatWithReplacement(0, Vars(globalData).Destroyed.ToString()));
    }

    #endregion Intro

    #region Verification of Identity

    // Originally IsoIdentificationSignIn
    private static void CitizenshipInspection(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 2, "CitizenshipInspection" + globalData.Years)(content)
                                                             .FormatWithReplacement(0, RandomDate(globalData, "CitizenshipInspection" + globalData.Years)));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SelectedPlayer = player;
            VerificationCompleted(globalData);
        }, showPlayer: player => !Vars(globalData).Verified[player]);
    }

    // Originally IsoIdentification
    private static void VerificationCompleted(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content =>
        {
            content = Either(globalData, 2, 2, "VerificationCompleted" + player)(content);
            content = Either(globalData, 3, 2, "VerificationCompleted" + player)(content);
            return content.FormatWithReplacement(0, player).FormatWithReplacement(1, RandomDate(globalData, "VerificationCompleted" + player));
        });
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).Verified[player] = true;
            Isolation(data);
        });
    }

    // Originally IsolationCaravanResolve
    private static void NeglectedByTheCaravan(GlobalData globalData)
    {
        if (Vars(globalData).CaravanVisits > 0)
        {
            VerificationRequired(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(NeglectedByTheCaravan_0);
    }

    private static void NeglectedByTheCaravan_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_MarketToken, PopUpButton.Confirm, VerificationRequired);
    }

    // Originally IsoVerify: no penalty when the Town Hall was destroyed
    private static void VerificationRequired(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (vars.Destroyed != 0)
        {
            ObsessionResolution(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();

        string[] unverified = globalData.GetActivePlayers().Where(player => !vars.Verified[player]).ToArray();

        foreach (string player in unverified)
        {
            globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        }

        if (unverified.Length == 0) globalData.ActiveWindow.AddNextContent(1);
        globalData.ActiveWindow.AddClickHereToContinue(ObsessionResolution);
    }

    #endregion Verification of Identity

    #region Smuggling

    // Originally S4Smug
    private static void SmugglersOffer(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(SmugglersOffer_0);
    }

    private static void SmugglersOffer_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, ControlledRelease);
    }

    // Originally SmugEvent: the player with the least VP chooses the Collaborative Experiment
    private static void ControlledRelease(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddNextContent(1);
        globalData.ActiveWindow.AddNextContentWithLinks(2, [data => ChooseExperiment(0, data), data => ChooseExperiment(1, data), data => ChooseExperiment(2, data)], true);
    }

    private static void ChooseExperiment(int experiment, GlobalData globalData)
    {
        Vars(globalData).Experiment = experiment;
        Preparations(globalData);
    }

    private static int Collaborators(GlobalData globalData) => globalData.PlayersNum switch
    {
        2 => 3,
        3 => 2,
        _ => Vars(globalData).RandomElement([2, 3], _RND_COLLABORATION)
    };

    // Originally SmugEventA
    private static void Preparations(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        int collaborators = Collaborators(globalData);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, collaborators.ToString())
                                                             .FormatWithIndex(1, vars.Experiment)
                                                             .FormatWithCondition(2, () => globalData.PlayersNum >= 3)
                                                             .FormatWithCondition(3, () => globalData.PlayersNum == 2));
        globalData.ActiveWindow.AddClickHereToContinue(Preparations_0);
    }

    private static void Preparations_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_CollaborativeToken, PopUpButton.Confirm, Isolation,
            content => content.FormatWithIndex(0, Vars(globalData).Experiment).FormatWithReplacement(1, Collaborators(globalData).ToString()));
    }

    // Originally SmugConsequence: every visit of the Smuggler's Dock risks a random consequence
    private static void SmugglingConsequences(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 3, "SmugglingConsequences" + vars.SmugglingCount)(content)
                                                             .FormatWithReplacement(0, RandomDate(globalData, "SmugglingConsequences" + vars.SmugglingCount)));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            vars.SelectedPlayer =  player;
            vars.SmugglingCount += 1;
            vars.RandomElement([CaughtInTheAct, TheTrial, PoliticsAsUsual], _RND_SMUGGLING + Math.Min(vars.SmugglingCount - 1, 9)).Invoke(globalData);
        });
    }

    private static int SmugglingSlot(GlobalData globalData) => Math.Min(Vars(globalData).SmugglingCount - 1, 9);

    // Originally S4Con1
    private static void CaughtInTheAct(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, RandomDate(globalData, "CaughtInTheAct" + SmugglingSlot(globalData))));
        globalData.ActiveWindow.AddClickHereToContinue(CaughtInTheAct_0);
    }

    private static void CaughtInTheAct_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Isolation);
    }

    // Originally S4Con2
    private static void TheTrial(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        int cost = Vars(globalData).RandomElement([1, 2], _RND_TRIAL_COST + SmugglingSlot(globalData));

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "TheTrial" + SmugglingSlot(globalData)))
                                                             .FormatWithReplacement(2, cost.ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data =>
        {
            Vars(data).SmuggleTrial = 0;
            TrialOutcome(data);
        }, data =>
        {
            Vars(data).SmuggleTrial = 1;
            TrialOutcome(data);
        }], true);
    }

    // Originally S4Con2A: paying guarantees success, otherwise the trial succeeds 1 time in 3
    private static void TrialOutcome(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        int outcome = vars.SmuggleTrial == 0 ? 0 : vars.RandomElement([0, 1, 2], _RND_TRIAL + SmugglingSlot(globalData));

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithCondition(0, () => outcome == 0));
        globalData.ActiveWindow.AddNextContent(outcome + 1, false, content => content.FormatWithReplacement(0, vars.SelectedPlayer));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).SmuggleTrial = outcome;
            TrialOutcome_0(data);
        });
    }

    private static void TrialOutcome_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars    = Vars(globalData);
        int                  outcome = vars.SmuggleTrial;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, outcome switch
        {
            0 => PopUpIcon.MFWlogo,
            1 => PopUpIcon.Creepy_Icon,
            _ => PopUpIcon.ScoreTrackMarker
        }, PopUpButton.Confirm, Isolation, string.Empty);

        globalData.ActivePopup.ReplaceDescription(Tag(globalData, "TrialOutcome_0_Content" + outcome)
                                                     .FormatWithReplacement(0, outcome == 2 ? vars.RandomElement([1, 2], _RND_TRIAL_VP + SmugglingSlot(globalData)).ToString() : "0"));
    }

    // Originally S4Con3
    private static void PoliticsAsUsual(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, RandomDate(globalData, "PoliticsAsUsual" + SmugglingSlot(globalData))));
        globalData.ActiveWindow.AddClickHereToContinue(PoliticsAsUsual_0);
    }

    private static void PoliticsAsUsual_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Isolation,
            content => content.FormatWithCondition(0, () => Vars(globalData).RandomBool(_RND_POLITICS + SmugglingSlot(globalData))));
    }

    #endregion Smuggling

    #region End of the Middle Years - The Collaborative Experiment

    // Originally SmugEvent2
    private static void ControlledExperiment(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Destroyed = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(1, Collaborators(globalData).ToString())
                                                             .FormatWithCondition(0, () => globalData.PlayersNum >= 3));
        globalData.ActiveWindow.AddYesNo(TownHallDestroyed, MissedOpportunity);
    }

    // Originally SmugEvent2A
    private static void TownHallDestroyed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Ending = 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(TownHallDestroyed_0);
    }

    private static void TownHallDestroyed_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_CollaborativeToken, PopUpButton.Confirm, DevastatingResults, string.Empty);

        string   content  = globalData.PlayersNum == 2 ? Tag(globalData, "TownHallDestroyed_0_Content") : string.Empty;
        string[] verified = globalData.GetActivePlayers().Where(player => vars.Verified[player]).ToArray();

        // Source gave the bonus to the first player when nobody had visited the Town Hall - no one gets it here
        if (verified.Length > 0)
        {
            int creepy = vars.RandomElement([2, 3], _RND_TOWN_HALL_CREEPY);
            content += Tag(globalData, "TownHallDestroyed_0_Content1");

            foreach (string player in verified)
            {
                content += Tag(globalData, "TownHallDestroyed_0_Content2").FormatWithReplacement(0, player).FormatWithReplacement(1, creepy.ToString());
            }
        }

        globalData.ActivePopup.ReplaceDescription(content == string.Empty ? Tag(globalData, "TownHallDestroyed_0_Content3") : content.TrimTrailingBreaks());
    }

    // Originally SmugEvent2ACont
    private static void DevastatingResults(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Destroyed = 16;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddNextContent(1 + Vars(globalData).Experiment, true);
        globalData.ActiveWindow.AddNextContent(4, true);
        globalData.ActiveWindow.AddNextContentWithLinks(5, [data => Vars(data).RandomElement([PathOfDestruction, PathOfDestruction, RampageConcludes], _RND_RAMPAGE_1).Invoke(data)], true);
    }

    // Originally SmugEvent2Aa: the Bank
    private static void PathOfDestruction(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Destroyed = 17;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(DevastatingResults))));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => Vars(data).RandomElement([TheLibrary, RampageConcludes], _RND_RAMPAGE_2).Invoke(data)], true);
    }

    // Originally SmugEvent2aa: the Library
    private static void TheLibrary(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Destroyed = 18;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(DevastatingResults))));
        globalData.ActiveWindow.AddClickHereToContinue(TheLibrary_0);
    }

    private static void TheLibrary_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, CaravanCheck);
    }

    // Originally SmugEventEnd
    private static void RampageConcludes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(DevastatingResults))));
        globalData.ActiveWindow.AddNextContent(1 + Vars(globalData).Experiment);
        globalData.ActiveWindow.AddNextContent(4, true);
        globalData.ActiveWindow.AddClickHereToContinue(RampageConcludes_0);
    }

    private static void RampageConcludes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, CaravanCheck,
            content => content.FormatWithCondition(0, () => Vars(globalData).RandomBool(_RND_RAMPAGE_END)));
    }

    // Originally SmugEvent2B
    private static void MissedOpportunity(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Ending = 2;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MissedOpportunity_0);
    }

    private static void MissedOpportunity_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_CollaborativeToken, PopUpButton.Confirm, CaravanCheck,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 2).FormatWithCondition(1, () => globalData.PlayersNum == 2));
    }

    #endregion End of the Middle Years - The Collaborative Experiment
}
