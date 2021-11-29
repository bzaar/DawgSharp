using System;
using System.Collections.Generic;
using System.IO;

namespace DawgSharp
{
    static class BuiltinTypeIO
    {
        static readonly Dictionary<Type, object> Writers = new()
        {
            {typeof (bool),    new Action<BinaryWriter, bool>((r, payload) => r.Write(payload))},
            {typeof (int),     new Action<BinaryWriter, int>((r, payload) => r.Write(payload))},
            {typeof (uint),    new Action<BinaryWriter, uint>((r, payload) => r.Write(payload))},
            {typeof (long),    new Action<BinaryWriter, long>((r, payload) => r.Write(payload))},
            {typeof (ulong),   new Action<BinaryWriter, ulong>((r, payload) => r.Write(payload))},
            {typeof (byte),    new Action<BinaryWriter, byte>((r, payload) => r.Write(payload))},
            {typeof (sbyte),   new Action<BinaryWriter, sbyte>((r, payload) => r.Write(payload))},
            {typeof (short),   new Action<BinaryWriter, short>((r, payload) => r.Write(payload))},
            {typeof (ushort),  new Action<BinaryWriter, ushort>((r, payload) => r.Write(payload))},
            {typeof (string),  new Action<BinaryWriter, string>((r, payload) => r.Write(payload))},
            {typeof (char),    new Action<BinaryWriter, char>((r, payload) => r.Write(payload))},
            {typeof (double),  new Action<BinaryWriter, double>((r, payload) => r.Write(payload))},
            {typeof (float),   new Action<BinaryWriter, float>((r, payload) => r.Write(payload))},
            {typeof (decimal), new Action<BinaryWriter, decimal>((r, payload) => r.Write(payload))},
        };

        static readonly Dictionary<Type, object> Readers = new()
        {
            {typeof (bool),    new Func<BinaryReader, bool>(r => r.ReadBoolean())},
            {typeof (int),     new Func<BinaryReader, int>(r => r.ReadInt32())},
            {typeof (uint),    new Func<BinaryReader, uint>(r => r.ReadUInt32())},
            {typeof (long),    new Func<BinaryReader, long>(r => r.ReadInt64())},
            {typeof (ulong),   new Func<BinaryReader, ulong>(r => r.ReadUInt64())},
            {typeof (byte),    new Func<BinaryReader, byte>(r => r.ReadByte())},
            {typeof (sbyte),   new Func<BinaryReader, sbyte>(r => r.ReadSByte())},
            {typeof (short),   new Func<BinaryReader, short>(r => r.ReadInt16())},
            {typeof (ushort),  new Func<BinaryReader, ushort>(r => r.ReadUInt16())},
            {typeof (string),  new Func<BinaryReader, string>(r => r.ReadString())},
            {typeof (char),    new Func<BinaryReader, char>(r => r.ReadChar())},
            {typeof (double),  new Func<BinaryReader, double>(r => r.ReadDouble())},
            {typeof (float),   new Func<BinaryReader, float>(r => r.ReadSingle())},
            {typeof (decimal), new Func<BinaryReader, decimal>(r => r.ReadDecimal())},
        };

        public static Func<BinaryReader, T> TryGetReader<T>()
        {
            Readers.TryGetValue(typeof(T), out object reader);

            return (Func<BinaryReader, T>) reader;
        }

        public static Action<BinaryWriter, T> TryGetWriter<T>()
        {
            Writers.TryGetValue(typeof(T), out object writer);

            return (Action<BinaryWriter, T>) writer;
        }

        public static Func<BinaryReader, T> GetReader<T>() => 
            TryGetReader<T>()
                ?? throw new Exception(nameof(T) + " is not a built-in type.");

        public static Action<BinaryWriter, T> GetWriter<T>() =>
            TryGetWriter<T>()
                ?? throw new Exception(nameof(T) + " is not a built-in type.");
    }
}