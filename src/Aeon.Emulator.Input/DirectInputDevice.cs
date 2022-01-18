using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input
{
    /// <summary>
    /// Provides access to a DirectInput device.
    /// </summary>
    public sealed class DirectInputDevice : IDisposable
    {
        private readonly IntPtr device;
        private readonly DirectInput input;
        private readonly NoParamProc release;
        private readonly GetDeviceStateProc getDeviceState;
        private readonly NoParamProc acquire;
        private readonly NoParamProc unacquire;
        private GCHandle releaseHandle;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInputDevice"/> class.
        /// </summary>
        /// <param name="device">The native device interface.</param>
        /// <param name="input">The DirectInput instance which owns this device.</param>
        internal DirectInputDevice(IntPtr device, DirectInput input)
        {
            this.device = device;
            this.input = input;

            unsafe
            {
                var inst = (DirectInputDevice8Inst*)device.ToPointer();

                this.release = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Release, typeof(NoParamProc));

                var setCoopLevel = (SetCooperativeLevelProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetCooperativeLevel, typeof(SetCooperativeLevelProc));
                var res = setCoopLevel(device, input.WindowHandle, 6);

                var setDataFormat = (SetDataFormatProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetDataFormat, typeof(SetDataFormatProc));
                var dataFormat = new DIDATAFORMAT()
                {
                    dwSize = (uint)sizeof(DIDATAFORMAT),
                    dwObjSize = (uint)sizeof(DIOBJECTDATAFORMAT),
                    dwFlags = 1,
                    dwDataSize = (uint)sizeof(DIJOYSTATE),
                    dwNumObjs = 6,
                    rgodf = DeviceObjectTypes.DataFormat
                };

                setDataFormat(device, new IntPtr(&dataFormat));

                this.getDeviceState = (GetDeviceStateProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetDeviceState, typeof(GetDeviceStateProc));
                this.acquire = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Acquire, typeof(NoParamProc));
                this.unacquire = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Unacquire, typeof(NoParamProc));
            }

            this.releaseHandle = GCHandle.Alloc(release, GCHandleType.Normal);
        }
        ~DirectInputDevice() => this.InternalDispose();

        /// <summary>
        /// Gets the X-axis position.
        /// </summary>
        public int XAxisPosition { get; private set; }
        /// <summary>
        /// Gets the Y-axis position.
        /// </summary>
        public int YAxisPosition { get; private set; }
        /// <summary>
        /// Gets the state of button 1.
        /// </summary>
        public bool Button1 { get; private set; }
        /// <summary>
        /// Gets the state of button 2.
        /// </summary>
        public bool Button2 { get; private set; }
        /// <summary>
        /// Gets the state of button 3.
        /// </summary>
        public bool Button3 { get; private set; }
        /// <summary>
        /// Gets the state of button 4.
        /// </summary>
        public bool Button4 { get; private set; }

        /// <summary>
        /// Updates the current device state.
        /// </summary>
        public void Update()
        {
            unsafe
            {
                var state = new DIJOYSTATE();
                var res = this.getDeviceState(this.device, (uint)sizeof(DIJOYSTATE), &state);
                if (res != 0)
                {
                    this.acquire(this.device);
                    res = this.getDeviceState(this.device, (uint)sizeof(DIJOYSTATE), &state);
                }

                if (res == 0)
                {
                    this.XAxisPosition = state.lX;
                    this.YAxisPosition = state.lY;
                    this.Button1 = (state.rgbButtons[0] & 0x80) != 0;
                    this.Button2 = (state.rgbButtons[1] & 0x80) != 0;
                    this.Button3 = (state.rgbButtons[2] & 0x80) != 0;
                    this.Button4 = (state.rgbButtons[3] & 0x80) != 0;
                }
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.InternalDispose();
            GC.SuppressFinalize(this);
        }

        private void InternalDispose()
        {
            if (!disposed)
            {
                disposed = true;
                this.release(device);
                this.releaseHandle.Free();
            }
        }
    }
}
