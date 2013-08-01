using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageImporter
{
    public static class Serializer
    {
        public static void Serialize<T>(T obj, string path)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                // serialize the specified object to a memory stream
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(outputStream, obj);

                FileStream f = new FileStream(path, FileMode.Create);
                f.Write(outputStream.ToArray(), 0, (int)outputStream.Length);
                f.Close();
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            T retval;
            // serialize the specified object to a memory stream
            BinaryFormatter formatter = new BinaryFormatter();

            // reconstruct an object instance from the serialized data
            using (var f = new FileStream(filePath, FileMode.Open))
            {
                retval = (T)formatter.Deserialize(f);
            }

            return retval;
        }
    }
}
