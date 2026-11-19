using Asp.Versioning;
using Cms.Idseefeld.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Cms.Idseefeld.Web.Controllers;

[ApiVersion("1.0")]
public sealed class ParagraphEditorController(
    IAuthorizationService authorizationService,
    IContentService contentService,
    IContentEditingService contentEditingService,
    IDocumentEditingPresentationFactory presentationFactory,
    IBackOfficeSecurityAccessor securityAccessor,
    ICoreScopeProvider scopeProvider,
    IAntiforgery antiforgery,
    IJsonSerializer jsonSerializer) : UpdateDocumentControllerBase(authorizationService)
{
    [HttpGet("{id:guid}/paragraph-editor")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get(Guid id)
        => await HandleRequest(id, async () =>
        {
            using ICoreScope scope = scopeProvider.CreateCoreScope();
            scope.ReadLock(Constants.Locks.ContentTree);
            IContent? content = contentService.GetById(id);
            IActionResult? unavailable = CheckAvailability(content);
            if (unavailable is not null)
            {
                return unavailable;
            }

            UpdateDocumentRequestModel model = await presentationFactory.CreateUpdateRequestModelAsync(content!);
            DocumentValueModel? grid = FindGrid(model);
            if (grid is null)
            {
                return NotFound();
            }

            IReadOnlyList<ParagraphText> paragraphs = ParagraphGridEditor.Read(grid, jsonSerializer);
            string token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken
                ?? throw new InvalidOperationException("The antiforgery token could not be created.");
            var result = new ParagraphEditorResponse(content!.PublishedVersionId, token, paragraphs);
            scope.Complete();
            return Ok(result);
        });

    [HttpPut("{id:guid}/paragraph-editor")]
    [MapToApiVersion("1.0")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Guid id, ParagraphEditorRequest request)
        => await HandleRequest(id, async () =>
        {
            if (request.Publish)
            {
                AuthorizationResult publishAccess = await authorizationService.AuthorizeResourceAsync(
                    User,
                    ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, id, []),
                    AuthorizationPolicies.ContentPermissionByResource);
                if (!publishAccess.Succeeded)
                {
                    return Forbidden();
                }
            }

            if (request.Changes.Count == 0)
            {
                return BadRequest(new ProblemDetails { Title = "Es wurden keine Änderungen übermittelt." });
            }

            using ICoreScope scope = scopeProvider.CreateCoreScope();
            scope.WriteLock(Constants.Locks.ContentTree);
            IContent? content = contentService.GetById(id);
            IActionResult? unavailable = CheckAvailability(content);
            if (unavailable is not null)
            {
                return unavailable;
            }

            if (content!.PublishedVersionId != request.PublishedVersionId)
            {
                return Conflict(new ProblemDetails { Title = "Die Seite wurde zwischenzeitlich geändert. Bitte laden Sie sie erneut." });
            }

            UpdateDocumentRequestModel model = await presentationFactory.CreateUpdateRequestModelAsync(content);
            DocumentValueModel? grid = FindGrid(model);
            if (grid is null)
            {
                return NotFound();
            }

            try
            {
                ParagraphGridEditor.PrepareEditorValues(model, jsonSerializer);
                ParagraphGridEditor.Apply(grid, request.Changes, jsonSerializer);
            }
            catch (InvalidDataException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Die Paragraphen konnten nicht zugeordnet werden.", Detail = exception.Message });
            }

            ContentUpdateModel update = presentationFactory.MapUpdateModel(model);
            Guid userKey = CurrentUserKey(securityAccessor);
            if (!request.Publish)
            {
                Attempt<ContentUpdateResult, ContentEditingOperationStatus> result =
                    await contentEditingService.UpdateAsync(id, update, userKey);
                scope.Complete();
                return result.Success ? Ok() : ContentEditingOperationStatusResult(result.Status);
            }

            Attempt<ContentUpdateResult, ContentEditingAndPublishingStatus> publishResult =
                await contentEditingService.UpdateAndPublishAsync(id, update, new HashSet<string>(), userKey);
            scope.Complete();
            if (!publishResult.Success &&
                publishResult.Status.ContentEditingOperationStatus == ContentEditingOperationStatus.Success)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Der Entwurf wurde gespeichert, aber die Veröffentlichung ist fehlgeschlagen.",
                    Detail = publishResult.Status.ContentPublishingOperationStatus?.ToString()
                });
            }

            return publishResult.Success
                ? Ok()
                : DocumentEditingAndPublishingOperationStatusResult(publishResult.Status, publishResult.Result.InvalidPropertyAliases);
        });

    private IActionResult? CheckAvailability(IContent? content)
    {
        if (content is null || content.Trashed || !content.Published)
        {
            return NotFound();
        }

        if (content.ContentType.VariesByCulture())
        {
            return Conflict(new ProblemDetails { Title = "Mehrsprachige Seiten können hier nicht bearbeitet werden." });
        }

        return content.Edited
            ? Conflict(new ProblemDetails { Title = "Für diese Seite existiert ein Entwurf. Bitte verwenden Sie das Backoffice." })
            : null;
    }

    private static DocumentValueModel? FindGrid(UpdateDocumentRequestModel model)
        => model.Values.SingleOrDefault(value =>
            value.Alias == "blockGrid" && value.Culture is null && value.Segment is null && value.Value is not null);
}

public sealed record ParagraphEditorResponse(int PublishedVersionId, string AntiforgeryToken, IReadOnlyList<ParagraphText> Paragraphs);

public sealed record ParagraphEditorRequest(int PublishedVersionId, bool Publish, IReadOnlyList<ParagraphText> Changes);
