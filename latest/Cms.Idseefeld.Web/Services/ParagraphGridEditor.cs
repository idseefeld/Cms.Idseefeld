using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Serialization;

namespace Cms.Idseefeld.Web.Services;

public static class ParagraphGridEditor
{
    private static readonly Guid ParagraphTypeKey = new("b9dd3c44-f474-4dc0-b7ed-a2ca5912e84a");
    public static IReadOnlyList<ParagraphText> Read(DocumentValueModel gridValue, IJsonSerializer serializer)
    {
        JsonObject grid = ParseGrid(gridValue, serializer);
        return FindParagraphs(grid)
            .Select(block => new ParagraphText(
                ReadKey(block),
                FindTextValue(block)?["value"]?["markup"]?.GetValue<string>() ?? string.Empty))
            .ToArray();
    }

    public static void Apply(DocumentValueModel gridValue, IReadOnlyList<ParagraphText> changes, IJsonSerializer serializer)
    {
        JsonObject grid = ParseGrid(gridValue, serializer);
        Dictionary<Guid, JsonObject> blocks = FindParagraphs(grid).ToDictionary(ReadKey);
        var seen = new HashSet<Guid>();

        foreach (ParagraphText change in changes)
        {
            if (!seen.Add(change.Key) || !blocks.TryGetValue(change.Key, out JsonObject? block))
            {
                throw new InvalidDataException("A paragraph is duplicated or no longer exists.");
            }

            JsonObject? text = FindTextValue(block);
            if (text is null)
            {
                if (block["values"] is not JsonArray values)
                {
                    throw new InvalidDataException("The paragraph has no values array.");
                }

                text = new JsonObject
                {
                    ["alias"] = "text",
                    ["editorAlias"] = "Umbraco.RichText",
                    ["culture"] = null,
                    ["segment"] = null,
                    ["value"] = null
                };
                values.Add(text);
            }

            if (text["value"] is null)
            {
                text["value"] = new JsonObject
                {
                    ["markup"] = string.Empty,
                    ["blocks"] = new JsonObject
                    {
                        ["layout"] = new JsonObject(),
                        ["contentData"] = new JsonArray(),
                        ["settingsData"] = new JsonArray(),
                        ["expose"] = new JsonArray()
                    }
                };
            }

            if (text["value"] is not JsonObject richText)
            {
                throw new InvalidDataException("The paragraph has an invalid rich-text value.");
            }

            richText["markup"] = change.Markup;
        }

        gridValue.Value = JsonSerializer.SerializeToElement(grid);
    }

    public static void PrepareEditorValues(UpdateDocumentRequestModel model, IJsonSerializer serializer)
    {
        // Match the Management API JSON round-trip, including Umbraco's polymorphic block layout converter.
        foreach (DocumentValueModel value in model.Values)
        {
            if (value.Value is not null)
            {
                value.Value = serializer.Deserialize<object>(serializer.Serialize(value.Value));
            }
        }
    }

    private static JsonObject ParseGrid(DocumentValueModel value, IJsonSerializer serializer)
    {
        JsonNode? node = value.Value switch
        {
            string json => JsonNode.Parse(json),
            null => null,
            _ => JsonNode.Parse(serializer.Serialize(value.Value))
        };
        return node as JsonObject ?? throw new InvalidDataException("The block grid is not an object.");
    }

    private static IEnumerable<JsonObject> FindParagraphs(JsonObject grid)
    {
        if (grid["contentData"] is not JsonArray blocks)
        {
            throw new InvalidDataException("The block grid has no content data.");
        }

        return blocks.OfType<JsonObject>().Where(block =>
            Guid.TryParse(block["contentTypeKey"]?.GetValue<string>(), out Guid typeKey)
            && typeKey == ParagraphTypeKey);
    }

    private static Guid ReadKey(JsonObject block)
        => Guid.TryParse(block["key"]?.GetValue<string>(), out Guid key)
            ? key
            : throw new InvalidDataException("A paragraph has no valid key.");

    private static JsonObject? FindTextValue(JsonObject block)
        => (block["values"] as JsonArray)?.OfType<JsonObject>()
            .SingleOrDefault(value => value["alias"]?.GetValue<string>() == "text"
                && value["culture"] is null && value["segment"] is null);
}

public sealed record ParagraphText(Guid Key, string Markup);
