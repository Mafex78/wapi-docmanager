using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Tests.Features.Documents;

/// <summary>
/// Visibilità delle azioni per ruolo e stato: deve riflettere gli [Authorize(Roles)] di DocumentsController e UsersController.
/// </summary>
public class DocumentPermissionsTests
{
    [Theory]
    [InlineData(new[] { RoleType.Viewer }, true, false)]
    [InlineData(new[] { RoleType.Editor }, true, true)]
    [InlineData(new[] { RoleType.Admin }, true, true)]
    [InlineData(new RoleType[] { }, false, false)]
    public void CanRead_And_CanWrite_Depend_On_Roles(RoleType[] roles, bool canRead, bool canWrite)
    {
        Assert.Equal(canRead, DocumentPermissions.CanRead(roles));
        Assert.Equal(canWrite, DocumentPermissions.CanWrite(roles));
    }

    [Theory]
    [InlineData(DocumentStatus.Draft, true)]
    [InlineData(DocumentStatus.Ready, true)]
    [InlineData(DocumentStatus.Sent, false)]
    public void Editor_CanEdit_And_CanDelete_Depend_On_Status(DocumentStatus status, bool expected)
    {
        RoleType[] roles = { RoleType.Editor };

        Assert.Equal(expected, DocumentPermissions.CanEdit(roles, status));
        Assert.Equal(expected, DocumentPermissions.CanDelete(roles, status));
    }

    [Fact]
    public void Viewer_Cannot_Edit_Delete_Or_Change_Status()
    {
        RoleType[] roles = { RoleType.Viewer };

        Assert.False(DocumentPermissions.CanEdit(roles, DocumentStatus.Draft));
        Assert.False(DocumentPermissions.CanDelete(roles, DocumentStatus.Draft));
        Assert.False(DocumentPermissions.CanChangeStatus(roles, DocumentStatus.Draft));
    }

    [Theory]
    [InlineData(DocumentStatus.Draft, true)]
    [InlineData(DocumentStatus.Sent, true)]
    [InlineData(DocumentStatus.Approved, false)]
    [InlineData(DocumentStatus.Rejected, false)]
    public void Editor_CanChangeStatus_Only_When_A_Transition_Exists(DocumentStatus status, bool expected)
    {
        Assert.Equal(expected, DocumentPermissions.CanChangeStatus(new[] { RoleType.Editor }, status));
    }

    [Theory]
    [InlineData(new[] { RoleType.Admin }, true)]
    [InlineData(new[] { RoleType.Editor, RoleType.Viewer }, false)]
    public void CanRegisterUsers_Only_Admin(RoleType[] roles, bool expected)
    {
        Assert.Equal(expected, DocumentPermissions.CanRegisterUsers(roles));
    }

    [Theory]
    [InlineData(new[] { RoleType.Viewer }, DocumentStatus.Draft, PermissionDenialReason.MissingRole)]
    [InlineData(new[] { RoleType.Viewer }, DocumentStatus.Sent, PermissionDenialReason.MissingRole)]
    [InlineData(new RoleType[] { }, DocumentStatus.Ready, PermissionDenialReason.MissingRole)]
    [InlineData(new[] { RoleType.Editor }, DocumentStatus.Draft, PermissionDenialReason.None)]
    [InlineData(new[] { RoleType.Admin }, DocumentStatus.Ready, PermissionDenialReason.None)]
    [InlineData(new[] { RoleType.Editor }, DocumentStatus.Sent, PermissionDenialReason.InvalidStatus)]
    [InlineData(new[] { RoleType.Admin }, DocumentStatus.Approved, PermissionDenialReason.InvalidStatus)]
    [InlineData(new[] { RoleType.Editor, RoleType.Viewer }, DocumentStatus.Rejected, PermissionDenialReason.InvalidStatus)]
    public void GetEditDenialReason_And_GetDeleteDenialReason_Check_Role_Before_Status(
        RoleType[] roles,
        DocumentStatus status,
        PermissionDenialReason expected)
    {
        Assert.Equal(expected, DocumentPermissions.GetEditDenialReason(roles, status));
        Assert.Equal(expected, DocumentPermissions.GetDeleteDenialReason(roles, status));
    }

    [Fact]
    public void DenialReason_Is_None_Only_When_Action_Is_Allowed()
    {
        RoleType[][] roleSets =
        {
            Array.Empty<RoleType>(),
            new[] { RoleType.Viewer },
            new[] { RoleType.Editor },
            new[] { RoleType.Admin }
        };

        foreach (RoleType[] roles in roleSets)
        {
            foreach (DocumentStatus status in Enum.GetValues<DocumentStatus>())
            {
                Assert.Equal(
                    DocumentPermissions.CanEdit(roles, status),
                    DocumentPermissions.GetEditDenialReason(roles, status) == PermissionDenialReason.None);
                Assert.Equal(
                    DocumentPermissions.CanDelete(roles, status),
                    DocumentPermissions.GetDeleteDenialReason(roles, status) == PermissionDenialReason.None);
            }
        }
    }
}
