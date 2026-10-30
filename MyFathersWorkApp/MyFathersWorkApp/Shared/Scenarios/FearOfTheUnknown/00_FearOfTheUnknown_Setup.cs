using System.Runtime.CompilerServices;

namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    // RandomArray indices - fixed per decision so undo / load / mirror mode replay identically
    private const int _RND_TRACKER            = 0;
    private const int _RND_FIRST_PLAYER       = 1;
    private const int _RND_GEN1_FATE          = 2;
    private const int _RND_OBSESSION          = 3;   // 3 - 6, per player slot
    private const int _RND_BRICK_VALUE        = 7;
    private const int _RND_WITCH              = 8;
    private const int _RND_SUSPECT_HEX1       = 9;
    private const int _RND_SUSPECT_HEX2       = 10;
    private const int _RND_SUSPECT_POINT      = 11;
    private const int _RND_SUSPECT_LAST_HEX   = 12;
    private const int _RND_KEY_VP             = 13;
    private const int _RND_GEN2_FOREIGN_FATE  = 14;
    private const int _RND_WALL_BRICKS        = 15;
    private const int _RND_CARAVAN            = 16;  // 16 - 20, per end of round (see CaravanCheck)
    private const int _RND_ASYLUM_UPGRADE     = 21;
    private const int _RND_ASYLUM_COST        = 22;
    private const int _RND_CREATURE_REQUEST   = 23;
    private const int _RND_GEN2_CREATURE_FATE = 24;
    private const int _RND_BARGAIN            = 25;  // 25 - 34, per business meeting
    private const int _RND_BARGAIN_VP         = 35;  // 35 - 44, per business meeting
    private const int _RND_COLLABORATION      = 45;
    private const int _RND_SMUGGLING          = 46;  // 46 - 55, per visit of the Smuggler's Dock
    private const int _RND_TRIAL_COST         = 56;  // 56 - 65
    private const int _RND_TRIAL              = 66;  // 66 - 75
    private const int _RND_TRIAL_VP           = 76;  // 76 - 85
    private const int _RND_POLITICS           = 86;  // 86 - 95
    private const int _RND_TOWN_HALL_CREEPY   = 96;
    private const int _RND_RAMPAGE_1          = 97;
    private const int _RND_RAMPAGE_2          = 98;
    private const int _RND_RAMPAGE_END        = 99;
    private const int _RND_CROSSROADS         = 100;
    private const int _RND_OBSESSION_VP       = 101;
    private const int _RND_JUNK_PENALTY       = 102;
    private const int _RND_COUNT_FAREWELL     = 103;
    private const int _RND_EXPEDITION         = 104; // 104 - 106, per round
    private const int _RND_LIBERAL_EVENT      = 107;
    private const int _RND_SALTS              = 108;
    private const int _RND_SALTS_REFUSED      = 109;
    private const int _RND_SALTS_SPOUSE       = 110;
    private const int _RND_BRIBE              = 111;
    private const int _RND_POLICY             = 112;
    private const int _RND_TAXES_VP           = 113;
    private const int _RND_TAXES_LOSS         = 114;
    private const int _RND_ASYLUM_OUTCOME     = 115; // 115 - 124, per admission
    private const int _RND_ASYLUM_QUESTION    = 125; // 125 - 164, admission * 4 + question
    private const int _RND_BATTLE_PAIR        = 400; // 400 - 649, per attack
    private const int _RND_BATTLE_HIT         = 650; // 650 - 899, per attack
    private const int _BATTLE_MAX_STEPS       = 250;

    public static void Init(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(Prologue);
    }

    private static void Prologue(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ScenarioStart);
    }

    private static void ScenarioStart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("FearoftheUnknownStart");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_I);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(SharedMania);
    }

    private static void SharedMania(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(SharedMania_0);
    }

    private static void SharedMania_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.StorybookToken, PopUpButton.Confirm, WitchHuntSetup);
    }

    #region Helpers

    private static FearOfTheUnknownVars Vars(GlobalData globalData) => globalData.FearOfTheUnknownVars;

    private static string Tag(GlobalData globalData, string tag) => globalData.GetScenarioLocalizedTag(tag);

    // Flavor: a random player who wrote the letter / journal. The same key gives the same author (for "(cont.)" pages).
    private static string RandomPlayer(GlobalData globalData, [CallerMemberName] string key = "")
    {
        return Vars(globalData).FlavorElement(globalData.GetActivePlayers().ToList(), key + "#author");
    }

    // Flavor: one of the alternative sentences of a passage ({{0=|first|second|...}} in the CSV)
    private static Func<string, string> Either(GlobalData globalData, int id, int count, string key)
    {
        return content => content.FormatWithIndex(id, Vars(globalData).Flavor(key + "#" + id, count));
    }

    // Flavor: a date within the years of the current round (originally date1a). Each date of a round is later than the
    // previous one: the round starts at a random day early in its first year and every date moves 1 to 10 months on.
    private static string RandomDate(GlobalData globalData, string key, string tag = "Flavor_Date")
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        int first = (globalData.Generation, globalData.Years) switch
        {
            _ when globalData.Generation > Generation.Third => 1912, // after the last round
            (Generation.First, Years.Early)   => 1831,
            (Generation.First, Years.Middle)  => 1842,
            (Generation.First, _)             => 1852,
            (Generation.Second, Years.Early)  => 1863,
            (Generation.Second, Years.Middle) => 1872,
            (Generation.Second, _)            => 1881,
            (_, Years.Early)                  => 1892,
            (_, Years.Middle)                 => 1902,
            _                                 => 1912
        };

        int last = first + (first is 1831 or 1852 or 1881 or 1912 ? 10 : first is 1863 or 1872 ? 8 : 9);

        if (vars.DateRound != first)
        {
            vars.DateRound = first;
            vars.DateDays  = vars.Flavor(key + "#start", 120);
        }

        vars.DateDays += 30 + vars.Flavor(key + "#step", 271);

        DateTime start = new(first, 1, 1);
        DateTime end   = new(last, 12, 31);
        DateTime date  = start.AddDays(vars.DateDays);
        if (date > end) date = end;

        return Tag(globalData, tag)
              .FormatWithReplacement(0, date.Day.ToString())
              .FormatWithIndex(1, date.Month - 1)
              .FormatWithReplacement(2, date.Year.ToString());
    }

    // Bold letter / journal header: "<prefix> <author> - <date>." Prefix tags hold alternatives ({{0=|Letter from |...}})
    private static string Header(GlobalData globalData, string prefixTag, int prefixIndex, string author, string date)
    {
        return Tag(globalData, "Flavor_Header")
              .FormatWithReplacement(0, Tag(globalData, prefixTag).FormatWithIndex(0, prefixIndex) + author)
              .FormatWithReplacement(1, date);
    }

    // Random letter header (originally letter1a) - the author stays the same for the passages continuing the letter
    private static string LetterHeader(GlobalData globalData, [CallerMemberName] string key = "")
    {
        return Header(globalData, "Flavor_Letter", Vars(globalData).Flavor(key + "#prefix", 4), RandomPlayer(globalData, key), RandomDate(globalData, key));
    }

    // Random journal header (originally journal1a)
    private static string JournalHeader(GlobalData globalData, [CallerMemberName] string key = "")
    {
        return Header(globalData, "Flavor_Journal", Vars(globalData).Flavor(key + "#prefix", 4), RandomPlayer(globalData, key), RandomDate(globalData, key));
    }

    // Display name of a player or of the Apprentice suspect
    private static string Name(GlobalData globalData, string player)
    {
        return player == FearOfTheUnknownVars.SUSPECT ? Suspect(globalData) : player;
    }

    // "Butcher's Apprentice" / "Blacksmith's Apprentice" - the one the family spared
    private static string Suspect(GlobalData globalData)
    {
        return Tag(globalData, "Suspect_Name").FormatWithIndex(0, Vars(globalData).Victim);
    }

    // Players and, with 2 or 3 players, the Apprentice
    private static bool SuspectInPlay(GlobalData globalData) => globalData.PlayersNum <= 3;

    // Index of a player in turn order (A = 0 ... D = 3)
    private static int PlayerIndex(GlobalData globalData, string player)
    {
        return Array.IndexOf(globalData.GetActivePlayers(), player);
    }

    #endregion Helpers
}
