using System.Formats.Tar;
using System.Text;

namespace Aether.Tests.Utils;

internal ref struct ValueStringBuilder
{
    private readonly Span<char> _buffer;
    private StringBuilder? _overflow;
    private int _position;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        _buffer = initialBuffer;
        _overflow = null;
        _position = 0;
    }

    public void Append(string value)
    {
        if (value.TryCopyTo(_buffer.Slice(_position)))
        {
            _position += value.Length;
        }
        else
        {
            _overflow ??= new StringBuilder();
            _overflow.Append(value);
        }
    }

    public void Append(char value)
    {
        if (_position < _buffer.Length)
        {
            _buffer[_position++] = value;
        }
        else
        {
            _overflow ??= new StringBuilder();
            _overflow.Append(value);
        }
    }

    public override string ToString()
    {
        if (_overflow != null)
        {
            return _overflow.Insert(0, _buffer[.._position].ToString()).ToString();
        }
        return _buffer[.._position].ToString();
    }
}