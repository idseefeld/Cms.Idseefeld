using System.Data;
using System.Security.Claims;
using Cms.Idseefeld.Web.Controllers;
using Cms.Idseefeld.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Cms.Idseefeld.Web.Tests;

[TestFixture]
public class ParagraphEditorControllerTests
{
    private readonly Guid _documentId = Guid.NewGuid();
    private Mock<IAuthorizationService> _authorization = null!;
    private Mock<IContentService> _contentService = null!;
    private Mock<IContentEditingService> _editing = null!;
    private Mock<IContent> _content = null!;
    private Mock<ICoreScope> _scope = null!;
    private ParagraphEditorController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _authorization = new Mock<IAuthorizationService>();
        _authorization.Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
        _contentService = new Mock<IContentService>();
        _editing = new Mock<IContentEditingService>();
        _content = new Mock<IContent>();
        var type = new Mock<ISimpleContentType>();
        type.SetupGet(item => item.Variations).Returns(ContentVariation.Nothing);
        _content.SetupGet(item => item.ContentType).Returns(type.Object);
        _content.SetupGet(item => item.Published).Returns(true);
        _content.SetupGet(item => item.PublishedVersionId).Returns(17);
        _contentService.Setup(service => service.GetById(_documentId)).Returns(_content.Object);

        _scope = new Mock<ICoreScope>();
        var scopes = new Mock<ICoreScopeProvider>();
        scopes.Setup(provider => provider.CreateCoreScope(
                It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(), It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(_scope.Object);

        _controller = new ParagraphEditorController(
            _authorization.Object,
            _contentService.Object,
            _editing.Object,
            Mock.Of<IDocumentEditingPresentationFactory>(),
            Mock.Of<IBackOfficeSecurityAccessor>(),
            scopes.Object,
            Mock.Of<IAntiforgery>(),
            Mock.Of<IJsonSerializer>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Test]
    public async Task DeniesUsersWithoutUpdatePermission()
    {
        _authorization.Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        IActionResult result = await _controller.Get(_documentId);

        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(403));
        _contentService.Verify(service => service.GetById(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task DeniesPublishingWithoutPublishPermission()
    {
        _authorization.SetupSequence(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success())
            .ReturnsAsync(AuthorizationResult.Failed());

        IActionResult result = await _controller.Save(_documentId, Request(17) with { Publish = true });

        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        Assert.That(((StatusCodeResult)result).StatusCode, Is.EqualTo(403));
        _contentService.Verify(service => service.GetById(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task RejectsExistingDraftBeforeSaving()
    {
        _content.SetupGet(item => item.Edited).Returns(true);

        IActionResult result = await _controller.Save(_documentId, Request(17));

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        _scope.Verify(scope => scope.WriteLock(It.IsAny<int[]>()), Times.Once);
        _editing.Verify(service => service.UpdateAsync(
            It.IsAny<Guid>(), It.IsAny<ContentUpdateModel>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task RejectsPublishedVersionChangedSinceEditingStarted()
    {
        IActionResult result = await _controller.Save(_documentId, Request(16));

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        _scope.Verify(scope => scope.WriteLock(It.IsAny<int[]>()), Times.Once);
        _editing.Verify(service => service.UpdateAsync(
            It.IsAny<Guid>(), It.IsAny<ContentUpdateModel>(), It.IsAny<Guid>()), Times.Never);
    }

    private static ParagraphEditorRequest Request(int publishedVersion)
        => new(publishedVersion, false, [new ParagraphText(Guid.NewGuid(), "<p>Text</p>")]);
}
