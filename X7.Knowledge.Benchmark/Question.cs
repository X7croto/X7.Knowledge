using System.Text.Json.Serialization;

namespace X7.Knowledge.Benchmark;

public sealed record Question
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("expectedCapability")]
    public required string ExpectedCapability { get; init; }

    [JsonPropertyName("codeFiles")]
    public required IReadOnlyList<string> CodeFiles { get; init; }

    [JsonPropertyName("kbFiles")]
    public required IReadOnlyList<string> KbFiles { get; init; }

    [JsonPropertyName("retired")]
    public bool Retired { get; init; }
}

public sealed record QuestionSet
{
    [JsonPropertyName("benchmarkVersion")]
    public required int BenchmarkVersion { get; init; }

    [JsonPropertyName("referenceSolution")]
    public required string ReferenceSolution { get; init; }

    [JsonPropertyName("questions")]
    public required IReadOnlyList<Question> Questions { get; init; }
}

public sealed record Measurement
{
    public required Question Question { get; init; }

    public required int CodeTokens { get; init; }

    public required int KbTokens { get; init; }

    public required IReadOnlyList<string> MissingCodeFiles { get; init; }

    /// <summary>BM-04: sem kbFiles, a Base não sustenta a resposta.</summary>
    public bool Supported => Question.KbFiles.Count > 0;

    public double? ContextRatio => Supported && CodeTokens > 0
        ? (double)KbTokens / CodeTokens
        : null;
}
