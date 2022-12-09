namespace Aeon.SourceGenerator
{
    /// <summary>
    /// An ordered list of operands.
    /// </summary>
    internal readonly struct OperandFormat : IReadOnlyList<OperandType>, IEquatable<OperandFormat>
    {
        public OperandFormat(int formatCode) => this.PackedCode = formatCode;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        public OperandType this[int index]
        {
            get
            {
                if (index == 0)
                {
                    var operand1 = (OperandType)(this.PackedCode & 0xFF);
                    if (operand1 == OperandType.None)
                        throw new IndexOutOfRangeException();

                    return operand1;
                }
                else if (index == 1)
                {
                    var operand2 = (OperandType)((this.PackedCode >> 8) & 0xFF);
                    if (operand2 == OperandType.None)
                        throw new IndexOutOfRangeException();

                    return operand2;
                }
                else if (index == 2)
                {
                    var operand3 = (OperandType)((this.PackedCode >> 16) & 0xFF);
                    if (operand3 == OperandType.None)
                        throw new IndexOutOfRangeException();

                    return operand3;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                var operand1 = (OperandType)(this.PackedCode & 0xFF);
                if (operand1 == OperandType.None)
                    return 0;

                var operand2 = (OperandType)((this.PackedCode >>> 8) & 0xFF);
                if (operand2 == OperandType.None)
                    return 1;

                var operand3 = (OperandType)((this.PackedCode >>> 16) & 0xFF);
                if (operand3 == OperandType.None)
                    return 2;

                return 3;
            }
        }

        public IEnumerable<int> SortedIndices
        {
            get
            {
                var startIndex = IndexOfAny(new[] { OperandType.RegisterByte, OperandType.RegisterWord, OperandType.SegmentRegister, OperandType.DebugRegister });
                if (startIndex <= 0)
                {
                    for (int i = 0; i < this.Count; i++)
                        yield return i;
                }
                else
                {
                    yield return startIndex;
                    for (int i = 0; i < startIndex; i++)
                        yield return i;

                    for (int i = startIndex + 1; i < this.Count; i++)
                        yield return i;
                }
            }
        }
        public int PackedCode { get; }

        /// <summary>
        /// Returns a string representation of the operands.
        /// </summary>
        /// <returns>String representation of the operands.</returns>
        public override string ToString() => string.Join(", ", this);
        public int IndexOfAny(IEnumerable<OperandType> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                var index = this.IndexOf(item);
                if (index >= 0)
                    return index;
            }

            return -1;
        }

        public int IndexOf(OperandType item)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                if (this[i] == item)
                    return i;
            }

            return -1;
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// True if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(OperandType item) => this.IndexOf(item) >= 0;
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<OperandType> GetEnumerator()
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
                yield return this[i];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Equals(OperandFormat other) => this.PackedCode == other.PackedCode;
        public override bool Equals(object obj) => obj is OperandFormat o && this.Equals(o);
        public override int GetHashCode() => this.PackedCode;
    }
}
