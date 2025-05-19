using System.Linq.Expressions;
using System.Reflection;

namespace MineSweeper.Tests.ViewModels;

internal static class TestUtility
{
    internal static void SetPrivateProperty<T, TProp>(this T obj, Expression<Func<T, TProp>> propertyAccessor,
        TProp value)
    {
        if (propertyAccessor.Body is not MemberExpression memberExpr)
            throw new ArgumentException("The property accessor must be a member expression.", nameof(propertyAccessor));

        var propertyInfo = memberExpr.Member as PropertyInfo;

        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(obj, value);
            return;
        }

        // Try to find a backing field (for auto-properties or manual fields)
        var fieldNameCandidates = new[]
        {
            $"<{memberExpr.Member.Name}>k__BackingField", // auto-property backing field
            $"_{char.ToLowerInvariant(memberExpr.Member.Name[0])}{memberExpr.Member.Name[1..]}", // _camelCase
            memberExpr.Member.Name // direct field
        };

        var fieldInfo = fieldNameCandidates
            .Select(name =>
                typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            .FirstOrDefault(f => f != null);

        if (fieldInfo != null)
        {
            fieldInfo.SetValue(obj, value);
            return;
        }

        throw new InvalidOperationException(
            $"Could not find a settable property or field for '{memberExpr.Member.Name}'.");
    }
}