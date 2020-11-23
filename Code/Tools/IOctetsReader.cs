using System.IO;
using UnityEngine;

public interface IOctetsReader
{
    Vector2 ReadVector2();
    Vector3 ReadVector3();
    Vector4 ReadVector4();
    Color ReadColor();

    bool ReadBoolean();
    byte ReadByte();
    double ReadDouble();
    short ReadInt16();
    int ReadInt32();
    long ReadInt64();

    sbyte ReadSByte();
    float ReadSingle();
    string ReadString();

    ushort ReadUInt16();
    uint ReadUInt32();
    ulong ReadUInt64();
}