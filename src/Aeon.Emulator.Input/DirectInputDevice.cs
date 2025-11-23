namespace Aeon.Emulator.Input;

/// <summary>
/// Provides access to a DirectInput device.
/// </summary>
public sealed class DirectInputDevice : IDisposable
{
    private readonly unsafe DirectInputDevice8Inst* device;
    private readonly DirectInput input;
    private readonly Lazy<DeviceInfo?> deviceInfo;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectInputDevice"/> class.
    /// </summary>
    /// <param name="device">The native device interface.</param>
    /// <param name="input">The DirectInput instance which owns this device.</param>
    internal unsafe DirectInputDevice(DirectInputDevice8Inst* device, DirectInput input)
    {
        this.device = device;
        this.input = input;

        unsafe
        {
            this.device = device;
            uint res = this.device->Vtbl->SetCooperativeLevel(device, input.WindowHandle, 6);

            var dataFormat = new DIDATAFORMAT
            {
                dwSize = (uint)sizeof(DIDATAFORMAT),
                dwObjSize = (uint)sizeof(DIOBJECTDATAFORMAT),
                dwFlags = 1,
                dwDataSize = (uint)sizeof(DIJOYSTATE),
                dwNumObjs = 6,
                rgodf = DeviceObjectTypes.DataFormat
            };

            res = this.device->Vtbl->SetDataFormat(device, &dataFormat);
        }

        this.deviceInfo = new Lazy<DeviceInfo?>(this.GetDeviceInfo, LazyThreadSafetyMode.PublicationOnly);
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
    /// Gets information about the device.
    /// </summary>
    public DeviceInfo? Info => this.deviceInfo.Value;

    /// <summary>
    /// Updates the current device state.
    /// </summary>
    public bool Update()
    {
        unsafe
        {
            var state = new DIJOYSTATE();
            var res = this.device->Vtbl->GetDeviceState(this.device, (uint)sizeof(DIJOYSTATE), &state);
            if (res != 0)
            {
                this.device->Vtbl->Acquire(this.device);
                res = this.device->Vtbl->GetDeviceState(this.device, (uint)sizeof(DIJOYSTATE), &state);
            }

            if (res == 0)
            {
                this.XAxisPosition = state.lX;
                this.YAxisPosition = state.lY;
                this.Button1 = (state.rgbButtons[0] & 0x80) != 0;
                this.Button2 = (state.rgbButtons[1] & 0x80) != 0;
                this.Button3 = (state.rgbButtons[2] & 0x80) != 0;
                this.Button4 = (state.rgbButtons[3] & 0x80) != 0;
                return true;
            }

            return false;
        }
    }
    public void Dispose()
    {
        this.InternalDispose();
        GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
        if (!this.disposed)
        {
            unsafe
            {
                this.device->Vtbl->Release(this.device);
            }

            this.disposed = true;
        }
    }
    private DeviceInfo? GetDeviceInfo()
    {
        unsafe
        {
            DIDEVICEINSTANCE info;
            if (this.device->Vtbl->GetDeviceInfo(this.device, &info) == 0)
                return new DeviceInfo(&info);
            else
                return null;
        }
    }
}
