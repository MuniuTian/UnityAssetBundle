/**
*
* 类 名： Octets
*
* 作 者： 张文涛
* 时 间： 2017/7/26
* 
* 功 能： 用于存储可扩展字节序列的类型
*         非线程安全
*/

using System;
using System.Text;
using System.Net;

[Serializable]
public class Octets : ICloneable, IComparable<Octets>, IComparable
{
    public Octets()
    {
    }

    public Octets(int size, Encoding enc = null)
    {
        ReserveSpace(size);
        defaultCharset = enc ?? Encoding.UTF8;
    }

    public Octets(Octets o)
    {
        Replace(o);
    }

    public bool Empty()
    {
        return count <= 0;
    }

    public int Size()
    {
        return count;
    }

    public int Capacity()
    {
        return buffer.Length;
    }

    public void Reset()
    {
        count = 0;
        buffer = EMPTY;
    }

    public void Clear()
    {
        count = 0;
    }

    public byte GetByte(int idx)
    {
        return buffer[idx];
    }

    public void SetByte(int idx, byte b)
    {
        buffer[idx] = b;
    }

    public byte[] Array()
    {
        return buffer;
    }

    public byte[] GetBytes()
    {
        if (count <= 0)
        {
            return EMPTY;
        }

        byte[] buf = new byte[count];
        Buffer.BlockCopy(buffer, 0, buf, 0, count);
        return buf;
    }

    public void Shrink()
    {
        Shrink(0);
    }

    /**
     * @param size 期望缩小的空间. 如果比当前数据小,则缩小的当前数据大小;
     */
    public void Shrink(int size)
    {
        if (count <= 0)
        {
            Reset();
            return;
        }

        if (size < count)
        {
            size = count;
        }

        if (size >= buffer.Length)
        {
            return;
        }

        byte[] buf = new byte[size];
        Buffer.BlockCopy(buffer, 0, buf, 0, count);
        buffer = buf;
    }

    public void Reserve(int size)
    {
        if (size > buffer.Length)
        {
            int cap = DEFAULT_SIZE;
            while (size > cap)
            {
                cap <<= 1;
            }

            byte[] buf = new byte[cap];
            if (count > 0)
            {
                Buffer.BlockCopy(buffer, 0, buf, 0, count);
            }

            buffer = buf;
        }
    }

    /**
     * 类似reserve, 但不保证原数据的有效;
     */
    public void ReserveSpace(int size)
    {
        if (size > buffer.Length)
        {
            int cap = DEFAULT_SIZE;
            while (size > cap)
            {
                cap <<= 1;
            }

            buffer = new byte[cap];
        }
    }

    public void Resize(int size)
    {
        if (size < 0)
        {
            size = 0;
        }
        else
        {
            Reserve(size);
        }

        count = size;
    }

    public void Replace(byte[] data, int pos, int size)
    {
        if (size <= 0)
        {
            count = 0;
            return;
        }

        int len = data.Length;
        if (pos < 0)
        {
            pos = 0;
        }

        if (pos >= len)
        {
            count = 0;
            return;
        }

        len -= pos;
        if (size > len)
        {
            size = len;
        }

        ReserveSpace(size);
        Buffer.BlockCopy(data, pos, buffer, 0, size);
        count = size;
    }

    public void Replace(byte[] data)
    {
        Replace(data, 0, data.Length);
    }

    public void Replace(Octets o)
    {
        Replace(o.buffer, 0, o.count);
    }

    public Octets Append(byte b)
    {
        Reserve(count + 1);
        buffer[count++] = b;
        return this;
    }

    public Octets Append(int num)
    {
        num = IPAddress.HostToNetworkOrder(num);
        byte[] arrByte = BitConverter.GetBytes(num);

        Append(arrByte);
        return this;
    }

    public Octets Append(byte[] data, int pos, int size)
    {
        int len = data.Length;
        if (pos < 0)
        {
            pos = 0;
        }

        if (size <= 0 || pos >= len)
        {
            return this;
        }

        len -= pos;
        if (size > len)
        {
            size = len;
        }

        Reserve(count + size);
        Buffer.BlockCopy(data, pos, buffer, count, size);
        count += size;
        return this;
    }

    public Octets Append(byte[] data)
    {
        return Append(data, 0, data.Length);
    }

    public Octets Append(Octets o)
    {
        return Append(o.buffer, 0, o.count);
    }

