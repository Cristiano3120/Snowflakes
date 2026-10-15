using Snowflakes;
using System.Diagnostics;

namespace Test;

internal class Program
{
    static void Main(string[] args)
    {
        var snowflakeGen = new SnowflakeGen(1);
        //Warmup
        Parallel.For(0, 10000, i =>
        {
            var snowflake = snowflakeGen.NextId();
            Console.WriteLine($"Snowflake: {snowflake.Value}");
        });

        const int SnowflakesToGenerate = 4096 * 1000;
        Snowflake[] snowflakes = new Snowflake[SnowflakesToGenerate];
        Stopwatch stopwatch = Stopwatch.StartNew();
         
        Parallel.For(0, SnowflakesToGenerate, i =>
        {
            snowflakes[i] = snowflakeGen.NextId();
        });
        stopwatch.Stop();
        Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");

        HashSet<Snowflake> uniqueSnowflakes = new HashSet<Snowflake>();
        foreach (var snowflake in snowflakes)
        {
            if (!uniqueSnowflakes.Add(new(snowflake)))
            {
                Console.WriteLine($"Duplicate found for Snowflake: {snowflake.Value}");
            }   
        }
    }
}
