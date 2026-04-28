namespace LeanLedgerServer.Automation;

using System.Text.Json;
using System.Text.Json.Serialization;
using LeanLedgerServer.PiggyBanks;
using LeanLedgerServer.Transactions;

[JsonConverter(typeof(RuleActionConverter))]
public abstract record RuleAction() {
    // TODO: Remove these methods and just use static dispatch from the call site
    public abstract RuleEffect RunOn(Transaction transaction);
}

public record Append(
    RuleTransactionField Field,
    string Value
): RuleAction {
    public override RuleEffect RunOn(Transaction transaction) {
        var transactionValue = Field.GetValueFrom(transaction);

        if (transactionValue is null) {
            Field.ApplyValueTo(transaction, Value);
        } else {
            transactionValue += Value;
            Field.ApplyValueTo(transaction, transactionValue + Value);
        }


        return new ChangedField(Field, transactionValue, Field.GetValueFrom(transaction));
    }
}

public record Set(
    RuleTransactionField Field,
    string Value
): RuleAction {
    public override RuleEffect RunOn(Transaction transaction) {
        var oldValue = Field.GetValueFrom(transaction);
        Field.ApplyValueTo(transaction, Value);
        return new ChangedField(Field, oldValue, Field.GetValueFrom(transaction));
    }
}

public record Clear(RuleTransactionField Field): RuleAction {
    public override RuleEffect RunOn(Transaction transaction) {
        var oldValue = Field.GetValueFrom(transaction);
        Field.ApplyValueTo(transaction, null);

        return new ChangedField(Field, oldValue, null);
    }
}

public record DeleteTransaction(): RuleAction {
    public override RuleEffect RunOn(Transaction transaction) {
        transaction.IsDeleted = true;
        return new DeletedTransaction();
    }
}

public record CreatePiggyEntry(Guid PiggyBankId, decimal Amount, string Description): RuleAction {
    public override RuleEffect RunOn(Transaction transaction) {
        var entry = new PiggyBankEntry {
            PiggyBankId = PiggyBankId,
            Amount = Amount,
            Description = Description,
            Date = transaction.Date,
        };

        return new CreatedPiggyEntry(entry);
    }
}

public static class RuleActionMap {
    private static readonly Dictionary<string, Type> _byName;
    private static readonly Dictionary<Type, string> _byType;

    static RuleActionMap() {
        var baseType = typeof(RuleAction);

        _byName = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToDictionary(
                ToDiscriminator,
                t => t,
                StringComparer.OrdinalIgnoreCase
            );

        _byType = _byName.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public static bool TryGetType(string discriminator, out Type type)
        => _byName.TryGetValue(discriminator, out type!);

    public static bool TryGetDiscriminator(Type type, out string discriminator)
        => _byType.TryGetValue(type, out discriminator!);

    private static string ToDiscriminator(Type t) {
        // Convention: EmailAction → Email not really in use, but just in case
        const string suffix = "Action";
        return t.Name.EndsWith(suffix)
            ? t.Name[..^suffix.Length]
            : t.Name;
    }
}

public class RuleActionConverter: JsonConverter<RuleAction> {
    public override RuleAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var discriminatorFieldName = options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase
            ? "actionType"
            : "ActionType";

        if (!root.TryGetProperty(discriminatorFieldName, out var typeProp)) {
            throw new JsonException("Missing ActionType");
        }

        var discriminator = typeProp.GetString();

        if (discriminator is null || !RuleActionMap.TryGetType(discriminator, out var targetType)) {
            throw new NotSupportedException($"Unknown ActionType: {discriminator}");
        }
        var json = root.GetRawText();
        var result = (RuleAction)JsonSerializer.Deserialize(
            json,
            targetType,
            options
        )!;

        return result;
    }

    public override void Write(Utf8JsonWriter writer, RuleAction value, JsonSerializerOptions options) {
        if (!RuleActionMap.TryGetDiscriminator(value.GetType(), out var discriminator)) {
            throw new NotSupportedException($"Unmapped type: {value.GetType()}");
        }
        var discriminatorFieldName = options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase
            ? "actionType"
            : "ActionType";

        writer.WriteStartObject();
        writer.WriteString(discriminatorFieldName, discriminator);

        var properties = JsonSerializer
             .SerializeToElement(value, value.GetType(), options)
             .EnumerateObject();

        foreach (var prop in properties) {
            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}

