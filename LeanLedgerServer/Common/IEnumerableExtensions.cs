namespace LeanLedgerServer.Common;

public static class IEnumerableExtensions {
    public static IEnumerable<(T Value, int Index)> Enumerate<T>(this IEnumerable<T> values) {
        var i = 0;
        foreach (var value in values) {
            yield return (value, i++);
        }
    }
}
