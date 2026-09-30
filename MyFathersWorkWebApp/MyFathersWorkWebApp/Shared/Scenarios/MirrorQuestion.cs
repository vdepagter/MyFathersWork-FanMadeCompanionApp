namespace MyFathersWorkWebApp;

// Thrown by a random draw in mirror mode when the players have not yet told which value the official app used
public class MirrorQuestionException(int index, int count, Func<int, string> valueLabel, int? numericMin) : Exception($"Mirror question for random slot {index}")
{
    public int               Index      { get; } = index;
    public int               Count      { get; } = count;
    public Func<int, string> ValueLabel { get; } = valueLabel;
    public int?              NumericMin { get; } = numericMin; // set for RandomInRange, allows typing the number
}

public class MirrorOption(int value, string changed, string before = "", string after = "")
{
    public int    Value   { get; } = value;   // RandomArray value that produces this outcome
    public string Changed { get; } = changed; // Part of the resulting screen that differs from the other options (plain text)
    public string Before  { get; } = before;  // Unchanged text right before / after it, for long options
    public string After   { get; } = after;
}

public class MirrorQuestion
{
    public required int                Index      { get; init; }
    public required int                Count      { get; init; }
    public required Action             Retry      { get; init; }
    public required List<MirrorOption> Options    { get; init; }
    public          string             Before     { get; init; } = string.Empty; // Text shared by all options, before the change (short options)
    public          string             After      { get; init; } = string.Empty; // Text shared by all options, after the change (short options)
    public          int?               NumericMin { get; init; }
    public          bool               Hidden     { get; init; } // Every value gives the same screen, the official app shows it later

    public bool IsNumeric   => NumericMin != null && Count > MaxOptions;
    public bool ShortLabels => Options.All(option => option.Changed.Length <= ShortLabelLength && option.Before == string.Empty && option.After == string.Empty);

    public const int MaxOptions       = 12;
    public const int ShortLabelLength = 24;
}
