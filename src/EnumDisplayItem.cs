namespace Medoz.CatChast;

public record EnumDisplayItem<T> (T Value, string DisplayName) where T : Enum;