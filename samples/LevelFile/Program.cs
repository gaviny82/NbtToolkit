using MinecraftToolkit.Nbt.Binary;

string levelPath = Console.ReadLine() ?? throw new InvalidDataException();
using var fileStream = File.OpenRead(levelPath);
var nbtReader = new NbtReader(fileStream, NbtCompression.GZip);
var tagCompound = nbtReader.ReadRootTag();

using FileStream fs = File.Open(Path.Combine(Environment.CurrentDirectory, "level_copy.dat"), FileMode.Create);
NbtWriter nbtWriter = new NbtWriter(fs, NbtCompression.GZip);
nbtWriter.WriteRootTag(tagCompound);
nbtWriter.Dispose();

Console.ReadKey();