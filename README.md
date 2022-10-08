# UUID v7

This C# project implements UUID v7 as described in 
[Peabody and Davis](https://datatracker.ietf.org/doc/html/draft-peabody-dispatch-new-uuid-format).
This is the latest IETF draft for UUIDs that are time-sortable
and have a random component to guarantee uniqueness.

It is available on NuGet as `UuidExtensions`.

The internal structure of the UUID uses:

* 36 bits to represent the number of whole seconds since 1 January 1970.
* 24 bits to represent the fractional number of seconds, giving a resolution of 50ns.
* 14 bits for a sequence number, in case of multiple uuids being generated 
  with the same timestamp instant.
    * Needed in cases when the machine's physical clock tick resolution is a lot worse than 
      the 50ns storage resolution.
    * Also needed when generating multiple uuids as of a fixed timestamp in the past.
* 48 bits of randomness.
* 6 "variant" bits to indicate this is a UUID v7 rather than v4 or one of the other UUID formats.

```text
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
```


## UUID v7 in Guid and String formats

Here is a pair of Uuid7s, one as a Guid and the other as a string, 
illustrating how their string representations are time-ordered:

```csharp
var uuid7 = new Uuid7();
Guid g1 = uuid7.Guid();     // e.g. 06338364-8305-788f-8000-9ada942eb663
string s2 = uuid7.String(); //      06338364-8305-7b74-8000-de4963503139
Assert.IsTrue(String.Compare(g1.ToString(), s2) < 0);
```

Note that internally, Microsoft stores Guids using big endian for the first three
components and little endian for the last two. So UUID v7 Guids 
are only time-ordered using `guid.ToString()`, not using `guid.ToByteArray()`.

## Alternative `Id25` string format

This package also supports an alternative string representation that I call `Id25`.
The goal here is to represent UUID v7 as the shortest possible 
alphanumeric string that is simple for humans to say and use. That is:

* It uses a single case not mixed case. We arbitrarily pick lower case.
* No punctuation. Thus a single double-click in a web page selects the whole id.
  There are also no complications for joining/splitting them if used in compound ids.
* If the whole alphabet `a-z` is not needed, drop first `l` and then `o` since
  these are the letters most easily confused with digits `1` and `0`.

To see why Id25 must be 25 characters long, UUIDs have 128 bits. 
An alphabet of 36 characters (`0-9, a-z`) would need a string representation 
$128/log_2(36) = 24.75$ characters long, which gets rounded up to 25.
Now $128/log_2(35) = 24.95$, also just under 25. So we can drop one character from the 
alphabet and keep the string length the same. According to the criteria above,
we should drop `l`, resulting in the 35-character alphabet `0-9, a-k, m-z`.

Finally, note this alphabet must put the digits before the letters
so that the strings come out properly time-ordered.

```csharp
var uuid7 = new Uuid7();
string s1 = uuid7.Id25(); // e.g. 0q994uri6sp53j3eu7bty4wsv
string s2 = uuid7.Id25(); //      0q994uri70qe0gjxrq8iv4iyu
Assert.IsTrue(String.Compare(s1, s2) < 0);
```

## Creating UUID v7 as of different timestamps

The timestamp may be specified explicitly by passing in the time
expressed as the whole number of nanoseconds since 00:00:00 on 1 January 1970 UTC.
Here are two Id25 strings nominally 1ns apart, and a third at the
same timestamp as the second. 


```csharp
var uuid7 = new Uuid7();
long t1 = Uuid7.TimeNs();
long t2 = t1 + 1;
string s1 = uuid7.Id25(t1); // e.g. 0q996kioxxyfds1stmjqajen6
string s2 = uuid7.Id25(t2); //      0q996kioxxyfj83w8bqp67d2j
string s3 = uuid7.Id25(t2); //      0q996kioxxyfj83z4pmujhrx4
Assert.IsTrue(String.Compare(s1, s2) < 0);
Assert.IsTrue(String.Compare(s2, s3) < 0);
```

Note that the second and the third have been generated from the same 
`Uuid7` object. Its internal sequence counter ensures successive 
UUIDv7/Id25 values remain time-ordered even if they have the 
same timestamp. 

The sequence counter resets when the timestamp changes.
So here we cannot tell whether `s1 > s3` or not.

```csharp
string s1 = uuid7.Id25(t1);
string s2 = uuid7.Id25(t2);
string s3 = uuid7.Id25(t1);
```
