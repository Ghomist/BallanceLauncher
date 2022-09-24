using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BallanceLauncher.Model;
using Swung0x48.Ballance.TdbReader;
using System.Data.HashFunction.CRC;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace BallanceLauncher.Utils
{
    public class TdbHelper
    {
        public static async Task<BallanceDatabase> ReadDatabaseAsync(string path)
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
            using var tdbStream = new TdbStream(readAsEncoded: false, writeAsEncoded: true, fs);
            using var tdbReader = new TdbReader(tdbStream);

            var virtoolsArrays = new List<VirtoolsArray>();
            var tasks = new List<Task>();

            try
            {
                while (tdbStream.Position <= tdbStream.Length)
                {
                    // read array header
                    var array = await VirtoolsArray.CreateAsync(tdbReader, populate: true);
                    virtoolsArrays.Add(array);
                    // read bytes
                    //var bytes = new byte[array.ChunkSize];
                    //await tdbStream.ReadAsync(bytes);
                    // populate
                    //var memory = new ReadOnlyMemory<byte>(bytes);
                    //await array.PopulateAsync(memory);
                }
            }
            catch (EndOfStreamException) { }

            //Task.WaitAll(populateTasks.ToArray()); // wait all!!!

            return new(virtoolsArrays.ToArray());
        }

        public static async Task WriteDatabaseAsync(BallanceDatabase database, string path)
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Write);
            using var tdbStream = new TdbStream(readAsEncoded: true, writeAsEncoded: false, fs);
            using var tdbWriter = new TdbWriter(tdbStream);

            foreach (var vtArr in database.Data)
            {
                var bytes = await vtArr.ToArrayAsync();
                tdbWriter.Write(bytes);
                //await vtArr.WriteToStreamAsync(tdbStream);
            }
        }
    }
}
