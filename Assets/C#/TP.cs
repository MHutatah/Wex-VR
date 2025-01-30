using System;

public class Instrument
{
    public string Name { get; set; }
    private int volume;
    public static int TotalInstruments = 0;

    public Instrument(string name, int startingVolume)
    {
        Name = name;
        volume = startingVolume;
        TotalInstruments++;
    }

    public void Play()
    {
        Console.WriteLine("The instrument is playing!");
    }

    public void AdjustVolume(int level)
    {
        volume += level;
        Console.WriteLine($"{Name} volume adjusted by {level}. New volume: {volume}");
    }

    public int GetVolume()
    {
        return volume;
    }
}

public class Guitar : Instrument
{
    public int StringCount { get; set; }

    public Guitar(string name, int startingVolume, int stringCount) 
        : base(name, startingVolume)
    {
        StringCount = stringCount;
    }

    public override void Play()
    {
        Console.WriteLine("The guitar strums a melody!");
    }
}

public class Piano : Instrument
{
    public Piano(string name, int startingVolume) 
        : base(name, startingVolume)
    {
    }

    public override void Play()
    {
        Console.WriteLine("The piano produces harmonious notes!");
    }

    public override void AdjustVolume(int level)
    {
        // Limit the volume increase to 10.
        if (level > 10)
        {
            level = 10;
        }
        base.AdjustVolume(level);
    }
}

public class Drum : Instrument
{
    public int DrumSize { get; set; }

    public Drum(string name, int startingVolume, int drumSize) 
        : base(name, startingVolume)
    {
        DrumSize = drumSize;
    }

    public override void Play()
    {
        Console.WriteLine("The drum beats with rhythm!");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create an array of Instrument objects with a mix of Guitar, Piano, and Drum.
        Instrument[] instruments = new Instrument[]
        {
            new Guitar("Guitar", 5, 6),
            new Piano("Piano", 7),
            new Drum("Drum", 10, 14),
            new Guitar("Electric Guitar", 8, 7),
            new Drum("Snare Drum", 6, 12)
        };

        // Simulate all instruments playing.
        foreach (var instrument in instruments)
        {
            instrument.Play();
        }

        // Each instrument adjusts its volume with different increments.
        instruments[0].AdjustVolume(5);
        instruments[1].AdjustVolume(10);
        instruments[2].AdjustVolume(15);
        instruments[3].AdjustVolume(8);
        instruments[4].AdjustVolume(3);

        // Display the volume of each instrument.
        foreach (var instrument in instruments)
        {
            Console.WriteLine($"{instrument.Name} current volume: {instrument.GetVolume()}");
        }

        // Display the total number of instruments created.
        Console.WriteLine($"Total instruments created: {Instrument.TotalInstruments}");
    }
}
