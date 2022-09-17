using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallanceLauncher.Model
{
    public class BallanceMap
    {
        public readonly string DisplayName;
        public readonly string FullName;

        public bool Exists => File.Exists(FullName);

        private readonly BallanceMapType _type;
        public string Type => _type switch
        {
            BallanceMapType.NMO => "NMO",
            BallanceMapType.CMO => "CMO",
            _ => "",
        };
        public Symbol TypeSymbol => _type == BallanceMapType.NMO ? Symbol.Map : Symbol.Document;

        public string Size => new FileInfo(FullName).Length switch
        {
            var x when x < 1024 => $"{x} B",
            var x when x < 1024 * 1024 => $"{x / 1024} KB",
            var x => $"{(float)x / 1024 / 1024:f2} MB"
        };

        public BallanceMap() { }
        public BallanceMap(string fullName, string displayName, BallanceMapType type) =>
            (FullName, DisplayName, _type) = (fullName, displayName, type);

        public void Delete() => File.Delete(FullName);

        public override string ToString() => DisplayName;

        public override bool Equals(object obj) => obj is BallanceMap map
            && (map.DisplayName, map._type) == (DisplayName, _type);

        public override int GetHashCode() => (DisplayName + '.' + Type).GetHashCode();
    }

    public enum BallanceMapType
    {
        NMO, CMO
    }
}
