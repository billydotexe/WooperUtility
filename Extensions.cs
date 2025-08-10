using Microsoft.Extensions.Options;

namespace WooperUtility;

public static class WooperExtensions
{
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider)
        where T : class
    {
        var o = serviceProvider.GetService<IOptions<T>>() ?? throw new ArgumentNullException(nameof(T));
        return o.Value;
    }
}
