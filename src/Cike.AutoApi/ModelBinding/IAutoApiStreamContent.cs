﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cike.AutoWebApi.ModelBinding
{
    public interface IAutoApiStreamContent : IDisposable
    {
        string FileName { get; }

        string ContentType { get; }

        long? ContentLength { get; }

        Stream GetStream();
    }
}
