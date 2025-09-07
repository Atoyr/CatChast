using System.Reflection;

namespace Medoz.CatChast.Data;

public static class ConfigExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, DynamicConfig> config, string key, out T? value) where T : IConfig
    {
        if (config.TryGetValue(key, out var dynamicConfig))
        {
            try
            {
                // DynamicConfigを受け取るコンストラクタを探す
                var constructor = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(DynamicConfig) },
                    null);

                if (constructor != null)
                {
                    value = (T)constructor.Invoke(new object[] { dynamicConfig });
                    return true;
                }

                // DynamicConfigを受け取るコンストラクタが見つからない場合のエラーメッセージ
                throw new InvalidOperationException(
                    $"Type '{typeof(T).Name}' does not have a constructor that accepts DynamicConfig parameter.");
            }
            catch (Exception ex)
            {
                // コンストラクタの実行中にエラーが発生した場合
                throw new InvalidOperationException(
                    $"Failed to create instance of '{typeof(T).Name}' with DynamicConfig parameter.", ex);
            }
        }

        value = default;
        return false;
    }
}
