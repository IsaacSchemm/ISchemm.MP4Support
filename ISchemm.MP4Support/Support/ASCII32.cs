using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ISchemm.MP4Support.Support {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ASCII32 {
        public fixed byte _data[4];

        public unsafe string String {
            get {
                fixed (ASCII32* ptr = &this) {
                    return new string((sbyte*)ptr, 0, 4);
                }
            }
        }

        public string DisplayString =>
            String.Any(char.IsControl)
            ? "Unprintable"
            : String;

        public override string ToString() => DisplayString;

        public unsafe static ASCII32 FromString(string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            fixed (byte* ptr = data)
            {
                return *(ASCII32*)ptr;
            }
        }
    }
}
