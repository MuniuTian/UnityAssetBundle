using System;
using System.Collections;
using System.Collections.Generic;

public sealed class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
{
    public Pair(TFirst first, TSecond second)
    {
        First = first;
        Second = second;
    }
    
    public bool Equals(Pair<TFirst, TSecond> other)
    {
        if (other == null)
        {
            return false;
        }
        return EqualityComparer<TFirst>.Default.Equals(First, other.First) &&
               EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
    }

    public override bool Equals(object o)
    {
        return Equals(o as Pair<TFirst, TSecond>);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TFirst>.Default.GetHashCode(First) * 37 +
               EqualityComparer<TSecond>.Default.GetHashCode(Second);
    }

    public TFirst First { get; }
    public TSecond Second { get; }
}