using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Resp3
{
    public class RespReader
    {
        StreamReader _stream;
        Resp3Protocol _protocol;
        public RespReader(StreamReader stream)
        {
            _stream = stream;
            _protocol = Resp3Protocol.Version2;
        }

        public void SetRespVersion(Resp3Protocol version)
        {
            _protocol = version;
        }
    }
}
