using Mue2MueDee.Application.Common.Exceptions;
using Xunit;

namespace Mue2MueDee.Application.Tests;

public class AppExceptionTests
{
    [Fact]
    public void AppException_ShouldRetainDynamicMessageAndStatusCode()
    {
        // Arrange
        var customMessage = "สินค้าชิ้นนี้มีสถานะเป็น SOLD แล้ว ไม่สามารถเปลี่ยนราคาได้";
        var expectedStatus = 400;

        // Act
        var ex = new AppException(customMessage, expectedStatus);

        // Assert
        Assert.Equal(customMessage, ex.Message);
        Assert.Equal(expectedStatus, ex.StatusCode);
    }

    [Fact]
    public void AppException_NotFoundFactory_ShouldHave404Status()
    {
        // Arrange
        var msg = "ไม่พบ Serial Number กล้องในระบบ";

        // Act
        var ex = AppException.NotFound(msg);

        // Assert
        Assert.Equal(msg, ex.Message);
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public void AppException_ForbiddenFactory_ShouldHave403Status()
    {
        // Arrange
        var msg = "พนักงานตำแหน่ง Staff ไม่ได้รับอนุญาตให้ดูผลกำไร";

        // Act
        var ex = AppException.Forbidden(msg);

        // Assert
        Assert.Equal(msg, ex.Message);
        Assert.Equal(403, ex.StatusCode);
    }
}
