using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input
{
    /// <summary>
    /// Provides access to DirectInput8.
    /// </summary>
    public sealed class DirectInput : IDisposable
    {
        private static WeakReference instance;
        private static readonly object getInstanceLock = new object();

        private IntPtr hwnd;
        private readonly IntPtr directInput;
        private readonly NoParamProc release;
        private readonly EnumDevicesProc enumDevices;
        private readonly CreateDeviceProc createDevice;
        private GCHandle releaseHandle;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInput"/> class.
        /// </summary>
        private DirectInput(IntPtr hwnd)
        {
            IntPtr dinput8;
            var iid = SafeNativeMethods.IID_IDirectInput8W;
            var clsid = SafeNativeMethods.CLSID_DirectInput8;
            var hinst = SafeNativeMethods.GetModuleHandleW(IntPtr.Zero);
            SafeNativeMethods.CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out dinput8);
            this.directInput = dinput8;
            this.hwnd = hwnd;

            unsafe
            {
                DirectInput8Inst* inst = (DirectInput8Inst*)dinput8.ToPointer();

                var initialize = (InitializeInputProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Initialize, typeof(InitializeInputProc));
                var res = initialize(dinput8, hinst, 0x0800);

                this.release = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Release, typeof(NoParamProc));
                this.enumDevices = (EnumDevicesProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->EnumDevices, typeof(EnumDevicesProc));
                this.createDevice = (CreateDeviceProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->CreateDevice, typeof(CreateDeviceProc));
            }

            this.releaseHandle = GCHandle.Alloc(this.release, GCHandleType.Normal);
        }
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DirectInput"/> is reclaimed by garbage collection.
        /// </summary>
        ~DirectInput()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the window handle which owns this instance.
        /// </summary>
        internal IntPtr WindowHandle
        {
            get { return this.hwnd; }
        }

        /// <summary>
        /// Gets the current DirectInput instance.
        /// </summary>
        /// <param name="hwnd">Main application window handle.</param>
        /// <returns>Current DirectInput instance.</returns>
        public static DirectInput GetInstance(IntPtr hwnd)
        {
            lock(getInstanceLock)
            {
                DirectInput directInput;
                if(instance != null)
                {
                    directInput = instance.Target as DirectInput;
                    if(directInput != null)
                    {
                        if(hwnd != IntPtr.Zero)
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
            unsafe
            {
                EnumDevicesCallback callback = (i, p) =>
                    {
                        devices.Add(new DeviceInfo((DIDEVICEINSTANCE*)i.ToPointer()));
                        return 1;
                    };

                var res = this.enumDevices(this.directInput, (uint)deviceClass, callback, IntPtr.Zero, (uint)filter);
            }

            return devices;
        }
        /// <summary>
        /// Returns a DeviceInfo instance describing an input device.
        /// </summary>
        /// <param name="instanceId">Instance ID of the device.</param>
        /// <returns>DeviceInfo instance describing the input device.</returns>
        public DeviceInfo GetDeviceInfo(Guid instanceId)
        {
            return GetDevices(DeviceClass.All, DeviceEnumFlags.All).FirstOrDefault(d => d.InstanceId == instanceId) ?? DeviceInfo.GetUnknownDeviceInfo(instanceId);
        }
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

                if(this.createDevice(this.directInput, &deviceId, &device, IntPtr.Zero) != 0)
                    throw new ArgumentException("Unable to create input device.");

                return new DirectInputDevice(new IntPtr(device), this);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                this.disposed = true;
                this.release(directInput);
                this.releaseHandle.Free();
            }
        }
    }
}
