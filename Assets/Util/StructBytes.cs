
using System;
using System.Runtime.InteropServices;

namespace W
{
    public class StructBytes
    {

        public static byte[] StructToBytes(object structObj) {
            int size = Marshal.SizeOf(structObj);
            byte[] bytes = new byte[size];

            IntPtr structPtr = Marshal.AllocHGlobal(size);
            try {
                Marshal.StructureToPtr(structObj, structPtr, false);
                Marshal.Copy(structPtr, bytes, 0, size);
                return bytes;
            } catch (Exception) {
                return null;
            } finally {
                Marshal.FreeHGlobal(structPtr);
            }
        }
        public static object BytesToStruct(byte[] bytes, Type strType) {
            int size = Marshal.SizeOf(strType);
            if (size > bytes.Length) {
                return null;
            }
            IntPtr strPtr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(bytes, 0, strPtr, size);
                object obj = Marshal.PtrToStructure(strPtr, strType);
                return obj;
            } catch (Exception) {
                return null;
            } finally {
                Marshal.FreeHGlobal(strPtr);
            }
        }
    }
}
