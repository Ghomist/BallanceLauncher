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
        public string DisplayName { get; private set; }
        public string FullName { get; private set; }
        //public string Hash { get; set; }
        public bool Exists { get; private set; }
        public BallanceMapType Type { get; private set; }

        public BallanceMap() { }

        public BallanceMap(string fullName, string displayName, BallanceMapType type) =>
            (FullName, DisplayName, Type, Exists) = (fullName, displayName, type, true);

        public string GetTypeString() => Type switch
        {
            BallanceMapType.NMO => "NMO",
            BallanceMapType.CMO => "CMO",
            _ => null,
        };

        public Symbol GetSymbol() => Type == BallanceMapType.NMO ? Symbol.Map : Symbol.Document;

        public void Delete()
        {
            File.Delete(FullName);
            Exists = false;
        }

        public override string ToString() => DisplayName;

        public override bool Equals(object obj) => obj is BallanceMap map
            && (map.DisplayName, map.Type) == (DisplayName, Type);

        public override int GetHashCode() => (DisplayName + '.' + GetTypeString()).GetHashCode();
    }

    public enum BallanceMapType
    {
        NMO, CMO
    }
}
