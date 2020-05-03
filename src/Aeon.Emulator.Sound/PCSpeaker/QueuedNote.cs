using System;

namespace Aeon.Emulator.Sound.PCSpeaker
{
    /// <summary>
    /// Stores pitch and duration of a queued PC speaker note.
    /// </summary>
    internal struct QueuedNote : IEquatable<QueuedNote>
    {
        #region Public Static Fields
        /// <summary>
        /// Indicates a rest note.
        /// </summary>
        public static readonly QueuedNote Rest = new QueuedNote();
        #endregion

        #region Private Fields
        private int period;
        private int periodCount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the QueuedNote struct.
        /// </summary>
        /// <param name="halfPeriod">Length of a period in samples.</param>
        /// <param name="periodCount">Number of full periods in the note.</param>
        public QueuedNote(int period, int periodCount)
        {
            this.period = period;
            this.periodCount = periodCount;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Tests for equality between two notes.
        /// </summary>
        /// <param name="noteA">First note to test.</param>
        /// <param name="noteB">Second note to test.</param>
        /// <returns>True if notes are equal; otherwise false.</returns>
        public static bool operator ==(QueuedNote noteA, QueuedNote noteB)
        {
            return noteA.Equals(noteB);
        }
        /// <summary>
        /// Tests for inequality between two notes.
        /// </summary>
        /// <param name="noteA">First note to test.</param>
        /// <param name="noteB">Second note to test.</param>
        /// <returns>True if notes are note equal; otherwise false.</returns>
        public static bool operator !=(QueuedNote noteA, QueuedNote noteB)
        {
            return !noteA.Equals(noteB);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the length of half of a period in samples.
        /// </summary>
        public int Period
        {
            get { return this.period; }
        }
        /// <summary>
        /// Gets the number of full periods in the note.
        /// </summary>
        public int PeriodCount
        {
            get { return this.periodCount; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a value indicating whether this instance is equal to another.
        /// </summary>
        /// <param name="other">Other instance to test for equality.</param>
        /// <returns>True if values are equal; otherwise false.</returns>
        public bool Equals(QueuedNote other)
        {
            return this.period == other.period && this.periodCount == other.periodCount;
        }
        /// <summary>
        /// Returns a value indicating whether this instance is equal to another.
        /// </summary>
        /// <param name="obj">Other instance to test for equality.</param>
        /// <returns>True if values are equal; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if(obj is QueuedNote)
                return Equals((QueuedNote)obj);
            else
                return false;
        }
        /// <summary>
        /// Returns a hash code for the instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return this.period;
        }
        /// <summary>
        /// Returns a string representation of the QueuedNote.
        /// </summary>
        /// <returns>String representation of the QueuedNote.</returns>
        public override string ToString()
        {
            if(this == Rest)
                return "Rest";
            else
                return string.Format("{0}, {1}", this.Period, this.PeriodCount);
        }
        #endregion
    }
}
