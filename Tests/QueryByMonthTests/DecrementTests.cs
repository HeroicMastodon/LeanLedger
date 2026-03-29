namespace Tests.QueryByMonthTests;

using LeanLedgerServer.Common;

public class DecrementTests {
    [TestCase]
    public void GivenNoArgument_ThenReturnLastMonth() {
        var query = new QueryByMonth(2, 2024);
        var decremented = query.Decrement();

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(1));
            Assert.That(decremented.Year, Is.EqualTo(2024));
        });
    }

    [TestCase]
    public void GivenNoArgument_WhenQueryIsJanuary_ThenReturnLastYear() {
        var query = new QueryByMonth(1, 2024);
        var decremented = query.Decrement();

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(12));
            Assert.That(decremented.Year, Is.EqualTo(2023));
        });
    }

    [TestCase]
    public void GivenMonths_WhenQueryDecrementDoesNotCrossBoundary_ThenReturnDecrementedMonth() {
        var query = new QueryByMonth(4, 2024);
        var decremented = query.Decrement(2);

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(2));
            Assert.That(decremented.Year, Is.EqualTo(2024));
        });
    }

    [TestCase]
    public void GivenMonths_WhenQueryDecrementCrossesBoundary_ThenReturnDecrementedMonthAndYear() {
        var query = new QueryByMonth(2, 2024);
        var decremented = query.Decrement(3);

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(11));
            Assert.That(decremented.Year, Is.EqualTo(2023));
        });
    }

    [TestCase]
    public void GivenMonths_WhenQueryDecrementExactlyCrossesBoundary_ThenReturnDecemberOfLastYear() {
        var query = new QueryByMonth(3, 2024);
        var decremented = query.Decrement(3);

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(12));
            Assert.That(decremented.Year, Is.EqualTo(2023));
        });
    }

    [TestCase]
    public void GivenMonths_DecrementMoreThan12Months_ThenReturnCorrectMonthAndYear() {
        var query = new QueryByMonth(5, 2024);
        var decremented = query.Decrement(14);

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(3));
            Assert.That(decremented.Year, Is.EqualTo(2023));
        });
    }
    [TestCase]
    public void GivenMonths_DecrementMoreThan24Months_ThenReturnCorrectMonthAndYear() {
        var query = new QueryByMonth(5, 2024);
        var decremented = query.Decrement(26);

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(3));
            Assert.That(decremented.Year, Is.EqualTo(2022));
        });
    }

    [TestCase]
    public void GivenMonths_WhenMonthIsDecember_ThenDecrementByOneMonthReturnsNovember() {
        var query = new QueryByMonth(12, 2024);
        var decremented = query.Decrement();

        Assert.Multiple(() => {
            Assert.That(decremented.Month, Is.EqualTo(11));
            Assert.That(decremented.Year, Is.EqualTo(2024));
        });
    }
}
