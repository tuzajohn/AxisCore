namespace Conduit.Mediator;

/// <summary>
/// Represents a void type, since <see cref="System.Void"/> is not a valid return type in C#.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>
    /// Default and only value of the <see cref="Unit"/> type.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// Task from unit.
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    /// <summary>
    /// ValueTask from unit.
    /// </summary>
    public static readonly ValueTask<Unit> ValueTask = new(Value);

    /// <inheritdoc />
    public int CompareTo(Unit other) => 0;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <inheritdoc />
    public override string ToString() => "()";
}
