using System;
using System.Collections;
using System.Linq;
using System.Diagnostics;


namespace GeneticSharp.Domain.Chromosomes
{
    /// <summary>
    /// Represents a gene of a chromosome.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public struct Gene : IEquatable<Gene>
    {
        #region Fields
        private object m_value;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Chromosomes.Gene"/> struct.
        /// </summary>
        /// <param name="value">The gene initial value.</param>
        public Gene(object value)
        {
            m_value = value;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public Object Value
        {
            get
            {
                return m_value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Gene first, Gene second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Gene first, Gene second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.</returns>
        public override string ToString()
        {
            return Value != null ? Value.ToString() : String.Empty;
        }

        /// <summary>
        /// Determines whether the specified <see cref="GeneticSharp.Domain.Chromosomes.Gene"/> is equal to the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.
        /// </summary>
        /// <param name="other">The <see cref="GeneticSharp.Domain.Chromosomes.Gene"/> to compare with the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="GeneticSharp.Domain.Chromosomes.Gene"/> is equal to the current
        /// <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Gene other)
        {
            var eValue = Value as IEnumerable;
            var eOther = other.Value as IEnumerable;
            if(eValue == null ||  eOther == null)
            {
                // They are not enumerables, use default behavior
                return Value.Equals(other.Value);
            }
            else
            {
                // They are enumerable make a "SequenceEquals"
                bool isTrue = true;

                var Vi = eValue.GetEnumerator();
                var Oi = eOther.GetEnumerator();

                // Check equality of all items
                while(Vi.MoveNext() && Oi.MoveNext())
                {
                    isTrue &= Vi.Current.Equals(Oi.Current);
                }

                // check if they are different lengths
                if(Vi.MoveNext() || Oi.MoveNext())
                    return false;

                return isTrue;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="GeneticSharp.Domain.Chromosomes.Gene"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Gene)
            {
                var other = (Gene)obj;

                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (Value == null)
            {
                return 0;
            }

            return Value.GetHashCode();
        }
        #endregion
    }
}
