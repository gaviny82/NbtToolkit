using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Test;

internal static class NbtBinaryTestCases
{
    public static readonly sbyte[] TagList_Byte_Values = [123, 127, 0, -128];

    public static readonly byte[] TagList_Byte_Bytes = [
        0x01, // Content tag ID: TAG_Byte
        0x00, 0x00, 0x00, 0x04, // Length: 4
        0x7B, // Item 1: 123
        0x7F, // Item 2: 127
        0x00, // Item 2: 0
        0x80, // Item 3: -128
    ];

    public static readonly short[] TagList_Short_Values = [123, 0, -123];

    public static readonly byte[] TagList_Short_Bytes = [
        0x02, // Content tag ID: TAG_Short
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x7B, // Item 1: 123
        0x00, 0x00, // Item 2: 0
        0xFF, 0x85, // Item 3: -123
    ];

    public static readonly int[] TagList_Int_Values = [123, 0, -123];

    public static readonly byte[] TagList_Int_Bytes = [
        0x03, // Content tag ID: TAG_Int
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x00, 0x00, 0x7B, // Item 1: 123
        0x00, 0x00, 0x00, 0x00, // Item 2: 0
        0xFF, 0xFF, 0xFF, 0x85, // Item 3: -123
    ];

    public static readonly long[] TagList_Long_Values = [123, 0, -123];

    public static readonly byte[] TagList_Long_Bytes = [
        0x04, // Content tag ID: TAG_Long
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, // Item 1: 123
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 2: 0
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x85, // Item 3: -123
    ];

    public static readonly float[] TagList_Float_Values = [
        float.PositiveInfinity,
        float.NegativeInfinity,
        +0.0f,
        -0.0f,
        float.NaN,
        3.1400001049041748046875f,
        -3.1400001049041748046875f
    ];

    public static readonly byte[] TagList_Float_Bytes = [
        0x05, // Content tag ID: TAG_Float
        0x00, 0x00, 0x00, 0x07, // Length: 7
        0x7F, 0x80, 0x00, 0x00, // Item 1: +Inf
        0xFF, 0x80, 0x00, 0x00, // Item 2: -Inf
        0x00, 0x00, 0x00, 0x00, // Item 3: +0
        0x80, 0x00, 0x00, 0x00, // Item 4: -0
        0xFF, 0xC0, 0x00, 0x00, // Item 5: NaN
        0x40, 0x48, 0xF5, 0xC3, // Item 6: 3.1400001049041748046875
        0xC0, 0x48, 0xF5, 0xC3, // Item 6: -3.1400001049041748046875
    ];

    public static readonly double[] TagList_Double_Values = [
        double.PositiveInfinity,
        double.NegativeInfinity,
        +0.0,
        -0.0,
        double.NaN,
        3.140000000000000124344978758017532527446746826171875,
        -3.140000000000000124344978758017532527446746826171875
    ];

    public static readonly byte[] TagList_Double_Bytes = [
        0x06, // Content tag ID: TAG_Double
        0x00, 0x00, 0x00, 0x07, // Length: 7
        0x7F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 1: +Inf
        0xFF, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 1: -Inf
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 3: +0
        0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 4: -0
        0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 5: NaN
        0x40, 0x09, 0x1E, 0xB8, 0x51, 0xEB, 0x85, 0x1F, // Item 6: 3.140000000000000124344978758017532527446746826171875
        0xC0, 0x09, 0x1E, 0xB8, 0x51, 0xEB, 0x85, 0x1F, // Item 7: -3.140000000000000124344978758017532527446746826171875
    ];

    public static readonly string[] TagList_String_Values = ["", "你好", "nbt!"];

    public static readonly byte[] TagList_String_Bytes = [
        0x08, // Content tag ID: TAG_String
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x00, // Item 1: length = 0
                    // Item 1: empty string
        0x00, 0x06, // Item 2: length = 6
        0xE4, 0xBD, 0xA0, 0xE5, 0xA5, 0xBD, // Item 2: "你好"
        0x00, 0x04, // Item 3: length = 4
        0x6E, 0x62, 0x74, 0x21, // Item 3: "nbt!"
    ];

