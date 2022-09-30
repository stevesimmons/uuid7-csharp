using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace UuidExtensions
{
    /// <summary>
    /// Generate a UUIDv7 following the Peabody and Davis RFC draft.
    /// Aim is to get get 100ns resolution if possible, 
    /// working on both Windows and Linux.
    /// </summary>
    public class Uuid7
    {
        /// <summary>
        /// The current time in integer nanoseconds, 
        /// measured from the Unix epoch (midnight on 1 January 1970).
        /// </summary>
        /// <returns>Integer number of nanoseconds.</returns>
        public static long TimeNs()
        {
            return 100 * (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks);
        }

        // Time values and sequence counter from the last call
        private static long _x = 0;
        private static long _y = 0;
        private static long _z = 0;
        private static int _seq = 0;

        // Time values and sequence counter from the last asOfNs call.
        // This ensures real-time operations will stay monotonic.
        private static long _x_asOf = 0;
        private static long _y_asOf = 0;
        private static long _z_asOf = 0;
        private static int _seq_asOf = 0;


        /// <summary>
        /// A new UUIDv7 Guid, which is time-ordered, with a nominal
        /// time resolution of 100ns and 32 bits of randomness.
        /// The current time is used, unless overridden.
        /// The special value of 0 gives an all zero uuid.
        /// </summary>
        /// <param name="asOfNs">Optional time to use, in integer nanoseconds since the Unix epoch.</param>
        /// <returns>
        /// Guid that follows UUID v7 format whose string and integer representations are time-sortable.
        /// </returns>
        public Guid Guid(long? asOfNs = null)
        {
            /* The time resolution stored here is 24 fractional bits,
             * corresponding to 50ns. This is sufficient for the underlying
             * 100ns tick size. The actual clock precision may be several
             * times less than this.
             
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                            unixts                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |unixts |   msec (12 bits)      |  ver  |     usec (12 bits)    |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |var|       seq (14 bits)       |          rand (16 bits)       |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                          rand (32 bits)                       |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
             */
            int uuidVersion = 7;
            // The UUID variant is the top-most bits of byte #9.
            // The number of bits increases because as new variants were added, 
            // the previous maximum variant value with "all 1 bits" got
            // extended with a new 0/1 bit to the right.
            // Thus variant 1 for RFC4122 UUIDs is represented by bits 10.
            // (Variant 2 for Microsoft Guids is represented by three bits 110).
            int uuidVariant = 0b10;
            int maxSeqValue = 0x3FFF;

            long ns;
            if (asOfNs == null)
                ns = TimeNs();
            else if (asOfNs == 0)
                // No randomness. In case one is needed for "earlier than everything else"
                return new Guid("00000000-0000-0000-0000-000000000000");
            else
                ns = (long)asOfNs;

            // Get timestamp components of length 32, 16, 12 bits,
            // with the first 36 bits being whole seconds and
            // the remaining 24 bits being fractional seconds.
            long x = Math.DivRem(ns, 16_000_000_000L, out long rest1);
            long y = Math.DivRem(rest1 << 16, 16_000_000_000L, out long rest2);
            long z = Math.DivRem(rest2 << 12, 16_000_000_000L, out long _);

            int seq;
            if (asOfNs != null)
            {
                if (x == _x && y == _y && z == _z)
                {
                    // Shouldn't be possible to call often enough that seq overflows
                    // before the next time tick. If that does happen
                    // subsequent uuids with that time tick will be unique
                    // (because of the random bytes) but no longer ordered.
                    if (_seq < maxSeqValue)
                        _seq += 1;
                }
                else
                {
                    _seq = 0;
                    _x = x;
                    _y = y;
                    _z = z;
                }
                seq = _seq;
            }
            else
            {
                // Check other counters if using asOfNs
                if (x == _x_asOf && y == _y_asOf && z == _z_asOf)
                {
                    if (_seq_asOf < maxSeqValue)
                        _seq_asOf += 1;
                }
                else
                {
                    _seq_asOf = 0;
                    _x_asOf = x;
                    _y_asOf = y;
                    _z_asOf = z;
                }
                seq = _seq_asOf;
            }

            // Last 8 bytes of uuid have variant and sequence in first two bytes,
            // then six bytes of randomness.
            var last8Bytes = new byte[8];
            RandomNumberGenerator.Fill(last8Bytes);
            last8Bytes[0] = (byte)(uuidVariant << 6 | seq >> 8);
            last8Bytes[1] = (byte)(seq & 0xFF);

            // Don't use Guid(bytes[]), which internally uses a mix of
            // big and little endian byte orderings for historical reasons
            // (see https://en.wikipedia.org/wiki/Universally_unique_identifier#Variants).
            // Instead use Guid(int, short, short, bytes[]), which doesn't mix endianness.
            return new Guid(
                (int)x,
                (short)y,
                (short)((uuidVersion << 12) + z & 0xFFFF),
                last8Bytes
            );
        }

        /// <summary>
        /// A UUIDv7 Guid transformed into a 25-character lower-case string which 
        /// preserves the time-ordered property. Using this distinct representation can
        /// reduce the chance that some v4 UUIDs end up in a collection of v7 UUIDs.
        /// 
        /// As for the Uuid7.Guid(), the current time is used, unless overridden.
        /// The special value of 0 gives an all zero uuid.
        /// </summary>
        /// <param name="asOfNs">Optional time to use, in integer nanoseconds since the Unix epoch.</param>
        /// <returns>
        /// 25-character string like "0q974fmmvghw8qfathid7qekc" that is time-sortable.
        /// </returns>
        public string Id25(long? asOfNs = null)
        {
            const string alphabet = "0123456789abcdefghijkmnopqrstuvwxyz"; // 35 chars - no "l"
            char[] id25_chars = new char[25];

            Guid guid = this.Guid(asOfNs);
            byte[] uuid7_bytes = guid.ToByteArray();
            BigInteger rest = new BigInteger(uuid7_bytes);
            BigInteger rem;
            BigInteger divisor = 35;

            for (var pos = 24; pos >= 0; pos--)
            {
                rem = rest % divisor;
                rest /= divisor;
                id25_chars[pos] = alphabet[(int)rem];
            }
            return new string(id25_chars);
        }

        public string String(long? asOfNs = null)
        {
            return this.Guid(asOfNs).ToString();
        }

        /// <summary>
        /// Check whether the tick values on this system are being returned
        /// with ~100ns precision. We should not see 15ms!
        /// Typical values on Win11 seem to be 132ns.
        /// </summary>
        /// <returns>String with description of timing analysis.</returns>
        public static string CheckTimingPrecision()
        {
            var distinctValues = new HashSet<long>();
            var sw = Stopwatch.StartNew();
            long numLoops = 0;
            while (sw.Elapsed.TotalSeconds < 0.5 && distinctValues.Count < 1000)
            {
                distinctValues.Add(TimeNs());
                numLoops += 1;
            }
            sw.Stop();

            var numSamples = distinctValues.Count;
            var actualPrecisionNs = 1_000_000 * sw.Elapsed.TotalMilliseconds / numSamples;
            var maxPrecisionNs = 1_000_000 * sw.Elapsed.TotalMilliseconds / numLoops;

            return $"Precision is {actualPrecisionNs:0}ns rather than {maxPrecisionNs:0}ns ({numSamples:N0} samples in {sw.Elapsed.TotalMilliseconds}ms)";
        }
    }
}
