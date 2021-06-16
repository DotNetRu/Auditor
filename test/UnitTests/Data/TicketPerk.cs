using System;
using System.Diagnostics.CodeAnalysis;

namespace DotNetRu.Auditor.UnitTests.Data
{
    public sealed class TicketPerk : IEquatable<TicketPerk>
    {
        public string? Name { get; set; }

        public int? Volume { get; set; }

        #region Equality Members

        public bool Equals(TicketPerk? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name && Volume == other.Volume;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TicketPerk);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Volume);
        }

        #endregion Equality Members
    }
}
