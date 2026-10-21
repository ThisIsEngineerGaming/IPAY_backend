using IPAY.Application.DTOs.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Shop;

[TestFixture]
public sealed class OrderStatusHistoryDtoTests
{
    [Test]
    public void Properties_RoundTripAssignedValues()
    {
        var changedAt = DateTime.UtcNow;
        var dto = new OrderStatusHistoryDto
        {
            Id = 1, OrderId = 12, OldStatus = "Pending", NewStatus = "Paid",
            ChangedAt = changedAt, ChangedByUserId = 4, Comment = "Payment confirmed"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.OrderId, Is.EqualTo(12));
            Assert.That(dto.OldStatus, Is.EqualTo("Pending"));
            Assert.That(dto.NewStatus, Is.EqualTo("Paid"));
            Assert.That(dto.ChangedAt, Is.EqualTo(changedAt));
            Assert.That(dto.ChangedByUserId, Is.EqualTo(4));
            Assert.That(dto.Comment, Is.EqualTo("Payment confirmed"));
        });
    }

    [Test]
    public void ChangedByUserIdAndComment_CanBeNull()
    {
        var dto = new OrderStatusHistoryDto { ChangedByUserId = null, Comment = null };
        Assert.Multiple(() =>
        {
            Assert.That(dto.ChangedByUserId, Is.Null);
            Assert.That(dto.Comment, Is.Null);
        });
    }
}
