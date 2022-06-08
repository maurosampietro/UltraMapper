namespace UltraMapper
{
    /// <summary>
    /// This conversions would be figured out and generated at runtime every time.
    /// We can improve performance by setting them up as converters at compile time.
    /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions
    /// </summary>
    public class BuiltInConverters
    {
        public void AddPrimitiveTypeToItself( Configuration config )
        {
            config.MapTypes<bool, bool>( s => s );
            config.MapTypes<byte, byte>( s => s );
            config.MapTypes<sbyte, sbyte>( s => s );
            config.MapTypes<char, char>( s => s );
            config.MapTypes<decimal, decimal>( s => s );
            config.MapTypes<double, double>( s => s );
            config.MapTypes<float, float>( s => s );
            config.MapTypes<int, int>( s => s );
            config.MapTypes<uint, uint>( s => s );
            config.MapTypes<long, long>( s => s );
            config.MapTypes<ulong, ulong>( s => s );
            config.MapTypes<short, short>( s => s );
            config.MapTypes<ushort, ushort>( s => s );
            config.MapTypes<string, string>( s => s );
        }

        public void AddImplicitNumeriConverters( Configuration config )
        {
            config.MapTypes<sbyte, short>( s => s );
            config.MapTypes<sbyte, int>( s => s );
            config.MapTypes<sbyte, long>( s => s );
            config.MapTypes<sbyte, float>( s => s );
            config.MapTypes<sbyte, double>( s => s );
            config.MapTypes<sbyte, decimal>( s => s );

            config.MapTypes<byte, short>( s => s );
            config.MapTypes<byte, ushort>( s => s );
            config.MapTypes<byte, int>( s => s );
            config.MapTypes<byte, uint>( s => s );
            config.MapTypes<byte, long>( s => s );
            config.MapTypes<byte, ulong>( s => s );
            config.MapTypes<byte, float>( s => s );
            config.MapTypes<byte, double>( s => s );
            config.MapTypes<byte, decimal>( s => s );

            config.MapTypes<short, int>( s => s );
            config.MapTypes<short, long>( s => s );
            config.MapTypes<short, float>( s => s );
            config.MapTypes<short, double>( s => s );
            config.MapTypes<short, decimal>( s => s );

            config.MapTypes<ushort, int>( s => s );
            config.MapTypes<ushort, uint>( s => s );
            config.MapTypes<ushort, long>( s => s );
            config.MapTypes<ushort, ulong>( s => s );
            config.MapTypes<ushort, float>( s => s );
            config.MapTypes<ushort, double>( s => s );
            config.MapTypes<ushort, decimal>( s => s );

            config.MapTypes<int, long>( s => s );
            config.MapTypes<int, float>( s => s );
            config.MapTypes<int, double>( s => s );
            config.MapTypes<int, decimal>( s => s );

            config.MapTypes<uint, long>( s => s );
            config.MapTypes<uint, ulong>( s => s );
            config.MapTypes<uint, float>( s => s );
            config.MapTypes<uint, double>( s => s );
            config.MapTypes<uint, decimal>( s => s );

            config.MapTypes<long, float>( s => s );
            config.MapTypes<long, double>( s => s );
            config.MapTypes<long, decimal>( s => s );

            config.MapTypes<ulong, float>( s => s );
            config.MapTypes<ulong, double>( s => s );
            config.MapTypes<ulong, decimal>( s => s );

            config.MapTypes<float, double>( s => s );
        }

        public void AddExplicitNumericConverters( Configuration config )
        {
            config.MapTypes<sbyte, byte>( s => (byte)s );
            config.MapTypes<sbyte, ushort>( s => (ushort)s );
            config.MapTypes<sbyte, uint>( s => (uint)s );
            config.MapTypes<sbyte, ulong>( s => (ulong)s );

            config.MapTypes<byte, sbyte>( s => (sbyte)s );

            config.MapTypes<short, sbyte>( s => (sbyte)s );
            config.MapTypes<short, byte>( s => (byte)s );
            config.MapTypes<short, ushort>( s => (ushort)s );
            config.MapTypes<short, uint>( s => (uint)s );
            config.MapTypes<short, ulong>( s => (ulong)s );

            config.MapTypes<ushort, sbyte>( s => (sbyte)s );
            config.MapTypes<ushort, byte>( s => (byte)s );
            config.MapTypes<ushort, short>( s => (short)s );

            config.MapTypes<int, sbyte>( s => (sbyte)s );
            config.MapTypes<int, byte>( s => (byte)s );
            config.MapTypes<int, short>( s => (short)s );
            config.MapTypes<int, ushort>( s => (ushort)s );
            config.MapTypes<int, uint>( s => (uint)s );
            config.MapTypes<int, ulong>( s => (ulong)s );

            config.MapTypes<uint, sbyte>( s => (sbyte)s );
            config.MapTypes<uint, byte>( s => (byte)s );
            config.MapTypes<uint, short>( s => (short)s );
            config.MapTypes<uint, ushort>( s => (ushort)s );
            config.MapTypes<uint, int>( s => (int)s );

            config.MapTypes<long, sbyte>( s => (sbyte)s );
            config.MapTypes<long, byte>( s => (byte)s );
            config.MapTypes<long, short>( s => (short)s );
            config.MapTypes<long, ushort>( s => (ushort)s );
            config.MapTypes<long, int>( s => (int)s );
            config.MapTypes<long, uint>( s => (uint)s );
            config.MapTypes<long, ulong>( s => (ulong)s );

            config.MapTypes<ulong, sbyte>( s => (sbyte)s );
            config.MapTypes<ulong, byte>( s => (byte)s );
            config.MapTypes<ulong, short>( s => (short)s );
            config.MapTypes<ulong, ushort>( s => (ushort)s );
            config.MapTypes<ulong, int>( s => (int)s );
            config.MapTypes<ulong, uint>( s => (uint)s );
            config.MapTypes<ulong, long>( s => (long)s );

            config.MapTypes<float, sbyte>( s => (sbyte)s );
            config.MapTypes<float, byte>( s => (byte)s );
            config.MapTypes<float, short>( s => (short)s );
            config.MapTypes<float, ushort>( s => (ushort)s );
            config.MapTypes<float, int>( s => (int)s );
            config.MapTypes<float, uint>( s => (uint)s );
            config.MapTypes<float, long>( s => (long)s );
            config.MapTypes<float, ulong>( s => (ulong)s );
            config.MapTypes<float, decimal>( s => (decimal)s );

            config.MapTypes<double, sbyte>( s => (sbyte)s );
            config.MapTypes<double, byte>( s => (byte)s );
            config.MapTypes<double, short>( s => (short)s );
            config.MapTypes<double, ushort>( s => (ushort)s );
            config.MapTypes<double, int>( s => (int)s );
            config.MapTypes<double, uint>( s => (uint)s );
            config.MapTypes<double, long>( s => (long)s );
            config.MapTypes<double, ulong>( s => (ulong)s );
            config.MapTypes<double, float>( s => (float)s );
            config.MapTypes<double, decimal>( s => (decimal)s );

            config.MapTypes<decimal, sbyte>( s => (sbyte)s );
            config.MapTypes<decimal, byte>( s => (byte)s );
            config.MapTypes<decimal, short>( s => (short)s );
            config.MapTypes<decimal, ushort>( s => (ushort)s );
            config.MapTypes<decimal, int>( s => (int)s );
            config.MapTypes<decimal, uint>( s => (uint)s );
            config.MapTypes<decimal, long>( s => (long)s );
            config.MapTypes<decimal, ulong>( s => (ulong)s );
            config.MapTypes<decimal, float>( s => (float)s );
            config.MapTypes<decimal, double>( s => (double)s );
        }

        public void AddStringToPrimitiveTypeConverters( Configuration config )
        {
            config.MapTypes<string, bool>( s => bool.Parse( s ) );
            config.MapTypes<string, byte>( s => byte.Parse( s.ToString() ) );
            config.MapTypes<string, sbyte>( s => sbyte.Parse( s.ToString() ) );
            config.MapTypes<string, char>( s => char.Parse( s.ToString() ) );
            config.MapTypes<string, decimal>( s => decimal.Parse( s.ToString() ) );
            config.MapTypes<string, double>( s => double.Parse( s.ToString() ) );
            config.MapTypes<string, float>( s => float.Parse( s.ToString() ) );
            config.MapTypes<string, int>( s => int.Parse( s.ToString() ) );
            config.MapTypes<string, uint>( s => uint.Parse( s.ToString() ) );
            config.MapTypes<string, long>( s => long.Parse( s.ToString() ) );
            config.MapTypes<string, ulong>( s => ulong.Parse( s.ToString() ) );
            config.MapTypes<string, short>( s => short.Parse( s.ToString() ) );
            config.MapTypes<string, ushort>( s => ushort.Parse( s.ToString() ) );
        }

        public void AddPrimitiveTypeToStringConverters( Configuration config )
        {
            config.MapTypes<bool, string>( s => s.ToString() );
            config.MapTypes<byte, string>( s => s.ToString() );
            config.MapTypes<sbyte, string>( s => s.ToString() );
            config.MapTypes<char, string>( s => s.ToString() );
            config.MapTypes<decimal, string>( s => s.ToString() );
            config.MapTypes<double, string>( s => s.ToString() );
            config.MapTypes<float, string>( s => s.ToString() );
            config.MapTypes<int, string>( s => s.ToString() );
            config.MapTypes<uint, string>( s => s.ToString() );
            config.MapTypes<long, string>( s => s.ToString() );
            config.MapTypes<ulong, string>( s => s.ToString() );
            config.MapTypes<short, string>( s => s.ToString() );
            config.MapTypes<ushort, string>( s => s.ToString() );
        }
    }
}