using System.Reflection;
using System.Text.RegularExpressions;
using FuzzySharp;

namespace QIMy.AI.Services;

/// <summary>
/// AI-powered duplicate detection implementation
/// Note: FindDuplicate*Async methods require implementation in Application layer with DbContext
/// </summary>
public class AiDuplicateDetectionService : IAiDuplicateDetectionService
{
    public AiDuplicateDetectionService()
    {
    }

    public Task<DuplicateDetectionResult> DetectDuplicatesAsync<TEntity>(
        TEntity entity,
        IEnumerable<TEntity> existingEntities,
        DuplicateDetectionOptions? options = null,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        options ??= new DuplicateDetectionOptions();
        var result = new DuplicateDetectionResult();

        var entityProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var fieldsToCheck = options.FieldsToCheck ?? entityProperties.Select(p => p.Name).ToList();

        foreach (var existingEntity in existingEntities)
        {
            var fieldMatches = new List<FieldMatch>();
            decimal totalScore = 0m;
            decimal totalWeight = 0m;

            foreach (var fieldName in fieldsToCheck)
            {
                var property = entityProperties.FirstOrDefault(p => p.Name == fieldName);
                if (property == null)
                    continue;

                var newValue = property.GetValue(entity)?.ToString();
                var existingValue = property.GetValue(existingEntity)?.ToString();

                if (string.IsNullOrWhiteSpace(newValue) || string.IsNullOrWhiteSpace(existingValue))
                    continue;

                var similarityScore = CalculateSimilarity(newValue, existingValue, options);
                var weight = options.FieldWeights.GetValueOrDefault(fieldName, 1m);

                fieldMatches.Add(new FieldMatch
                {
                    FieldName = fieldName,
                    NewValue = newValue,
                    ExistingValue = existingValue,
                    SimilarityScore = similarityScore,
                    IsExactMatch = similarityScore >= 0.99m
                });

                totalScore += similarityScore * weight;
                totalWeight += weight;
            }

            if (totalWeight > 0)
            {
                var averageScore = totalScore / totalWeight;

                if (averageScore >= options.FuzzyThreshold)
                {
                    var duplicateType = averageScore switch
                    {
                        >= 0.95m => DuplicateType.Exact,
                        >= 0.85m => DuplicateType.Fuzzy,
                        >= 0.75m => DuplicateType.Suspected,
                        _ => DuplicateType.Possible
                    };

                    // Get entity ID if available
                    int entityId = 0;
                    var idProperty = entityProperties.FirstOrDefault(p => p.Name == "Id");
                    if (idProperty != null)
                    {
                        entityId = (int)(idProperty.GetValue(existingEntity) ?? 0);
                    }

                    result.Duplicates.Add(new DuplicateMatch
                    {
                        EntityId = entityId,
                        EntityDescription = GetEntityDescription(existingEntity),
                        MatchScore = averageScore,
                        MatchedFields = fieldMatches.Where(f => f.SimilarityScore >= 0.7m).ToList(),
                        Type = duplicateType,
                        Reason = GenerateReason(fieldMatches, averageScore)
                    });
                }
            }
        }

        result.HasDuplicates = result.Duplicates.Any();
        result.OverallConfidence = result.Duplicates.Any()
            ? result.Duplicates.Max(d => d.MatchScore)
            : 0m;

        result.RecommendedAction = result.OverallConfidence switch
        {
            >= 0.95m => DuplicateAction.Block,
            >= 0.80m => DuplicateAction.Warn,
            _ => DuplicateAction.Allow
        };

        result.Explanation = GenerateExplanation(result);

        return Task.FromResult(result);
    }

    public Task<List<DuplicateMatch>> FindDuplicateClientsAsync(
        string companyName,
        string? vatNumber = null,
        string? email = null,
        string? phone = null,
        CancellationToken cancellationToken = default)
    {
        // NOTE: This method should be implemented in Application layer with DbContext access
        // This is a base implementation that returns empty list
        // Use DetectDuplicatesAsync<Client>() with existing entities for actual duplicate detection
        return Task.FromResult(new List<DuplicateMatch>());
    }

