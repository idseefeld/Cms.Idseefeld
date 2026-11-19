using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using Cms.Idseefeld.Web.Services;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Cms.Idseefeld.Web.Tests;

[TestFixture]
public class ParagraphGridEditorTests
{
    private static readonly Guid First = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Empty = new("22222222-2222-2222-2222-222222222222");
    private static readonly IJsonSerializer Serializer = CreateSerializer();

    private static IJsonSerializer CreateSerializer()
    {
        var factory = new Mock<IJsonSerializerEncoderFactory>();
        factory.Setup(item => item.CreateEncoder<SystemTextJsonSerializer>()).Returns(JavaScriptEncoder.Default);
        return new SystemTextJsonSerializer(factory.Object);
    }

    private const string Grid = """
        {
          "layout":{"Umbraco.BlockGrid":[{"contentKey":"layout","areas":[{"items":[{"contentKey":"11111111-1111-1111-1111-111111111111"},{"contentKey":"22222222-2222-2222-2222-222222222222"}]}]}]},
          "contentData":[
            {"contentTypeKey":"b9dd3c44-f474-4dc0-b7ed-a2ca5912e84a","key":"11111111-1111-1111-1111-111111111111","values":[{"alias":"text","culture":null,"segment":null,"editorAlias":"Umbraco.RichText","value":{"markup":"<p>Alt</p>","blocks":{"layout":{"custom":[1]},"contentData":[{"key":"keep"}]}}}]},
            {"contentTypeKey":"b9dd3c44-f474-4dc0-b7ed-a2ca5912e84a","key":"22222222-2222-2222-2222-222222222222","values":[]},
            {"contentTypeKey":"73998762-808e-4ded-a789-0b239327ba6b","key":"33333333-3333-3333-3333-333333333333","values":[{"alias":"text","value":"Headline"}]}
          ],
          "settingsData":[{"key":"settings","values":[{"value":"keep"}]}],
          "expose":[{"contentKey":"11111111-1111-1111-1111-111111111111","culture":null,"segment":null}]
        }
        """;

    [Test]
    public void ReadsParagraphsIncludingEmptyBlocks()
    {
        var value = new DocumentValueModel { Alias = "blockGrid", Value = JsonSerializer.Deserialize<JsonElement>(Grid) };

        IReadOnlyList<ParagraphText> paragraphs = ParagraphGridEditor.Read(value, Serializer);

        Assert.That(paragraphs, Is.EqualTo(new[]
        {
            new ParagraphText(First, "<p>Alt</p>"),
            new ParagraphText(Empty, string.Empty)
        }));
    }

    [Test]
    public void ReadsEditorObjectsUsingManagementApiPropertyNames()
    {
        var value = new DocumentValueModel
        {
            Alias = "blockGrid",
            Value = new
            {
                ContentData = new[]
                {
                    new
                    {
                        ContentTypeKey = "b9dd3c44-f474-4dc0-b7ed-a2ca5912e84a",
                        Key = First,
                        Values = Array.Empty<object>()
                    }
                }
            }
        };

        Assert.That(ParagraphGridEditor.Read(value, Serializer), Is.EqualTo(new[] { new ParagraphText(First, string.Empty) }));
    }

