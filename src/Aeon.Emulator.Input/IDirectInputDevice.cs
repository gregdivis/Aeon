using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input;

[StructLayout(LayoutKind.Sequential)]
internal struct DirectInputDevice8Inst
{
    public unsafe DirectInputDevice8V* Vtbl;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DirectInputDevice8V
{
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, Guid*, void**, uint> QueryInterface;
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, uint> AddRef;
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, uint> Release;

    public IntPtr GetCapabilities;
    public IntPtr EnumObjects;
    public IntPtr GetProperty;
    public IntPtr SetProperty;

    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, uint> Acquire;
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, uint> Unacquire;

    //internal unsafe delegate uint GetDeviceStateProc(IntPtr pThis, uint cbData, DIJOYSTATE* lpvData);
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, uint, DIJOYSTATE*, uint> GetDeviceState;

    public IntPtr GetDeviceData;

    //internal delegate uint SetDataFormatProc(IntPtr pThis, IntPtr lpdf);
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, DIDATAFORMAT*, uint> SetDataFormat;

    public IntPtr SetEventNotification;

    //internal unsafe delegate uint SetCooperativeLevelProc(IntPtr pThis, IntPtr hwnd, uint dwFlags);
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, IntPtr, uint, uint> SetCooperativeLevel;

    public IntPtr GetObjectInfo;
    public delegate* unmanaged[Stdcall]<DirectInputDevice8Inst*, DIDEVICEINSTANCE*, uint> GetDeviceInfo;
    public IntPtr RunControlPanel;
    public IntPtr Initialize;
    public IntPtr CreateEffect;
    public IntPtr EnumEffects;
    public IntPtr GetEffectInfo;
    public IntPtr GetForceFeedbackState;
    public IntPtr SendForceFeedbackCommand;
    public IntPtr EnumCreatedEffectObjects;
    public IntPtr Escape;
    public IntPtr Poll;
    public IntPtr SendDeviceData;
    public IntPtr EnumEffectsInFile;
    public IntPtr WriteEffectToFile;
    public IntPtr BuildActionMap;
    public IntPtr SetActionMap;
    public IntPtr GetImageInfo;
}

//[UnmanagedFunctionPointer(CallingConvention.StdCall)]
//internal delegate uint SetDataFormatProc(IntPtr pThis, IntPtr lpdf);

//[UnmanagedFunctionPointer(CallingConvention.StdCall)]
//internal unsafe delegate uint GetDeviceStateProc(IntPtr pThis, uint cbData, DIJOYSTATE* lpvData);

//[UnmanagedFunctionPointer(CallingConvention.StdCall)]
//internal unsafe delegate uint SetCooperativeLevelProc(IntPtr pThis, IntPtr hwnd, uint dwFlags);

[StructLayout(LayoutKind.Sequential)]
internal struct DIJOYSTATE
{
    public int lX;
    public int lY;
    public unsafe fixed byte rgbButtons[4];
}

internal static class DeviceObjectTypes
{
    public static readonly Guid XAxis = new(0xA36D02E0, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
    public static readonly Guid YAxis = new(0xA36D02E1, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
    public static readonly Guid ZAxis = new(0xA36D02E2, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
    public static readonly Guid Button = new(0xA36D02F0, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
    public static readonly IntPtr DataFormat;

    static DeviceObjectTypes()
    {
        unsafe
        {
            DataFormat = Marshal.AllocCoTaskMem(sizeof(DIOBJECTDATAFORMAT) * 6);
            var ptr = (DIOBJECTDATAFORMAT*)DataFormat.ToPointer();

            fixed (Guid* xaxis = &XAxis)
            {
                ptr[0].pguid = new IntPtr(xaxis);
                ptr[0].dwOfs = 0;
                ptr[0].dwType = 0x00FFFF03;
                ptr[0].dwFlags = 0;
            }

            fixed (Guid* yaxis = &YAxis)
            {
                ptr[1].pguid = new IntPtr(yaxis);
                ptr[1].dwOfs = 4;
                ptr[1].dwType = 0x00FFFF03;
                ptr[1].dwFlags = 0;
            }

            //fixed(Guid* zaxis = &ZAxis)
            //{
            //    ptr[2].pguid = new IntPtr(zaxis);
            //    ptr[2].dwOfs = 8;
            //    ptr[2].dwType = 0x00FFFF03;
            //    ptr[2].dwFlags = 0;
            //}

            for (int i = 0; i < 4; i++)
            {
                ptr[i + 2].pguid = IntPtr.Zero;
                ptr[i + 2].dwOfs = (uint)(i + 8);
                ptr[i + 2].dwType = 0x00FFFF0C;
                ptr[i + 2].dwFlags = 0;
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct DIOBJECTDATAFORMAT
{
    public IntPtr pguid;
    public uint dwOfs;
    public uint dwType;
    public uint dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DIDATAFORMAT
{
    public uint dwSize;
    public uint dwObjSize;
    public uint dwFlags;
    public uint dwDataSize;
    public uint dwNumObjs;
    public IntPtr rgodf;
}
