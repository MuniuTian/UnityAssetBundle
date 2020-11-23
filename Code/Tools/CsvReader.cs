using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CsvReader : IDisposable, IEnumerable<List<string>>
{
    public CsvReader(Stream stream)
    {
        _reader = new StreamReader(stream);
    }

    public CsvReader(string path)
    {
        _reader = new StreamReader(path);
    }

    public void Dispose()
    {
        _reader.Close();
    }

    public IEnumerator<List<string>> GetEnumerator()
    {
        var line = string.Empty;
        var items = new List<string>();
        while (!_Eof(ref line))
        {
            int pos = 0;
            items.Clear();
            while (pos < line.Length)
            {
                if (line[pos] == '"')
                {
                    _HandleQuote(ref pos, ref items, ref line);
                }
                else
                {
                    _HandleNormal(ref pos, ref items, ref line);
                }

                _FindNextComma(ref pos, ref line);
            }
            yield return items;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private bool _Eof(ref string line)
    {
        while (null != (line = _reader.ReadLine()))
        {
            if ('#' != line[0])
            {
                break;
            }
        }

        return null == line;
    }

    private void _HandleQuote(ref int pos, ref List<string> items, ref string line)
    {
        ++pos;
        int start = pos;
        int quotedNums = 1;
        while (pos < line.Length)
        {
            if (line[pos] == '"')
            {
                ++quotedNums;

                if ((pos + 1 >= line.Length) || (line[pos + 1] != '"' && quotedNums % 2 == 0))
                {
                    break;
                }
            }
            ++pos;
        }

        if (quotedNums % 2 != 0)
        {
            UnityEngine.Debug.LogError("[CsvReader] The quoted nums is error!");
        }

        string item = line.Substring(start, pos - start);
        item = item.Replace("\"\"", "\"");
        items.Add(item);
    }

    private void _HandleNormal(ref int pos, ref List<string> items, ref string line)
    {
        int start = pos;
        while (pos < line.Length && line[pos] != ',')
        {
            ++pos;
        }
        string item = line.Substring(start, pos - start);
        items.Add(item);
    }

    private void _FindNextComma(ref int pos, ref string line)
    {
        while (pos < line.Length && line[pos] != ',')
        {
            ++pos;
        }

        if (pos < line.Length)
        {
            ++pos;
        }
    }

    private readonly StreamReader _reader;
}