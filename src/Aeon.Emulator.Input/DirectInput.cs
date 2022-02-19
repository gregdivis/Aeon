using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Input
{
    /// <summary>
    /// Provides access to DirectInput8.
    /// </summary>
    public sealed class DirectInput : IDisposable
    {
        private static WeakReference instance;
        private static readonly object getInstanceLock = new();

        private IntPtr hwnd;
        private readonly unsafe DirectInput8Inst* directInput;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInput"/> class.
        /// </summary>
        private DirectInput(IntPtr hwnd)
        {
            unsafe
            {
                var iid = NativeMethods.IID_IDirectInput8W;
                var clsid = NativeMethods.CLSID_DirectInput8;
                var hinst = NativeMethods.GetModuleHandleW(IntPtr.Zero);
                DirectInput8Inst* dinput8 = null;
                _ = NativeMethods.CoCreateInstance(&clsid, null, 1, &iid, (void**)&dinput8);

                dinput8->Vtbl->Initialize(dinput8, hinst, 0x0800);


                this.directInput = dinput8;
                this.hwnd = hwnd;
            }
        }
        ~DirectInput() => this.Dispose();

        /// <summary>
        /// Gets the window handle which owns this instance.
        /// </summary>
        internal IntPtr WindowHandle => this.hwnd;

        /// <summary>
        /// Gets the current DirectInput instance.
        /// </summary>
        /// <param name="hwnd">Main application window handle.</param>
        /// <returns>Current DirectInput instance.</returns>
        public static DirectInput GetInstance(IntPtr hwnd)
        {
            lock (getInstanceLock)
            {
                DirectInput directInput;
                if (instance != null)
                {
                    directInput = instance.Target as DirectInput;
                    if (directInput != null)
                    {
                        if (hwnd != IntPtr.Zero)
                            directInput.hwnd = hwnd;

                        return directInput;
                    }
                }

                directInput = new DirectInput(hwnd);
                instance = new WeakReference(directInput);
                return directInput;
            }
        }
        /// <summary>
        /// Gets the current DirectInput instance.
        /// </summary>
        /// <returns>Current DirectInput instance.</returns>
        public static DirectInput GetInstance()
        {
            return GetInstance(IntPtr.Zero);
        }

        /// <summary>
        /// Returns an enumeration of the available input devices.
        /// </summary>
        /// <param name="deviceClass">The device class to enumerate.</param>
        /// <param name="filter">The device filter to apply to the enumeration.</param>
        /// <returns>Available devices which match the input filters.</returns>
        public IEnumerable<DeviceInfo> GetDevices(DeviceClass deviceClass, DeviceEnumFlags filter)
        {
            var devices = new List<DeviceInfo>();
            var gcHandle = GCHandle.Alloc(devices);
            try
            {
                unsafe
                {
                    _ = this.directInput->Vtbl->EnumDevices(this.directInput, (uint)deviceClass, &EnumDevicesCallback, GCHandle.ToIntPtr(gcHandle), (uint)filter);
                }
            }
            finally
            {
                gcHandle.Free();
            }

            return devices;
        }
        /// <summary>
        /// Returns a DeviceInfo instance describing an input device.
        /// </summary>
        /// <param name="instanceId">Instance ID of the device.</param>
        /// <returns>DeviceInfo instance describing the input device.</returns>
        public DeviceInfo GetDeviceInfo(Guid instanceId) => this.GetDevices(DeviceClass.All, DeviceEnumFlags.All).FirstOrDefault(d => d.InstanceId == instanceId) ?? DeviceInfo.GetUnknownDeviceInfo(instanceId);
        /// <summary>
        /// Returns an instance of an input device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to create.</param>
        /// <returns>Instance of the device.</returns>
        public DirectInputDevice CreateDevice(Guid deviceId)
        {
            unsafe
            {
                DirectInputDevice8Inst* device;

                if (this.directInput->Vtbl->CreateDevice(this.directInput, &deviceId, &device, null) != 0)
                    throw new ArgumentException("Unable to create input device.");

                return new DirectInputDevice(device, this);
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }

        private void Dispose()
        {
            if (!disposed)
            {
                this.disposed = true;
                unsafe
                {
                    this.directInput->Vtbl->Release(this.directInput);
                }
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static unsafe uint EnumDevicesCallback(DIDEVICEINSTANCE* lpddi, IntPtr pvRef)
        {
            var devices = (List<DeviceInfo>)GCHandle.FromIntPtr(pvRef).Target;
            devices.Add(new DeviceInfo(lpddi));
            return 1;
        }
    }
}
