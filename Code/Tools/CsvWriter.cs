using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CsvWriter : IDisposable
{
    public CsvWriter(string path)
    {
        _writer = new StreamWriter(path);
        _writer.NewLine = os.linesep;
    }

    public void Dispose()
    {
        _writer.Close();
    }

    public void WriteRow(List<string> items)
    {
        if (null != items && items.Count > 0)
        {
            _WriteItem(items[0]);

            int itemCount = items.Count;
            for (int i = 1; i < itemCount; ++i)
            {
                _writer.Write(',');
                _WriteItem(items[i]);
            }
        }

        _writer.WriteLine();
        _writer.Flush();
    }

    private void _WriteItem(string item)
    {
        if (item.IndexOfAny(_separateChars) == -1)
        {
            _writer.Write(item);
        }
        else
        {
            _writer.Write('"');
            _writer.Write(item.Replace("\"", "\"\""));
            _writer.Write('"');
        }
    }

    private readonly StreamWriter _writer;
    private char[] _separateChars = new char[] { '"', ',' };
}