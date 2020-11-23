using System;
using System.IO;
using UnityEngine;

public class OctetsWriter : BinaryWriter, IOctetsWriter
{
    public OctetsWriter(Stream stream) : base(stream, os.UTF8)
    {

    }

    public void Write(Vector2 v)
    {
        _floatBuffer[0] = v.x;
        _floatBuffer[1] = v.y;

        _WriteFloatBuffer(8);
    }

    public void Write(Vector3 v)
    {
        _floatBuffer[0] = v.x;
        _floatBuffer[1] = v.y;
        _floatBuffer[2] = v.z;

        _WriteFloatBuffer(12);
    }

    public void Write(Vector4 v)
    {
        _floatBuffer[0] = v.x;
        _floatBuffer[1] = v.y;
        _floatBuffer[2] = v.z;
        _floatBuffer[3] = v.w;

        _WriteFloatBuffer(16);
    }

    public void Write(Quaternion v)
    {
        _floatBuffer[0] = v.x;
        _floatBuffer[1] = v.y;
        _floatBuffer[2] = v.z;
        _floatBuffer[3] = v.w;

        _WriteFloatBuffer(16);
    }

    public void Write(Color color)
    {
        Write(ColorTools.ToInt32(color));
    }

    public override void Write(string s)
    {
        s = s ?? string.Empty;
        base.Write(s);
    }

    private void _WriteFloatBuffer(int count)
    {
        Buffer.BlockCopy(_floatBuffer, 0, _byteBuffer, 0, count);
        base.Write(_byteBuffer, 0, count);
    }

    public override void Write(ushort val)
    {
        base.Write(val);
    }

    public override void Write(int val)
    {
        base.Write(val);
    }

    public override void Write(uint val)
    {
        base.Write(val);
    }

    private readonly float[] _floatBuffer = new float[4];
    private readonly byte[] _byteBuffer = new byte[16];
}