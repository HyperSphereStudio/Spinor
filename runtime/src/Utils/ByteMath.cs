/*
   * Author : Johnathan Bizzano
   * Created : Monday, March 6, 2023
   * Last Modified : Monday, March 6, 2023
*/

using System;
using runtime.core;

namespace runtime.Utils;

public static class ByteMath
{
    public static int Bits2Bytes(int bits) => (int) Math.Ceiling(bits / 8f);

    public static int PromoteBytes(int bytes) {
        return bytes switch {
            <= 2 => bytes,
            <= 4 => 4,
            <= 8 => 8,
            <= 16 => 16,
            _ => throw new SpinorException($"Unable To Promote Bytes {bytes}")
        };
    }
}