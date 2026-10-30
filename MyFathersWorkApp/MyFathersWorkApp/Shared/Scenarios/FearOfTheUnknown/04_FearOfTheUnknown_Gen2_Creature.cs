namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    private const int _CREATURE_NOT_CREATED = 1;
    private const int _CREATURE_CREATED     = 2;

    public static void Creature(GlobalData globalData)
    {
        // Round 7 -> Creature
        // Round 8 -> Creature2
        // Round 9 -> Creature3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.Creature;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       asylumSection = "Asylum";
        GameplayHubSection asylum        = globalData.ActiveHub.AddSection(asylumSection, true);
        asylum.ReplaceShouldShow(() => globalData.Years is Years.Middle or Years.Late);
        asylum.AddDefaultContent(asylumSection);
        asylum.AddSpecialClickHere(asylumSection, AsylumSignIn);

        const string       businessSection = "Business";
        GameplayHubSection business        = globalData.ActiveHub.AddSection(businessSection, true);
        business.ReplaceShouldShow(() => globalData.Years is Years.Middle or Years.Late);
        business.AddDefaultContent(businessSection);
        business.AddSpecialClickHere(businessSection, BusinessMeeting);

        const string       requestsSection = "Requests";
        GameplayHubSection requests        = globalData.ActiveHub.AddSection(requestsSection, true);
        requests.AddDefaultContent(requestsSection);
        requests.AddSpecialClickHere(requestsSection, Guilt);

        const string       churchSection = "Church";
        GameplayHubSection church        = globalData.ActiveHub.AddSection(churchSection);
        church.AddSpecialClickHere(churchSection, Church);
        church.AddDefaultContent(churchSection);

        const string       newAsylumSection = "NewAsylum";
        GameplayHubSection newAsylum        = globalData.ActiveHub.AddSection(newAsylumSection);
        newAsylum.ReplaceShouldShow(() => globalData.Years == Years.Early);
        newAsylum.AddDefaultContent(newAsylumSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Creature_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Creature_0);
    }

    private static void Creature_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => EsteemedInstitution,
            Years.Middle => AsylumInvestors,
            Years.Late   => GrandfathersCreation,
            _            => _ => { }
        });
    }

    #region Draws

    private static int AsylumUpgrade(GlobalData globalData) => Vars(globalData).RandomElement([0, 1, 2], _RND_ASYLUM_UPGRADE);

    private static string AsylumUpgradeName(GlobalData globalData) => Tag(globalData, "AsylumUpgrade").FormatWithIndex(0, AsylumUpgrade(globalData));

    private static int AsylumUpgradeCost(GlobalData globalData) => Vars(globalData).RandomElement([1, 2], _RND_ASYLUM_COST);

    #endregion Draws

    #region Intro

    private static void CreatureIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Path = FearOfTheUnknownHubId.Creature;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("CreatureIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_II);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(CreatureDemands);
    }

    private static void CreatureDemands(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("CreatureDemands");
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(CreatureIntro))));
        globalData.ActiveWindow.AddClickHereToContinue(CreatureMasterwork);
    }

    // Originally TheCreatureCard
    private static void CreatureMasterwork(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddYesNo(HideousMotivation, data =>
        {
            Vars(data).CreatureCard = false;
            Obsessions(data);
        });
    }

    private static void HideousMotivation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(HideousMotivation_0);
    }

    private static void HideousMotivation_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_MWUpdateCreature, PopUpButton.Confirm, data =>
        {
            Vars(data).CreatureCard = true;
            Obsessions(data);
        });
    }

    private static void CreatureSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        FearOfTheUnknownVars vars = Vars(globalData);
        vars.Tracker += globalData.PlayersNum == 4 ? 3 : 2;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, Creature, content => content
                                                                                                                                                       .FormatWithCondition(0, () => vars.Victim == _VICTIM_BUTCHER)
                                                                                                                                                       .FormatWithReplacement(1, vars.Tracker.ToString())
                                                                                                                                                       .FormatWithCondition(2, () => globalData.PlayersNum == 3));
    }

    #endregion Intro

    #region Hub Events

    // Originally FOTU-Church (also used by the Generation III hubs)
    private static void Church(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ReturnToHub);
    }

    private static void ReturnToHub(GlobalData globalData)
    {
        switch (Vars(globalData).HubId)
        {
            case FearOfTheUnknownHubId.WitchHunt:  WitchHunt(globalData); break;
            case FearOfTheUnknownHubId.Foreigners: Foreigners(globalData); break;
            case FearOfTheUnknownHubId.Creature:   Creature(globalData); break;
            case FearOfTheUnknownHubId.Isolation:  Isolation(globalData); break;
            case FearOfTheUnknownHubId.Tension:    Tension(globalData); break;
            case FearOfTheUnknownHubId.Privatized: Privatized(globalData); break;
            case FearOfTheUnknownHubId.Liberal:    Liberal(globalData); break;
        }
    }

    // A player completed the Puppy or the Bride
    private static void Guilt(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData, "Guilt" + vars.CreatureHelp));
        globalData.ActiveWindow.AddDefaultContent(Either(globalData, 0, 2, "Guilt" + vars.CreatureHelp));
        globalData.ActiveWindow.AddClickHereToContinue(Guilt_0);
    }

    private static void Guilt_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_CardBack, PopUpButton.Confirm, data =>
        {
            Vars(data).CreatureHelp += 1;
            Creature(data);
        });
    }

    private static void AsylumSignIn(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            FearOfTheUnknownVars vars = Vars(globalData);
            vars.SelectedPlayer       =  player;
            vars.AsylumVisits[player] += 1;

            // Source checked whether any player had visited twice; the message is meant for the visiting player
            if (vars.AsylumVisits[player] > 1) AsylumAgain(globalData);
            else AsylumMeet(globalData);
        });
    }

    // Originally AsylumMeet
    private static void AsylumMeet(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 2, "AsylumMeet" + player)(content)
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "AsylumMeet" + player)));
        globalData.ActiveWindow.AddClickHereToContinue(AsylumMeet_0);
    }

    private static void AsylumMeet_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainCaretakerFromLost, PopUpButton.Confirm, Creature);
    }

    // Originally AsylumComplete
    private static void AsylumAgain(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "AsylumAgain" + player + Vars(globalData).AsylumVisits[player])));
        globalData.ActiveWindow.AddClickHereToContinue(AsylumAgain_0);
    }

    private static void AsylumAgain_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, Creature,
            content => content.FormatWithReplacement(0, Vars(globalData).SelectedPlayer));
    }

    #endregion Hub Events

    #region The Count

    // Originally BusinessMeet
    private static void BusinessMeeting(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(Either(globalData, 0, 2, "BusinessMeeting" + vars.BusinessMeetings));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            vars.BusinessPlayer   =  player;
            vars.BusinessBargain  =  false;
            vars.BusinessUpgrade  =  false;
            vars.BusinessMeetings += 1;
            SlyInvestment(globalData);
        });
    }

    // Originally BusinessMeetb
    private static void SlyInvestment(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 2, "SlyInvestment" + vars.BusinessMeetings)(content)
                                                             .FormatWithReplacement(0, vars.BusinessPlayer)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "SlyInvestment" + vars.BusinessMeetings)));
        globalData.ActiveWindow.AddClickHereToContinue(BusinessOffer);
    }

    // Originally BusMeetA - E
    private static void BusinessOffer(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(Vars(globalData).BusinessPlayer, BusinessOfferRevealed);
    }

    private static void BusinessOfferRevealed(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        bool houseAvailable = vars.HouseOnTheHill == string.Empty;
        bool ballAvailable  = vars.BallAndChain < 2;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 5, 2, "BusinessOfferRevealed" + vars.BusinessMeetings + vars.BusinessBargain)(content)
                                                             .FormatWithReplacement(0, vars.BusinessPlayer)
                                                             .FormatWithCondition(1, () => vars.BusinessBargain)
                                                             .FormatWithCondition(2, () => ballAvailable)
                                                             .FormatWithCondition(3, () => houseAvailable));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => BusinessInvest(3, data), data => BusinessInvest(2, data), data => BusinessInvest(1, data), BusinessNotConvinced, BusinessDeclined], true,
            content => content
                      .FormatWithCondition(0, () => houseAvailable)
                      .FormatWithCondition(1, () => ballAvailable)
                      .FormatWithCondition(2, () => !vars.BusinessBargain)
                      .FormatWithCondition(3, () => vars.BusinessBargain));
    }

    private static void BusinessInvest(int money, GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = vars.BusinessPlayer;

        vars.Debt[player] += money;

        if (money == 3) vars.HouseOnTheHill = player;

        if (money == 2)
        {
            vars.BusinessUpgrade =  true;
            vars.BallAndChain    += 1;
        }

        BusinessThanks(globalData);
    }

    // Fifty percent chance to improve the deal or end it
    private static void BusinessNotConvinced(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        vars.RandomElement([BusinessBargain, BusinessDeclined], _RND_BARGAIN + Math.Min(vars.BusinessMeetings - 1, 9)).Invoke(globalData);
    }

    // Originally BusBargain
    private static void BusinessBargain(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.BusinessBargain = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 2, "BusinessBargain" + vars.BusinessMeetings)(content).FormatWithReplacement(0, vars.BusinessPlayer));
        globalData.ActiveWindow.AddClickHereToContinue(BusinessOffer);
    }

    // Originally BusNo
    private static void BusinessDeclined(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 2, "BusinessDeclined" + vars.BusinessMeetings)(content).FormatWithReplacement(0, vars.BusinessPlayer));
        globalData.ActiveWindow.AddClickHereToContinue(Creature);
    }

    // Originally BusThanks
    private static void BusinessThanks(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, vars.BusinessPlayer)
                                                             .FormatWithCondition(1, () => vars.Cure == ExtendedBool.False));

        if (vars.BusinessUpgrade || vars.BusinessBargain) globalData.ActiveWindow.AddClickHereToContinue(BusinessThanks_0);
        else globalData.ActiveWindow.AddClickHereToContinue(AfterBusiness);
    }

    private static void BusinessThanks_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        int bonusVp = vars.BusinessBargain ? vars.RandomElement([2, 3], _RND_BARGAIN_VP + Math.Min(vars.BusinessMeetings - 1, 9)) : 0;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, vars.BusinessUpgrade ? PopUpIcon.S2_EstateUpgradeBACK : PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, AfterBusiness,
            content => content
                      .FormatWithReplacement(2, bonusVp.ToString())
                      .FormatWithCondition(0, () => vars.BusinessUpgrade)
                      .FormatWithCondition(3, () => vars.BusinessUpgrade && vars.BusinessBargain)
                      .FormatWithCondition(1, () => vars.BusinessBargain));
    }

    // The Count reveals his curse to the first investor
    private static void AfterBusiness(GlobalData globalData)
    {
        if (Vars(globalData).Cure == ExtendedBool.None) RemoveACurse(globalData);
        else Creature(globalData);
    }

    // Originally CountCureQuestion
    private static void RemoveACurse(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(Vars(globalData).BusinessPlayer, RemoveACurseRevealed);
    }

    private static void RemoveACurseRevealed(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.Cure       = ExtendedBool.False;
        vars.CurePlayer = vars.BusinessPlayer;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, vars.BusinessPlayer));
        globalData.ActiveWindow.AddClickHereToContinue(RemoveACurse_0);
    }

    private static void RemoveACurse_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.StorybookToken, PopUpButton.Confirm, Creature);
    }

    #endregion The Count

    #region End of the Early Years

    // Originally AsylumOpps
    private static void EsteemedInstitution(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(EsteemedInstitution_0);
    }

    private static void EsteemedInstitution_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Servant, PopUpButton.Confirm, BusinessOpportunity);
    }

    // Originally AsylumOpp2
    private static void BusinessOpportunity(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(EsteemedInstitution))));
        globalData.ActiveWindow.AddClickHereToContinue(BusinessOpportunity_0);
    }

    private static void BusinessOpportunity_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_HouseOnTheHillTile, PopUpButton.Confirm, CreatureMiddleSetup);
    }

    private static void CreatureMiddleSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Creature,
            content => content.FormatWithCondition(0, () => Vars(globalData).Victim == _VICTIM_BUTCHER));
    }

    #endregion End of the Early Years

    #region End of the Middle Years

    // Originally S4Asylum
    private static void AsylumInvestors(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AsylumInvestors_0);
    }

    private static void AsylumInvestors_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, AsylumSupport,
            content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
    }

    // Originally S4Asylum2
    private static void AsylumSupport(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Jail = 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
        globalData.ActiveWindow.AddNextContent(1 + AsylumUpgrade(globalData), true);
        globalData.ActiveWindow.AddClickHereToContinue(AsylumSupport_0);
    }

    private static void AsylumSupport_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, AsylumResolution,
            content => content.FormatWithReplacement(0, AsylumUpgradeCost(globalData).ToString()));
    }

    // Originally S4Asylum3
    private static void AsylumResolution(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();

        for (int x = 0; x <= globalData.PlayersNum; ++x)
        {
            int buyers = x;
            globalData.ActiveWindow.AddCustomLink(Tag(globalData, "AsylumResolution_Buyers").FormatWithIndex(0, buyers), data =>
            {
                Vars(data).AsylumCount += AsylumUpgradeCost(data) * buyers;
                Creature(data);
            });
        }
    }

    #endregion End of the Middle Years

    #region End of Generation II - Creature

    // Originally S4Creature1
    private static void GrandfathersCreation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        string request = Tag(globalData, "CreatureRequest").FormatWithIndex(0, Vars(globalData).RandomElement([0, 1, 2], _RND_CREATURE_REQUEST));

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, request));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data =>
        {
            Vars(data).CreatureMade = _CREATURE_CREATED;
            CreatureAppeased(data);
        }, data =>
        {
            Vars(data).CreatureMade = _CREATURE_NOT_CREATED;
            TotalContributions(data);
        }], true, content => content.FormatWithReplacement(0, request));
    }

    // Originally S4GoodCreature
    private static void CreatureAppeased(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TotalContributions);
    }

    // Originally S4CompareTotals: the Asylum and the Count compare the money they received
    private static void TotalContributions(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();

        int asylum   = vars.AsylumCount;
        int business = globalData.GetActivePlayers().Sum(player => vars.Debt[player]);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, asylum.ToString()).FormatWithReplacement(1, business.ToString()));

        if (asylum == business)
        {
            vars.AsylumCount += 1;
            globalData.ActiveWindow.AddNextContent(1, true);
        }
        else if (business > asylum)
        {
            vars.Jail = 2;
            globalData.ActiveWindow.AddNextContent(2, true);
        }
        else
        {
            vars.Jail = 1;
            globalData.ActiveWindow.AddNextContent(3, true);
        }

        globalData.ActiveWindow.AddClickHereToContinue(CountReturns);
    }

    // Originally S4CountCure
    private static void CountReturns(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (vars.Cure != ExtendedBool.False)
        {
            StartCollection(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(vars.CurePlayer, CursedCure);
    }

    private static void CursedCure(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, vars.CurePlayer).FormatWithReplacement(1, RandomDate(globalData, "CursedCure")));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data =>
        {
            Vars(data).Cure = ExtendedBool.True;
            CureConfirmed(data);
        }, CureDenied], true);
    }

    // Originally CureCountYes
    private static void CureConfirmed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).CurePlayer));
        globalData.ActiveWindow.AddClickHereToContinue(CureConfirmed_0);
    }

    private static void CureConfirmed_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ExperimentCBack, PopUpButton.Confirm, StartCollection);
    }

    // Originally CureCountNo
    private static void CureDenied(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).CurePlayer));
        globalData.ActiveWindow.AddClickHereToContinue(StartCollection);
    }

    // Originally Payment1A - E: the Count collects the money pledged at the House on the Hill
    private static void StartCollection(GlobalData globalData)
    {
        Vars(globalData).PaymentIndex = 0;
        NextCollection(globalData);
    }

    private static void NextCollection(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        while (vars.PaymentIndex < globalData.PlayersNum && vars.Debt[globalData.GetPlayerNameByIndex(vars.PaymentIndex)] == 0) vars.PaymentIndex += 1;

        if (vars.PaymentIndex < globalData.PlayersNum) CollectionDay(globalData);
        else Gen2CreatureEnd(globalData);
    }

    private static void CollectionDay(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex), CollectionDayRevealed);
    }

    private static void CollectionDayRevealed(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, vars.Debt[player].ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [CountPaid, CountDisappointed], true);
    }

    // Originally Payment1ThanksA - E
    private static void CountPaid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex)));
        globalData.ActiveWindow.AddClickHereToContinue(CountPaid_0);
    }

    private static void CountPaid_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Money_Icon, PopUpButton.Confirm, data =>
        {
            Vars(data).Debt[player]  =  0;
            Vars(data).PaymentIndex += 1;
            NextCollection(data);
        }, content => content.FormatWithReplacement(0, vars.Debt[player].ToString()));
    }

    // Originally Payment1NoA - E: the debt grows by $1
    private static void CountDisappointed(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).Debt[player]  += 1;
            Vars(data).PaymentIndex += 1;
            NextCollection(data);
        });
    }

    private static void Gen2CreatureEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm,
            data => Vars(data).RandomElement([CreaturesAid, MoneyTalks], _RND_GEN2_CREATURE_FATE).Invoke(data),
            content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)).FormatWithCondition(1, () => Vars(globalData).HouseOnTheHill == string.Empty));
    }

    // Originally S4Jail1
    private static void CreaturesAid(GlobalData globalData)
    {
        FearOfTheUnknownVars vars    = Vars(globalData);
        bool                 created = vars.CreatureMade == _CREATURE_CREATED;

        globalData.SaveToUndo();
        vars.Gen2Asylum = created;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(created ? 1 : 2);
        globalData.ActiveWindow.AddClickHereToContinue(CreatureRevenge);
    }

    // Originally S4Jail2
    private static void MoneyTalks(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        bool                 count = vars.Jail == 2;

        globalData.SaveToUndo();
        vars.Gen2Asylum = !count;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(count ? 1 : 2);
        globalData.ActiveWindow.AddClickHereToContinue(CreatureRevenge);
    }

    // Originally S4BadCreature
    private static void CreatureRevenge(GlobalData globalData)
    {
        if (Vars(globalData).CreatureMade != _CREATURE_NOT_CREATED)
        {
            Gen3Intro(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CreatureRevenge_0);
    }

    private static void CreatureRevenge_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Bodies_Icon, PopUpButton.Confirm, MinorCompassion);
    }

    // Originally RareCreatureBonus: only when exactly one of the Creature's Experiments was completed
    private static void MinorCompassion(GlobalData globalData)
    {
        if (Vars(globalData).CreatureHelp != 1)
        {
            Gen3Intro(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MinorCompassion_0);
    }

    private static void MinorCompassion_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AdvanceJournalTrack, PopUpButton.Confirm, Gen3Intro);
    }

    private static void Gen3Intro(GlobalData globalData)
    {
        if (Vars(globalData).Gen2Asylum) LiberalIntro(globalData);
        else PrivatizedIntro(globalData);
    }

    #endregion End of Generation II - Creature
}
