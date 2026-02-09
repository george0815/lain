using System;
using System.Collections.Generic;
using System.Text;
using lain.protocol.dto;
using lain.protocol.helpers;

namespace lain.protocol
{
    internal class Serializer
    {


        /// <summary>
        /// Serialization entrypoint, takes in a torrent and outputs raw bytes, which then can be written to a file
        /// </summary>
        internal static void SaveTorrentAsFile(Torrent torrent, string filename)
        {

            var model = torrent.ToBencodeModel();
            using var ms = new MemoryStream();
            WriteValues(ms, model);
            System.IO.File.WriteAllBytes(filename, ms.ToArray());


        }

        /// <summary>
        /// Main dispatcher, calls helper method depedning on type
        /// </summary>
        internal static void WriteValues (MemoryStream stream, object model)
        {


            switch (model)
            {

                case long i:
                    WriteInt(stream, i);
                    break;


                case byte[] b:
                    WriteBytes(stream, b);
                    break;

                case List<object> l:
                    WriteList(stream, l);
                    break;

                case Dictionary<byte[], object> d:
                    WriteDict(stream, d);
                    break;

                case SortedDictionary<byte[], object> sd:
                    WriteDict(stream, sd);
                    break;

                default:
                    throw new InvalidDataException($"Unsupported bencode type: {model.GetType()}");



            }



        }



        #region SERIALIZATION HELPERS

        //int
        private static void WriteInt(MemoryStream stream, long value)
        {
            stream.WriteByte((byte)'i');
            WriteAscii(stream, value.ToString());
            stream.WriteByte((byte)'e');
        }

        //bytes
        private static void WriteBytes(MemoryStream stream, byte[] bytes)
        {
            WriteAscii(stream, bytes.Length.ToString());
            stream.WriteByte((byte)':');
            stream.Write(bytes, 0, bytes.Length);
        }


        //list 
        private static void WriteList(MemoryStream stream, List<object> list)
        {
            stream.WriteByte((byte)'l');

            foreach (object item in list) 
                WriteValues(stream, item);
       

            stream.WriteByte((byte)'e');

        }

        //dict
        private static void WriteDict(MemoryStream stream, IDictionary<byte[], object> dict)
        {
            stream.WriteByte((byte)'d');

            foreach (var kv in dict.OrderBy(k => k.Key, ByteComparer.Instance))
            {
                WriteBytes(stream, kv.Key);
                WriteValues(stream, kv.Value);
            }
            


            stream.WriteByte((byte)'e');
        }



        //ascii
        private static void WriteAscii(MemoryStream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }




        #endregion





    }
}
