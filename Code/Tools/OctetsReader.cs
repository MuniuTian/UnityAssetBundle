using System;
using System.Text;
using System.IO;
using UnityEngine;

public class OctetsReader : BinaryReader, IOctetsReader
{
    public OctetsReader(Stream stream) : base(stream, os.UTF8)
    {
        _stream = stream;

        if (null == _stream)
        {
            throw new IOException("Stream is invalid");
        }
    }

    public override string ReadString()
    {
        return base.ReadString();
    }

    public Vector2 ReadVector2()
    {
        _ReadFloatBuffer(8);
        return new Vector2(_floatBuffer[0], _floatBuffer[1]);
    }

    public Vector3 ReadVector3()
    {
        _ReadFloatBuffer(12);
        return new Vector3(_floatBuffer[0], _floatBuffer[1], _floatBuffer[2]);
    }

    public Vector4 ReadVector4()
    {
        _ReadFloatBuffer(16);
        return new Vector4(_floatBuffer[0], _floatBuffer[1], _floatBuffer[2], _floatBuffer[3]);
    }

    public Quaternion ReadQuaternion()
    {
        _ReadFloatBuffer(16);
        return new Quaternion(_floatBuffer[0], _floatBuffer[1], _floatBuffer[2], _floatBuffer[3]);
    }

    private void _ReadFloatBuffer(int count)
    {
        _stream.Read(_byteBuffer, 0, count);
        Buffer.BlockCopy(_byteBuffer, 0, _floatBuffer, 0, count);
    }

    public Color ReadColor()
    {
        Color color = ColorTools.FromInt32(ReadInt32());
        return color;
    }

    public override ushort ReadUInt16()
    {
        return base.ReadUInt16();
    }

    public override int ReadInt32()
    {
        return base.ReadInt32();
    }

    public override uint ReadUInt32()
    {
        return base.ReadUInt32();
    }

    private readonly Stream _stream;

    private readonly float[] _floatBuffer = new float[4];
    private readonly byte[] _byteBuffer = new byte[16];

    private int _builderCount;
}