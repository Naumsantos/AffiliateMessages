namespace AffiliateMessages.Services;

public sealed record BatchProcessResult(
    IReadOnlyList<string> Messages,
    int InvalidRowsCount,
    int MessagesWithoutPhrase);