    public Task<List<DuplicateMatch>> FindDuplicateSuppliersAsync(
        string companyName,
        string? vatNumber = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        // NOTE: This method should be implemented in Application layer with DbContext access
        // This is a base implementation that returns empty list
        // Use DetectDuplicatesAsync<Supplier>() with existing entities for actual duplicate detection
        return Task.FromResult(new List<DuplicateMatch>());
    }

    public Task<List<DuplicateMatch>> FindDuplicateInvoicesAsync(
        string invoiceNumber,
        int? clientId = null,
        decimal? amount = null,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        // NOTE: This method should be implemented in Application layer with DbContext access
        // This is a base implementation that returns empty list
        // Use DetectDuplicatesAsync<Invoice>() with existing entities for actual duplicate detection
        return Task.FromResult(new List<DuplicateMatch>());
    }

    private decimal CalculateSimilarity(string value1, string value2, DuplicateDetectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(value1) || string.IsNullOrWhiteSpace(value2))
            return 0m;

        if (options.IgnoreCase)
        {
            value1 = value1.ToLowerInvariant();
            value2 = value2.ToLowerInvariant();
        }

        if (options.IgnoreWhitespace)
        {
            value1 = Regex.Replace(value1, @"\s+", "");
            value2 = Regex.Replace(value2, @"\s+", "");
        }

        // Use FuzzySharp for fuzzy matching
        var ratio = Fuzz.Ratio(value1, value2);
        return ratio / 100m;
    }

    private string NormalizeVat(string vat)
    {
        // Remove spaces, dashes, dots
        return Regex.Replace(vat.ToUpperInvariant(), @"[\s\-\.]", "");
    }

    private string NormalizePhone(string phone)
    {
        // Remove spaces, dashes, parentheses, keep only digits and +
        return Regex.Replace(phone, @"[\s\-\(\)]", "");
    }

    private string GetEntityDescription<TEntity>(TEntity entity)
    {
        var properties = typeof(TEntity).GetProperties();

        // Try common description fields
        var nameProperty = properties.FirstOrDefault(p => p.Name.Contains("Name") || p.Name.Contains("Title"));
        if (nameProperty != null)
        {
            return nameProperty.GetValue(entity)?.ToString() ?? "Unknown";
        }

        // Try ID
        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");
        if (idProperty != null)
        {
            return $"ID: {idProperty.GetValue(entity)}";
        }

        return "Unknown";
    }

    private string GenerateReason(List<FieldMatch> fieldMatches, decimal averageScore)
    {
        var exactMatches = fieldMatches.Where(f => f.IsExactMatch).ToList();
        var fuzzyMatches = fieldMatches.Where(f => !f.IsExactMatch && f.SimilarityScore >= 0.7m).ToList();

        if (exactMatches.Any())
        {
            return $"Exact match on: {string.Join(", ", exactMatches.Select(f => f.FieldName))}";
        }

        if (fuzzyMatches.Any())
        {
            return $"High similarity ({averageScore:P0}) on: {string.Join(", ", fuzzyMatches.Select(f => f.FieldName))}";
        }

        return $"Overall similarity: {averageScore:P0}";
    }

    private string GenerateExplanation(DuplicateDetectionResult result)
    {
        if (!result.HasDuplicates)
            return "No duplicates found. Safe to proceed.";

        var bestMatch = result.Duplicates.OrderByDescending(d => d.MatchScore).First();

        return result.RecommendedAction switch
        {
            DuplicateAction.Block => $"Found {result.Duplicates.Count} duplicate(s) with high confidence ({bestMatch.MatchScore:P0}). Creation blocked to prevent duplicates.",
            DuplicateAction.Warn => $"Found {result.Duplicates.Count} potential duplicate(s) ({bestMatch.MatchScore:P0}). Please review before proceeding.",
            _ => $"Found {result.Duplicates.Count} possible duplicate(s) with low confidence. You may proceed with caution."
        };
    }
}
