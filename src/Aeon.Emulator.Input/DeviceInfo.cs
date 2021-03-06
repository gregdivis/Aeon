﻿using System;

namespace Aeon.Emulator.Input
{
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
        private DeviceInfo()
        {
        }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the device product name.
        /// </summary>
        public string Product { get; private set; }
        /// <summary>
        /// Gets the device instance ID.
        /// </summary>
        public Guid InstanceId { get; private set; }
        /// <summary>
        /// Gets the device product ID.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Returns a DeviceInfo instance representing an unknown device.
        /// </summary>
        /// <param name="instanceId">Instance ID of the unknown device.</param>
        /// <returns>DeviceInfo instance representing the unknown device.</returns>
        internal static DeviceInfo GetUnknownDeviceInfo(Guid instanceId)
        {
            return new DeviceInfo()
            {
                Name = "Unknown Device",
                Product = "Unknown",
                InstanceId = instanceId
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.InstanceId.GetHashCode();
        }
        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(DeviceInfo other)
        {
            if(object.ReferenceEquals(other, null))
                return false;

            return this.InstanceId == other.InstanceId && this.ProductId == other.ProductId;
        }
        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceInfo);
        }
    }
}
