
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

//// <summary>
///</summary>




namespace lain.protocol
{

    internal sealed class ByteReader
    {
        
        private readonly byte[] _bytes;
        internal int Position { get; private set; }
        internal ByteReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        internal byte Current => Position < _bytes.Length ? _bytes[Position] : throw new EndOfStreamException();

        internal void MoveNext() {Position++;}


        internal int Mark() => Position;
        internal byte[] Slice (int start, int end)
        {
            int length = end - start;
            byte[] slice = new byte[length];
            Array.Copy(_bytes, start, slice, 0, length);
            return slice;

        }



    }


    internal class Parser
    {


        private static readonly Dictionary<string, byte[]> ByteMap = new() { 
            
            {"DictStart", System.Text.Encoding.UTF8.GetBytes("d")},
            {"DictEnd", System.Text.Encoding.UTF8.GetBytes("e")},
            {"ListStart", System.Text.Encoding.UTF8.GetBytes("l")},
            {"ListEnd", System.Text.Encoding.UTF8.GetBytes("e")},
            {"IntStart", System.Text.Encoding.UTF8.GetBytes("i")},
            {"IntEnd", System.Text.Encoding.UTF8.GetBytes("e")},
            {"StringSeparator", System.Text.Encoding.UTF8.GetBytes(":")}

            };



        internal static object Parse(byte[] bytes)
        {

            var reader = new ByteReader(bytes); 
            return ParseNext(reader);

        }



        internal static object ParseNext(ByteReader reader)
        {
            if (reader.Current == ByteMap["DictStart"][0])
                return ParseDict(reader);

            if (reader.Current == ByteMap["ListStart"][0])
                return ParseList(reader);
            if (reader.Current == ByteMap["IntStart"][0])
                return ParseInt(reader);
       

            return ParseString(reader);






        }






        #region Parse Helper Methods



        private static Dictionary<string, object> ParseDict(ByteReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            // Move past the 'd' character
            reader.MoveNext();
            while (reader.Current != ByteMap["DictEnd"][0])
            {

                byte[] keyByte = ParseString(reader);
                string key = Encoding.UTF8.GetString(keyByte);

                if (key == "info")
                {
                    int start = reader.Mark();
                    object value = ParseNext(reader);
                    int end = reader.Mark();

                    dict[key] = value;
                    dict["_raw_info"] = reader.Slice(start, end);
                }
                else
                {
                    dict[key] = ParseNext(reader);

                }


            }
            // Move past the 'e' character
            reader.MoveNext();
            return dict;
        }



        private static byte[] ParseString(ByteReader reader)
        {



            StringBuilder lengthBuilder = new StringBuilder();
            while (true)
            {
                if (reader.Current == ByteMap["StringSeparator"][0])
                    break;

                lengthBuilder.Append((char)reader.Current);
                reader.MoveNext();
            }
            // Move past the ':' character
            reader.MoveNext();
            int length = int.Parse(lengthBuilder.ToString());
            byte[] strBytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                strBytes[i] = reader.Current;
                reader.MoveNext();
            }
            return strBytes;
        }   


        private static long ParseInt(ByteReader reader)
        {
            // Move past the 'i' character
            reader.MoveNext();
            StringBuilder intBuilder = new StringBuilder();
            while (reader.Current != ByteMap["IntEnd"][0])
            {
                intBuilder.Append((char)reader.Current);
                reader.MoveNext();
            }
            // Move past the 'e' character
            reader.MoveNext();
            return long.Parse(intBuilder.ToString());
        }


        private static List<object> ParseList(ByteReader reader)
        {
            List<object> list = new List<object>();
            // Move past the 'l' character
            reader.MoveNext();
            while (reader.Current != ByteMap["ListEnd"][0])
            {
                object value = ParseNext(reader);
                list.Add(value);
            }
            // Move past the 'e' character
            reader.MoveNext();
            return list;
        }


        


        #endregion







    }
}
