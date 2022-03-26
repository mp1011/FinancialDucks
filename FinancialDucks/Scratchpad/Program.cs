// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var lines = File.ReadAllLines(@"E:\Documents\pc.txt");

List<decimal> amounts = new List<decimal>();
int skip = 9;
while(true)
{
    var batch = lines
        .Skip(skip)
        .TakeWhile(p => p.Trim().Length > 0)
        .ToArray();

    if (batch.Length < 3)
        break;

    amounts.Add(decimal.Parse(batch[2]));
    skip += batch.Length+1;
}
Console.ReadKey();