    [Test]
    public void PreservesNestedLayoutWhenGridIsUmbracoEditorObject()
    {
        Guid parent = Guid.NewGuid();
        Guid area = Guid.NewGuid();
        Guid parentType = Guid.NewGuid();
        Guid paragraphType = new("b9dd3c44-f474-4dc0-b7ed-a2ca5912e84a");
        var grid = new BlockGridValue([
            new BlockGridLayoutItem(parent)
            {
                ColumnSpan = 12,
                RowSpan = 2,
                Areas = [new BlockGridLayoutAreaItem(area)
                {
                    Items = [new BlockGridLayoutItem(First) { ColumnSpan = 6, RowSpan = 1 }]
                }]
            }
        ])
        {
            ContentData = [
                new BlockItemData(parent, parentType, "container"),
                new BlockItemData(First, paragraphType, "wysiwg65_paragraph")
                {
                    Values = [new BlockPropertyValue { Alias = "text", Value = new { markup = "<p>Alt</p>" } }]
                }
            ],
            Expose = [new BlockItemVariation(parent, null, null), new BlockItemVariation(First, null, null)]
        };
        var value = new DocumentValueModel { Alias = "blockGrid", Value = grid };
        var model = new UpdateDocumentRequestModel { Values = [value] };
        JsonNode original = JsonNode.Parse(Serializer.Serialize(grid))!;

        ParagraphGridEditor.PrepareEditorValues(model, Serializer);
        ParagraphGridEditor.Apply(value, [new ParagraphText(First, "<p>Neu</p>")], Serializer);

        JsonNode actual = JsonSerializer.SerializeToNode(value.Value)!;
        original["contentData"]![1]!["values"]![0]!["value"]!["markup"] = "<p>Neu</p>";
        Assert.That(JsonNode.DeepEquals(actual, original), Is.True);
        JsonNode layout = actual["layout"]!["Umbraco.BlockGrid"]![0]!;
        Assert.That(layout["columnSpan"]!.GetValue<int>(), Is.EqualTo(12));
        Assert.That(layout["rowSpan"]!.GetValue<int>(), Is.EqualTo(2));
        Assert.That(layout["areas"]![0]!["key"]!.GetValue<Guid>(), Is.EqualTo(area));
        Assert.That(layout["areas"]![0]!["items"]![0]!["contentKey"]!.GetValue<Guid>(), Is.EqualTo(First));
        Assert.That(actual["contentData"]![1]!["values"]![0]!["value"]!["markup"]!.GetValue<string>(), Is.EqualTo("<p>Neu</p>"));

        var textProperty = new Mock<IPropertyType>();
        textProperty.SetupGet(item => item.Alias).Returns("text");
        var containerType = new Mock<IContentType>();
        containerType.SetupGet(item => item.Key).Returns(parentType);
        containerType.SetupGet(item => item.Alias).Returns("container");
        containerType.SetupGet(item => item.CompositionPropertyTypes).Returns([]);
        var paragraphContentType = new Mock<IContentType>();
        paragraphContentType.SetupGet(item => item.Key).Returns(paragraphType);
        paragraphContentType.SetupGet(item => item.Alias).Returns("wysiwg65_paragraph");
        paragraphContentType.SetupGet(item => item.CompositionPropertyTypes).Returns([textProperty.Object]);
        var typeCache = new Mock<IBlockEditorElementTypeCache>();
        typeCache.Setup(cache => cache.GetMany(It.IsAny<IEnumerable<Guid>>()))
            .Returns([containerType.Object, paragraphContentType.Object]);
        var blockValues = new BlockEditorValues<BlockGridValue, BlockGridLayoutItem>(
            new BlockGridEditorDataConverter(Serializer), typeCache.Object, NullLogger.Instance);

        BlockEditorData<BlockGridValue, BlockGridLayoutItem>? parsed = blockValues.DeserializeAndClean(value.Value);
        Assert.That(parsed, Is.Not.Null);
        Assert.That(parsed!.References.Select(reference => reference.ContentKey), Does.Contain(First));
        Assert.That(parsed.BlockValue.ContentData.Select(block => block.Key), Is.EquivalentTo(new[] { parent, First }));
    }

    [Test]
    public void PreparesOtherTypedBlockGridValuesUsingUmbracoSerializer()
    {
        Guid parent = Guid.NewGuid();
        Guid area = Guid.NewGuid();
        var otherGrid = new BlockGridValue([
            new BlockGridLayoutItem(parent)
            {
                Areas = [new BlockGridLayoutAreaItem(area) { Items = [new BlockGridLayoutItem(First)] }]
            }
        ]);
        var other = new DocumentValueModel { Alias = "otherGrid", Value = otherGrid };
        var model = new UpdateDocumentRequestModel { Values = [other] };

        ParagraphGridEditor.PrepareEditorValues(model, Serializer);

        var value = (JsonObject)other.Value!;
        Assert.That(value["layout"]!["Umbraco.BlockGrid"]![0]!["areas"]![0]!["items"]![0]!["contentKey"]!.GetValue<Guid>(), Is.EqualTo(First));
    }

    [Test]
    public void ChangesOnlyRequestedMarkupAndPreservesGridAndRichTextBlocks()
    {
        var value = new DocumentValueModel { Alias = "blockGrid", Value = Grid };
        JsonNode expected = JsonNode.Parse(Grid)!;
        expected["contentData"]![0]!["values"]![0]!["value"]!["markup"] = "<p>Neu</p>";

        ParagraphGridEditor.Apply(value, [new ParagraphText(First, "<p>Neu</p>")], Serializer);

        JsonNode actual = JsonSerializer.SerializeToNode(value.Value)!;
        Assert.That(JsonNode.DeepEquals(actual, expected), Is.True);
    }

    [Test]
    public void CreatesRichTextValueForEmptyParagraph()
    {
        var value = new DocumentValueModel { Alias = "blockGrid", Value = Grid };

        ParagraphGridEditor.Apply(value, [new ParagraphText(Empty, "<p>Text</p>")], Serializer);

        JsonNode actual = JsonSerializer.SerializeToNode(value.Value)!;
        JsonNode text = actual["contentData"]![1]!["values"]![0]!;
        Assert.That(text["alias"]!.GetValue<string>(), Is.EqualTo("text"));
        Assert.That(text["value"]!["markup"]!.GetValue<string>(), Is.EqualTo("<p>Text</p>"));
        Assert.That(text["value"]!["blocks"]!["contentData"]!.AsArray(), Is.Empty);
    }

    [Test]
    public void RejectsUnknownAndDuplicateParagraphsWithoutModifyingGrid()
    {
        var value = new DocumentValueModel { Alias = "blockGrid", Value = Grid };

        Assert.Throws<InvalidDataException>(() => ParagraphGridEditor.Apply(value,
            [new ParagraphText(First, "new"), new ParagraphText(Guid.NewGuid(), "bad")], Serializer));
        Assert.That(value.Value, Is.EqualTo(Grid));
        Assert.Throws<InvalidDataException>(() => ParagraphGridEditor.Apply(value,
            [new ParagraphText(First, "new"), new ParagraphText(First, "duplicate")], Serializer));
        Assert.That(value.Value, Is.EqualTo(Grid));
    }
}