    public static readonly sbyte[][] TagList_ByteArray_Values = [
        [],
        [-128, 127],
        [-2, -1, 0, 1]
    ];

    public static readonly byte[] TagList_ByteArray_Bytes = [
        0x07, // Content tag ID: TAG_Byte_Array
        0x00, 0x00, 0x00, 0x03, // Length: 3
        // Array 1: []
        0x00, 0x00, 0x00, 0x00, // Length = 0
        // Array 2: [-128, 127]
        0x00, 0x00, 0x00, 0x02, // Length = 2
        0x80, 0x7F,
        // Array 3: [-2, -1, 0, 1]
        0x00, 0x00, 0x00, 0x04, // Length = 4
        0xFE, 0xFF, 0x00, 0x01,
    ];

    public static readonly int[][] TagList_IntArray_Values = [
        [],
        [-128, 127],
        [-2, -1, 0, 1]
    ];

    public static readonly byte[] TagList_IntArray_Bytes = [
        0x0B, // Content tag ID: TAG_Int_Array
        0x00, 0x00, 0x00, 0x03, // Length: 3
        // Array 1: []
        0x00, 0x00, 0x00, 0x00, // Length = 0
        // Array 2: [-128, 127]
        0x00, 0x00, 0x00, 0x02, // Length = 2
        0xFF, 0xFF, 0xFF, 0x80,
        0x00, 0x00, 0x00, 0x7F,
        // Array 3: [-2, -1, 0, 1]
        0x00, 0x00, 0x00, 0x04, // Length = 4
        0xFF, 0xFF, 0xFF, 0xFE,
        0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x01,
    ];

    public static readonly long[][] TagList_LongArray_Values = [
        [],
        [-128, 127],
        [-2, -1, 0, 1]
    ];

    public static readonly byte[] TagList_LongArray_Bytes = [
        0x0C, // Content tag ID: TAG_Long_Array
        0x00, 0x00, 0x00, 0x03, // Length: 3
        // Array 1: []
        0x00, 0x00, 0x00, 0x00, // Length = 0
        // Array 2: [-128, 127]
        0x00, 0x00, 0x00, 0x02, // Length = 2
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x80,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F,
        // Array 3: [-2, -1, 0, 1]
        0x00, 0x00, 0x00, 0x04, // Length = 4
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
    ];

    public static readonly byte[] TagCompound_Simple_Bytes = [
        // Item 1
        0x03, // Tag ID: TAG_Int
        0x00, 0x04, // Key length: 4
        0x6B, 0x65, 0x79, 0x31, // UTF8 key: "key1"
        0x00, 0x00, 0x00, 0x01, // TAG_Int payload: 1
        // Item 2
        0x03, // Tag ID: TAG_Int
        0x00, 0x04, // Key length: 4
        0xE9, 0x94, 0xAE, 0x32, // UTF8 key: "键2"
        0x00, 0x00, 0x00, 0x02, // TAG_Int payload: 2
        // TAG_End
        0x00
    ];

    public static readonly TagCompound TagCompound_Simple_Value = new()
    {
        ["key1"] = new TagInt(1),
        ["键2"] = new TagInt(2),
    };

    public static readonly byte[] TagList_IntList_Bytes = [
        0x09, // List content ID: TAG_List
        0x00, 0x00, 0x00, 0x02, // Length: 2
        // Child list 1
        0x03, // List content ID: TAG_Int
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x00, 0x00, 0x01, // Item 1: 1
        0x00, 0x00, 0x00, 0x02, // Item 2: 2
        0x00, 0x00, 0x00, 0x03, // Item 3: 3
        // Child list 2
        0x03, // List content ID: TAG_Int
        0x00, 0x00, 0x00, 0x03, // Length: 3
        0x00, 0x00, 0x00, 0x04, // Item 1: 1
        0x00, 0x00, 0x00, 0x05, // Item 2: 2
        0x00, 0x00, 0x00, 0x06, // Item 3: 3
    ];

    public static readonly TagList<TagList> TagList_IntList_Value = new()
    {
        new TagList<int> { 1, 2, 3 },
        new TagList<int> { 4, 5, 6 },
    };
}
