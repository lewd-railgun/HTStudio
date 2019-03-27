using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace HTStudio.Worker
{    
    public static class EZTransXP
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibrary(String fileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi)]
        private extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
        private extern static bool FreeLibrary(IntPtr hModule);

        private static IntPtr GetProcAddressWithCheck(IntPtr hwnd, string procedureName)
        {
            var handle = GetProcAddress(hwnd, procedureName);
            if (handle == IntPtr.Zero)
            {
                throw new Exception("EZTransXP에서 대상 함수를 불러올 수 없었습니다: " + procedureName);
            }
            return handle;
        }

        private static IntPtr EZTransXPHandle;

        public static bool IsInited {
            get; private set;
        } = false;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeEx([MarshalAs(UnmanagedType.LPStr)] string data, [MarshalAs(UnmanagedType.LPStr)] string key);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void Terminate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void FreeMem(IntPtr data);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr TranslateMMNT(int data, IntPtr translateString);

        //Ehnd Extension
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr TranslateMMNTW(int data, IntPtr translateString);

        private static InitializeEx J2K_InitializeEx;
        private static Terminate J2K_Terminate;
        private static FreeMem J2K_FreeMem;
        private static TranslateMMNT J2K_TranslateMMNT;
        private static TranslateMMNTW J2K_TranslateMMNTW;

        private static string GetDLLPath()
        {
            //Preset File
            if (File.Exists("EZTransXP.path"))
            {
                return File.ReadAllText("EZTransXP.path");
            }

            //Reg
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\ChangShin\\ezTrans"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("FilePath");
                        if (o != null)
                        {
                            return o.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static void Init()
        {
            var path = GetDLLPath();
            if(path == null)
            {
                throw new Exception("EZTransXP를 찾을 수 없습니다");
            }

            EZTransXPHandle = LoadLibrary(Path.Combine(path, "J2KEngine.dll"));
            if(EZTransXPHandle == IntPtr.Zero)
            {
                throw new Exception("J2KEngine.dll 을 로드할 수 없습니다. 경로가 잘못되었거나 EZTransXP가 잘못되었습니다.");
            }
            
            J2K_InitializeEx = Marshal.GetDelegateForFunctionPointer<InitializeEx>( GetProcAddressWithCheck(EZTransXPHandle, "J2K_InitializeEx"));
            J2K_Terminate = Marshal.GetDelegateForFunctionPointer<Terminate>(GetProcAddressWithCheck(EZTransXPHandle, "J2K_Terminate"));
            J2K_FreeMem = Marshal.GetDelegateForFunctionPointer<FreeMem>( GetProcAddressWithCheck(EZTransXPHandle, "J2K_FreeMem"));
            J2K_TranslateMMNT = Marshal.GetDelegateForFunctionPointer<TranslateMMNT>( GetProcAddressWithCheck(EZTransXPHandle, "J2K_TranslateMMNT"));
            try
            {
                J2K_TranslateMMNTW = Marshal.GetDelegateForFunctionPointer<TranslateMMNTW>(GetProcAddressWithCheck(EZTransXPHandle, "J2K_TranslateMMNTW"));
            }catch(Exception e)
            {

            }

            //From Anemo
            J2K_InitializeEx("CSUSER123455", Path.Combine(path, "Dat"));

            IsInited = true;
        }

        public static string TranslateJ2K(string japanese)
        {
            if (J2K_TranslateMMNTW != null)
            {
                var strByte = Encoding.Unicode.GetBytes(japanese);
                var strPtr = Marshal.AllocHGlobal(strByte.Length + 2); //유니코드의 널 스트링 구분자는 2바이트를 사용
                Marshal.Copy(strByte, 0, strPtr, strByte.Length);
                Marshal.Copy(new byte[] { 0x0, 0x0 }, 0, strPtr + strByte.Length, 2);

                var translated = J2K_TranslateMMNTW(0, strPtr);

                Marshal.FreeHGlobal(strPtr);

                if (translated == IntPtr.Zero)
                {
                    return "";
                }

                var result = Marshal.PtrToStringUni(translated);

                J2K_FreeMem(translated);

                return result;
            }
            else
            {
                var strByte = Encoding.GetEncoding(932).GetBytes(japanese);
                var strPtr = Marshal.AllocHGlobal(strByte.Length+1);
                Marshal.Copy(strByte, 0, strPtr, strByte.Length);
                Marshal.Copy(new byte[] { 0x0 }, 0, strPtr + strByte.Length, 1);

                var translated = J2K_TranslateMMNT(0, strPtr);

                Marshal.FreeHGlobal(strPtr);

                if (translated == IntPtr.Zero)
                {
                    return "";
                }

                /*
                byte* bytes = (byte*)translated.ToPointer();
                int size = 0;
                while (bytes[size] != 0)
                {
                    ++size;
                }
                byte[] buffer = new byte[size];
                Marshal.Copy(translated, buffer, 0, size);
                */
                byte[] buffer = Encoding.Default.GetBytes(Marshal.PtrToStringAnsi(translated));

                J2K_FreeMem(translated);

                return Encoding.GetEncoding(949).GetString(buffer);
            }

        }

        public static void Free()
        {
            try
            {
                J2K_Terminate();
                FreeLibrary(EZTransXPHandle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
