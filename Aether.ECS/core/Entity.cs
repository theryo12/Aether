using System.Runtime.InteropServices;

namespace Aether.Core;

[StructLayout(LayoutKind.Auto, Pack = 1)]
public readonly struct Entity(int id)
{
  /// <summary>
  ///     Unique identifier of the entity.
  ///     Stored in a 32-bit integer to minimize memory overhead.
  /// </summary>
  public readonly int ID = id;

  private readonly ComponentMask _componentMask = new();

  public override string ToString() => $"Entity({nameof(ID)}: {ID})";


  public static bool operator ==(Entity left, Entity right) => left.ID == right.ID && left._componentMask == right._componentMask;
  public static bool operator !=(Entity left, Entity right) => !(left == right);

  public override bool Equals(object? obj)
  {
    if (obj is Entity other)
    {
      return ID == other.ID && _componentMask.Equals(other._componentMask);
    }
    return false;
  }

  public override int GetHashCode() => (ID * 397) ^ _componentMask.GetHashCode();

  /// <summary>
  /// A method to check if entity is "alive" (initialized)
  /// </summary>
  public bool Alive => ID != 0;
}