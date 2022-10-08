using UuidExtensions;

// Print a line on the console with Guid and Id25 forms of a UUID v7:
//
// > UuidExtensions.ConsoleApp.exe
// 063418c8-2955-7ba6-8000-36e6e2d0eeab 0q9kggmfz1wkhxmk7i7k2bb0t

var uuid7 = new Uuid7();
long t = Uuid7.TimeNs();
var s1 = Uuid7.Guid(t).ToString();
var s2 = Uuid7.Id25(t);
Console.WriteLine($"{s1} {s2}");

Console.WriteLine(Uuid7.CheckTimingPrecision());


