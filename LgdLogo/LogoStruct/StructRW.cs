using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LgdLogo
{

  static class StructRW
  {
    /// <summary>
    /// StructLayout属性のクラスをwriterに書き込む。
    /// </summary>
    /// <typeparam name="T">書込み対象のクラス</typeparam>
    /// <param name="src">書込み対象のインスタンス</param>
    /// <param name="writer">書込み用のwriter</param>
    public static void Write<T>(T src, BinaryWriter writer) where T : class
    {
      var buffer = ToBytes<T>(src);
      writer.Write(buffer);
    }

    /// <summary>
    /// StructLayout属性のクラスをバイト配列に変換
    /// </summary>
    public static Byte[] ToBytes<T>(T src) where T : class
    {
      var size = Marshal.SizeOf(typeof(T));
      var buffer = new Byte[size];
      var ptr = IntPtr.Zero;

      try
      {
        ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(src, ptr, false);
        Marshal.Copy(ptr, buffer, 0, size);
      }
      finally
      {
        if (ptr != IntPtr.Zero)
          Marshal.FreeHGlobal(ptr);
      }
      return buffer;
    }



    /// <summary>
    /// バイト配列からクラスを読み込む。
    /// </summary>
    /// <typeparam name="T">読込み対象のクラス</typeparam>
    /// <param name="reader">読込み元のreader</param>
    /// <returns>読込まれたインスタンス</returns>
    public static T Read<T>(BinaryReader reader) where T : class
    {
      var size = Marshal.SizeOf(typeof(T));
      var ptr = IntPtr.Zero;

      try
      {
        ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(reader.ReadBytes(size), 0, ptr, size);
        return (T)Marshal.PtrToStructure(ptr, typeof(T));
      }
      finally
      {
        if (ptr != IntPtr.Zero)
          Marshal.FreeHGlobal(ptr);
      }
    }


  }
}
