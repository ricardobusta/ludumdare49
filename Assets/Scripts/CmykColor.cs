using System;
using UnityEngine;

[Serializable]
public struct CmykColor
{
    public float c;
    public float m;
    public float y;
    public float k;
    public float a;

    public Color ToRgba()
    {
        var invK = 1 - k;
        return new Color((1 - c) * invK, (1 - m) * invK, (1 - y) * invK, a);
    }

    public static bool operator ==(CmykColor a, CmykColor b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(CmykColor a, CmykColor b)
    {
        return !(a == b);
    }

    private bool Equals(CmykColor other)
    {
        return ToRgba().Equals(other.ToRgba());
    }

    public override int GetHashCode()
    {
        return ToRgba().GetHashCode();
    }

    public override string ToString()
    {
        return $"<color=#0ff>C {(int) (c * 100)}%</color> " +
               $"<color=#f0f>M {(int) (m * 100)}%</color> " +
               $"<color=#ff0>Y {(int) (y * 100)}%</color> " +
               $"<color=#000>K {(int) (k * 100)}%</color> " +
               $"<color=#fff>A {(int) (a * 100)}%</color>";
    }
}