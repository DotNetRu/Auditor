using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotNetRu.Auditor.Tests.Data
{
    public sealed class Ticket : IEquatable<Ticket>
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public List<string> SpeakerIds { get; } = new();

        public int? SeatNumber { get; set; }

        public DateTime? ShowTime { get; set; }

        public List<TicketPerk> Perks { get; } = new();

        #region Equality Members

        public bool Equals(Ticket? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                Id == other.Id &&
                Name == other.Name &&
                SpeakerIds.SequenceEqual(other.SpeakerIds) &&
                SeatNumber == other.SeatNumber &&
                Nullable.Equals(ShowTime, other.ShowTime) &&
                Perks.SequenceEqual(other.Perks);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Ticket);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, SpeakerIds.GetItemsHashCode(), SeatNumber, ShowTime, Perks.GetItemsHashCode());
        }

        #endregion Equality Members
    }
}
