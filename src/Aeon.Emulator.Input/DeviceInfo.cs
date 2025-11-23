namespace Aeon.Emulator.Input;

/// <summary>
/// Contains information about a DirectInput device.
/// </summary>
public sealed class DeviceInfo : IEquatable<DeviceInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceInfo"/> class.
    /// </summary>
    /// <param name="info">The native device info.</param>
    internal unsafe DeviceInfo(DIDEVICEINSTANCE* info)
    {
        this.Name = new string(info->wszInstanceName);
        this.Product = new string(info->wszProductName);
        this.InstanceId = info->guidInstance;
        this.ProductId = info->guidProduct;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceInfo"/> class.
    /// </summary>
    private DeviceInfo(string name, string product, Guid instanceId)
    {
        this.Name = name;
        this.Product = product;
        this.InstanceId = instanceId;
    }

    /// <summary>
    /// Gets the device name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the device product name.
    /// </summary>
    public string Product { get; }
    /// <summary>
    /// Gets the device instance ID.
    /// </summary>
    public Guid InstanceId { get; }
    /// <summary>
    /// Gets the device product ID.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Returns a DeviceInfo instance representing an unknown device.
    /// </summary>
    /// <param name="instanceId">Instance ID of the unknown device.</param>
    /// <returns>DeviceInfo instance representing the unknown device.</returns>
    internal static DeviceInfo GetUnknownDeviceInfo(Guid instanceId) => new("Unknown Device", "Unknown", instanceId);

    public override string ToString() => this.Name;
    public override int GetHashCode() => this.InstanceId.GetHashCode();
    public bool Equals(DeviceInfo? other)
    {
        if (other is null)
            return false;

        return this.InstanceId == other.InstanceId && this.ProductId == other.ProductId;
    }
    public override bool Equals(object? obj) => this.Equals(obj as DeviceInfo);
}
