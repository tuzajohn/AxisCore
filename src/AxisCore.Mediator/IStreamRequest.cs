namespace AxisCore.Mediator;

/// <summary>
/// Marker interface to represent a request that streams responses.
/// </summary>
/// <typeparam name="TResponse">Stream item type</typeparam>
public interface IStreamRequest<out TResponse>
{
}
