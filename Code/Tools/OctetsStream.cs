using System;

[Serializable]
public class OctetsStream : Octets
{
    public OctetsStream()
    {
    }

    public OctetsStream(int size) : base(size)
    {
    }

    public OctetsStream(Octets o) : base(o)
    {
    }

    public bool EOS()
    {
        return pos >= count;
    }

    public int Position()
    {
        return pos;
    }

    public void SetPosition(int pos)
    {
        this.pos = pos;
    }

    public int Remain()
    {
        return count - pos;
    }

    public override object Clone()
    {
        OctetsStream os = new OctetsStream(this);
        os.pos = pos;
        return os;
    }

    public int ReadInt32()
    {
        int pos_new = pos + 4;
        if (pos_new > count)
        {
            UnityEngine.Debug.LogError("[OctetsStream.ReadInt32()] pos_new > count.");
            return 0;
        }

        byte b0 = buffer[pos];
        byte b1 = buffer[pos + 1];
        byte b2 = buffer[pos + 2];
        byte b3 = buffer[pos + 3];
        pos = pos_new;

        if (BitConverter.IsLittleEndian)
        {
            return b0 + (b1 << 8) + (b2 << 16) + (b3 << 24);
        }

        return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
    }

    public override string ToString()
    {
        return "[" + pos + '/' + count + '/' + buffer.Length + ']';
    }

    protected int pos; // 当前的读写位置;
}