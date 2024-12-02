namespace Aether.Utils;

/// <summary>
///   ObjectPool provides a way to reuse objects rather than constantly allocating and deallocating memory.
///   This helps to minimize GC pressure and improve performance when dealing with a large number of components.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> where T : class, new()
{
  // stack to hold the avaliable objects
  private readonly Stack<T> _pool;

  // tracks the maximum size of the pool to avoid infinite growth
  private readonly int _maxSize;

  /// <summary>
  ///   Initializes the pool with an optional initial size and maximum size.
  /// </summary>
  /// <param name="initialSize">The initial size of the pool.</param>
  /// <param name="maxSize">The maximum size of the pool. Prevents the pool from growing indefinitely.</param>
  public ObjectPool(int initialSize = 10, int maxSize = 100)
  {
    _pool = new Stack<T>(initialSize);
    _maxSize = maxSize;

    for (int i = 0; i < initialSize; i++)
    {
      _pool.Push(new T());
    }
  }

  /// <summary>
  ///   Retrieves an object from the pool or creates a new one if the pool is empty.
  /// </summary>
  /// <returns>A reusable object of type T.</returns>
  public T Rent()
  {
    if (_pool.Count > 0)
    {
      return _pool.Pop();
    }

    if (_pool.Count < _maxSize)
    {
      return new T();
    }

    throw new InvalidOperationException("Object pool exhausted.");
  }

  /// <summary>
  ///   Returns an object to the pool, clearing its state if necessary.
  /// </summary>
  /// <param name="obj">The object to return to the pool.</param>
  public void Return(T obj)
  {
    ArgumentNullException.ThrowIfNull(obj, nameof(obj));

    if (obj is IResettable resettable)
    {
      resettable.Reset();
    }

    if (_pool.Count < _maxSize)
    {
      _pool.Push(obj);
    }
  }

  /// <summary>
  ///   Clears all objects from the pool.
  /// </summary>
  public void Clear()
  {
    _pool.Clear();
  }

  /// <summary>
  ///   Returns the current count of available objects in the pool.
  /// </summary>
  public int Count => _pool.Count;
}

/// <summary>
/// Optional interface for objects that can reset their state when returned to the pool.
/// </summary>
public interface IResettable
{
  void Reset();
}