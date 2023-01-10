using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NET.AutoWebApi.ModelBinding
{
    public interface IAutoApiStreamContent
    {
        string FileName { get; }

        string ContentType { get; }

        long? ContentLength { get; }

        Stream GetStream();
    }
}
