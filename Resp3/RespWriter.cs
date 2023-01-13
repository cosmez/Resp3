using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Resp3
{
    public enum Resp3Protocol
    {
        Version1 = 1,
        Version2 = 2,
        Version3 = 3
    }

    public class RespWriter : IDisposable
    {
        StreamWriter _stream;
        Resp3Protocol _protocol;
        public RespWriter(StreamWriter stream) 
        {
            _stream = stream;
            _protocol = Resp3Protocol.Version2;
        }

        public void SetRespVersion(Resp3Protocol version)
        {
            _protocol = version;
        }


        //write for .NET built-in types


        /// <summary>
        /// Writes a value as a BulkString
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value) 
        {
            WriteBulkString(value.AsSpan());
        }

        /// <summary>
        /// Writes a value as a BulkString
        /// </summary>
        /// <param name="value"></param>
        public void Write(DateTime value)
        {
            WriteBulkString(value.ToString());
        }

        //different number types
        public void Write(double value)
        {
            if (_protocol == Resp3Protocol.Version3)
                WriteDouble(value);
            else
                WriteBulkString(value.ToString());
        }

        public void Write(float value)
        {
            if (_protocol == Resp3Protocol.Version3)
                WriteDouble(value);
            else
                WriteBulkString(value.ToString());
        }

        public void Write(decimal value)
        {
            if (_protocol == Resp3Protocol.Version3)
                WriteDouble(value);
            else
                WriteBulkString(value.ToString());
        }




        /// <summary>
        /// Writes a value as a BulkString
        /// </summary>
        /// <param name="value"></param>
        public void Write(int value)
        {
            WriteInteger(value);
        }

        /// <summary>
        /// Writes a value as a BulkString
        /// </summary>
        /// <param name="value"></param>
        public void Write(long value)
        {
            WriteInteger(value);
        }

        public void Write<T>(T value)
        {
            switch (value)
            {
                case int i:
                    Write(i);
                    break;
                case long l:
                    Write(l);
                    break;
                case double d:
                    Write(d);
                    break;
                case float f:
                    Write(f);
                    break;
                case string s:
                    Write(s);
                    break;
                case DateTime dt:
                    Write(dt);
                    break;
                case T[] arr:
                    Write(arr);
                    break;
                case object obj:
                    Write(obj.ToString());
                    break;
            }
        }


        public void Write<T>(T[] array)
        {
            StartArray(array.Length);
            foreach (var value in array) Write(value);
        }

        public void Write<T>(IEnumerable<T> enumerable)
        {
            StartArray(enumerable.Count());
            foreach (var value in enumerable) Write(value);
        }


        public void Write<T,K>(IDictionary<T, K> dictonary)
        {
            if (_protocol == Resp3Protocol.Version3)
                StartMap(dictonary.Count);
            else
                StartArray(dictonary.Count * 2);

            foreach (var kvPair in dictonary)
            {
                Write(kvPair.Key);
                Write(kvPair.Value);
            }
        }

        //Direct methods for writing to protocol

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEnd()
        {
            _stream.Write('\r');
            _stream.Write('\n');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartAggregate(char type, int elements)
        {
            _stream.Write(type);
            _stream.Write(elements.ToString());
            WriteEnd();
        }


        public void WriteString(ReadOnlySpan<char> chars)
        {
            _stream.Write('+');
            _stream.Write(chars);
            WriteEnd();
        }

        public void WriteBulkString(ReadOnlySpan<char> chars)
        {
            _stream.Write('$');
            _stream.Write(chars.Length);
            WriteEnd();
            _stream.Write(chars);
            WriteEnd();
        }


        public void WriteInteger(int value) 
        {
            _stream.Write(':');
            _stream.Write(value);
            WriteEnd();
        }

        public void WriteInteger(long value)
        {
            _stream.Write(':');
            _stream.Write(value);
            WriteEnd();
        }

        public void WriteError(ReadOnlySpan<char> chars)
        {
            _stream.Write('-');
            _stream.Write(chars);
            WriteEnd();
        }


        //RESP3
        //Direct methods for writing to protocol

        public void WriteNull()
        {
            _stream.Write('_');
            WriteEnd();
        }

        public void WriteDouble(double value)
        {
            _stream.Write(',');
            _stream.Write(value.ToString());
            WriteEnd();
        }

        public void WriteDouble(float value)
        {
            _stream.Write(',');
            _stream.Write(value.ToString());
            WriteEnd();
        }

        public void WriteDouble(decimal value)
        {
            _stream.Write(',');
            _stream.Write(value.ToString());
            WriteEnd();
        }

        public void WriteInf(char sign = '+')
        {
            _stream.Write(',');
            if (sign == '-') _stream.Write('-');
            _stream.Write("inf");
            WriteEnd();
        }

        public void WriteNaN()
        {
            _stream.Write(',');
            _stream.Write("nan");
            WriteEnd();
        }

        public void WriteBool(bool value)
        {
            _stream.Write('#');
            _stream.Write(value ? 't' : 'f');
            WriteEnd();
        }

        public void WriteBlobError(string error)
        {
            _stream.Write('!');
            _stream.Write(error.Length);
            WriteEnd();
            _stream.Write(error);
            WriteEnd();
        }

        public void WriteVerbatimString(string type, string value)
        {
            _stream.Write('=');
            _stream.Write(value.Length + type.Length + 1);
            WriteEnd();
            _stream.Write(type);
            _stream.Write(':');
            _stream.Write(value);
            WriteEnd();
        }

        public void WriteBigNumber(string number)
        {
            _stream.Write('(');
            _stream.Write(number);
            WriteEnd();
        }

        public void WriteBigumber(BigInteger integer)
        {
            _stream.Write('(');
            _stream.Write(integer.ToString());
            WriteEnd();
        }

        //Agregate Types

        public void StartArray(int length) => StartAggregate('*', length);
        public void StartMap(int length) => StartAggregate('%', length);
        public void StartSet(int length) => StartAggregate('~', length);
        public void StartAttribute(int length) => StartAggregate('|', length);
        public void StartPush(int length) => StartAggregate('>', length);


        public void StartStreamedString(Span<char> marker)
        {
            _stream.Write("$EOF:");
            _stream.Write(marker);
            WriteEnd();
        }

        public void EndStreamedString(Span<char> marker)
        {
            _stream.Write(marker);
        }


        private Random random = new Random();
        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string _lastMarker = string.Empty;
        public string StartStreamedString()
        {
            _lastMarker = RandomString(40);
            _stream.Write("$EOF:");
            WriteEnd();
            return _lastMarker;
        }

        public void EndStreamedString()
        {
            _lastMarker = string.Empty;
            _stream.Write(_lastMarker);
        }


        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}