    //if太多了，折叠一下吧...
    public Octets Insert(int from, byte[] data, int pos, int size)
    {
        if (from < 0) from = 0;
        if (size <= 0) return this;
        if (from >= count) return Append(data, pos, size);
        int len = data.Length;
        if (pos < 0) pos = 0;
        if (pos >= len) return this;
        len -= pos;
        if (size > len) size = len;
        Reserve(count + size);
        Buffer.BlockCopy(buffer, from, buffer, from + size, count - from);
        Buffer.BlockCopy(data, pos, buffer, from, size);
        count += size;
        return this;
    }

    public Octets Insert(int from, byte[] data)
    {
        return Insert(from, data, 0, data.Length);
    }

    public Octets Insert(int from, Octets o)
    {
        return Insert(from, o.buffer, 0, o.count);
    }

    public Octets Erase(int from, int to)
    {
        if (from < 0)
        {
            from = 0;
        }

        if (from >= count || from >= to)
        {
            return this;
        }

        if (to >= count)
        {
            count = from;
        }
        else
        {
            count -= to;
            Buffer.BlockCopy(buffer, to, buffer, from, count);
            count += from;
        }
        return this;
    }

    public Octets EraseFront(int size)
    {
        if (size >= count)
        {
            count = 0;
        }
        else if (size > 0)
        {
            count -= size;
            Buffer.BlockCopy(buffer, size, buffer, 0, count);
        }
        return this;
    }

    public void SetString(string str)
    {
        buffer = defaultCharset.GetBytes(str);
        count = buffer.Length;
    }

    public void SetString(string str, Encoding encoding)
    {
        buffer = encoding.GetBytes(str);
        count = buffer.Length;
    }

    public void SetString(string str, string encoding)
    {
        buffer = Encoding.GetEncoding(encoding).GetBytes(str);
        count = buffer.Length;
    }

    public string GetString()
    {
        return defaultCharset.GetString(buffer, 0, count);
    }

    public string GetString(Encoding encoding)
    {
        return encoding.GetString(buffer, 0, count);
    }

    public string GetString(string encoding)
    {
        return Encoding.GetEncoding(encoding).GetString(buffer, 0, count);
    }

    public virtual object Clone()
    {
        return new Octets(this);
    }

    public override int GetHashCode()
    {
        int result = count;
        if (count <= 32)
        {
            for (int i = 0; i < count; ++i)
            {
                result = 31 * result + buffer[i];
            }
        }
        else
        {
            for (int i = 0; i < 16; ++i)
            {
                result = 31 * result + buffer[i];
            }
            for (int i = count - 16; i < count; ++i)
            {
                result = 31 * result + buffer[i];
            }
        }

        return result;
    }

    public int CompareTo(Octets o)
    {
        if (o == null)
        {
            return 1;
        }

        int n = (count <= o.count ? count : o.count);
        byte[] buf = buffer;
        byte[] data = o.buffer;
        for (int i = 0; i < n; ++i)
        {
            int v = buf[i] - data[i];
            if (v != 0)
            {
                return v;
            }
        }

        return count - o.count;
    }

    public int CompareTo(object o)
    {
        if (!(o is Octets))
        {
            return 1;
        }

        return CompareTo((Octets)o);
    }

    public static byte[] Int2Bytes(int num)
    {
        num = IPAddress.NetworkToHostOrder(num);
        return BitConverter.GetBytes(num);
    }

    private static int Bytes2Int(byte[] msg, int nBegin)
    {
        int num = BitConverter.ToInt32(msg, nBegin);
        return IPAddress.NetworkToHostOrder(num);
    }

    public override bool Equals(object o)
    {
        if (this == o)
        {
            return true;
        }

        if (!(o is Octets))
        {
            return false;
        }

        Octets oct = (Octets)o;
        if (count != oct.count)
        {
            return false;
        }

        byte[] buf = buffer;
        byte[] data = oct.buffer;
        for (int i = 0, n = count; i < n; ++i)
        {
            if (buf[i] != data[i])
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString()
    {
        return "[" + count + '/' + buffer.Length + ']';
    }

    protected int count; // 当前有效的数据缓冲区大小;
    protected byte[] buffer = EMPTY; // 数据缓冲区;
    public const int DEFAULT_SIZE = 16; // 默认的缓冲区;
    protected static Encoding defaultCharset = Encoding.UTF8;
    public static readonly byte[] EMPTY = new byte[0]; // 共享的空缓冲区;
}
