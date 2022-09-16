using Swung0x48.Ballance.TdbReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BallanceLauncher.Model
{
    public class BallanceDatabase
    {
        public readonly VirtoolsArray[] Data;
        private readonly VirtoolsArray _levelActivate;
        private readonly VirtoolsArray _options;

        public bool Lv13Enable => Data.Length > 14;

        public BallanceDatabase(VirtoolsArray[] data)
        {
            Data = data;
            _levelActivate = Data.FirstOrDefault(d => d.SheetName == "DB_Levelfreischaltung");
            _options = Data.FirstOrDefault(d => d.SheetName == "DB_Options");
        }

        public (string, int)[] GetRecordsOf(int level)
        {
            if (level < 1 || level > 13) throw new ArgumentOutOfRangeException(nameof(level), "Out of level range");
            if (level == 13 && !Lv13Enable) return null;
            var name = $"DB_Highscore_Lv{level:d02}";
            var data = Data.First(d => d.SheetName == name);
            var ret = new (string, int)[data.RowCount];
            for (int i = 0; i < data.RowCount; ++i)
            {
                var player = (string)data.Cells[0, i];
                var score = (int)data.Cells[1, i];
                ret[i] = (player, score);
            }
            return ret;
        }

        public void ClearRecordsOf(int level)
        {
            if (level < 1 || level > 13) throw new ArgumentOutOfRangeException(nameof(level), "Out of level range");
            if (level == 13 && !Lv13Enable) return;
            var name = $"DB_Highscore_Lv{level:d02}";
            var data = Data.First(d => d.SheetName == name);
            for (int i = 0; i < data.RowCount; ++i)
            {
                data.Cells[0, i] = level == 12 ? "Mrs. Default" : "Mr. Default";
                data.Cells[1, i] = (level == 12 ? 7000 : 4000) - i * 400;
            }
        }

        public void ClearAllRecords()
        {
            for (int i = 1; i <= 13; ++i)
            {
                ClearRecordsOf(i);
            }
        }

        //public bool Lv1State
        //{
        //    get => (int)_levelActivate.Cells[0, 0] == 1;
        //    set => _levelActivate.Cells[0, 0] = value ? 1 : 0;
        //}
        //public bool Lv2State
        //{
        //    get => (int)_levelActivate.Cells[0, 1] == 1;
        //    set => _levelActivate.Cells[0, 1] = value ? 1 : 0;
        //}
        //public bool Lv3State
        //{
        //    get => (int)_levelActivate.Cells[0, 2] == 1;
        //    set => _levelActivate.Cells[0, 2] = value ? 1 : 0;
        //}
        //public bool Lv4State
        //{
        //    get => (int)_levelActivate.Cells[0, 3] == 1;
        //    set => _levelActivate.Cells[0, 3] = value ? 1 : 0;
        //}
        //public bool Lv5State
        //{
        //    get => (int)_levelActivate.Cells[0, 4] == 1;
        //    set => _levelActivate.Cells[0, 4] = value ? 1 : 0;
        //}
        //public bool Lv6State
        //{
        //    get => (int)_levelActivate.Cells[0, 5] == 1;
        //    set => _levelActivate.Cells[0, 5] = value ? 1 : 0;
        //}
        //public bool Lv7State
        //{
        //    get => (int)_levelActivate.Cells[0, 6] == 1;
        //    set => _levelActivate.Cells[0, 6] = value ? 1 : 0;
        //}
        //public bool Lv8State
        //{
        //    get => (int)_levelActivate.Cells[0, 7] == 1;
        //    set => _levelActivate.Cells[0, 7] = value ? 1 : 0;
        //}
        //public bool Lv9State
        //{
        //    get => (int)_levelActivate.Cells[0, 8] == 1;
        //    set => _levelActivate.Cells[0, 8] = value ? 1 : 0;
        //}
        //public bool Lv10State
        //{
        //    get => (int)_levelActivate.Cells[0, 9] == 1;
        //    set => _levelActivate.Cells[0, 9] = value ? 1 : 0;
        //}
        //public bool Lv11State
        //{
        //    get => (int)_levelActivate.Cells[0, 10] == 1;
        //    set => _levelActivate.Cells[0, 10] = value ? 1 : 0;
        //}
        //public bool Lv12State
        //{
        //    get => (int)_levelActivate.Cells[0, 11] == 1;
        //    set => _levelActivate.Cells[0, 11] = value ? 1 : 0;
        //}

        public bool GetLockedOf(int level) => (int)_levelActivate.Cells[0, level - 1] == 1;
        public void SetLockedOf(int level, bool locked) => _levelActivate.Cells[0, level - 1] = locked ? 1 : 0;

        private static readonly List<string> _keys = new() {"1","2","3","4","5","6","7","8","9","0","-","=","BackSpace","Tab","Q","W","E","R","T","Y","U","I","O","P",
        "[","]","Ctrl","A","S","D","F","G","H","J","K","L",";","'","`","Shift","\\","Z","X","C","V","B","N","M",",",".","/",
        "Right Shift","Alt","Space","Num 7","Num 8","Num 9","Num -","Num 4","Num 5","Num 6","Num +","Num 1","Num 2","Num 3","Num 0","Num Del","<","Up","Down","Left","Right"};

        public string KeyForward => _keys[(int)_options.Cells[2, 0]];
        public string KeyBackward => _keys[(int)_options.Cells[3, 0]];
        public string KeyLeft => _keys[(int)_options.Cells[4, 0]];
        public string KeyRight => _keys[(int)_options.Cells[5, 0]];
        public string KeyRotateCam => _keys[(int)_options.Cells[6, 0]];
        public string KeyLiftCam => _keys[(int)_options.Cells[7, 0]];

        public int Volume
        {
            get => (int)((float)_options.Cells[0, 0] * 100);
            set { if (value >= 0 && value <= 100) _options.Cells[0, 0] = value / 100.0f; }
        }
        public bool SynchToScreen
        {
            get => (int)_options.Cells[1, 0] == 1;
            set => _options.Cells[1, 0] = value ? 1 : 0;
        }
        public bool InvertCamRotation
        {
            get => (int)_options.Cells[8, 0] == 1;
            set => _options.Cells[8, 0] = value ? 1 : 0;
        }
        public bool CloudLayer
        {
            get => (int)_options.Cells[10, 0] == 1;
            set => _options.Cells[10, 0] = value ? 1 : 0;
        }
        public string LastPlayer
        {
            get => (string)_options.Cells[9, 0];
            //set => _options.Cells[0, 9] = value;
        }

    }
}
