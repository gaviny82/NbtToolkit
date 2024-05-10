using MinecraftToolkit.Nbt.Parsing;

string levelPath = Console.ReadLine() ?? throw new InvalidDataException();
using var fileStream = File.OpenRead(levelPath);
var nbtReader = new NbtReader(fileStream, NbtCompression.GZip);
var tagCompound = nbtReader.ReadRootTag();
Console.ReadKey();