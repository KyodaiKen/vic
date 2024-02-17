using libpngchunkdec;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Test PNG:");
Stream File = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
Decoder PNGDec = new(File);
Console.WriteLine("{0} x {1}, {3}, {2} bpc", PNGDec.Image?.Width, PNGDec.Image?.Height, PNGDec.Image?.BitDepth, PNGDec.Image?.ColorType.ToString());
Stream OutputFile = new FileStream(args[1], FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
OutputFile.SetLength(0);
int rowsRead = 0;
while (!PNGDec.EOF || !(rowsRead >= PNGDec.Image?.Height))
{
    int rowsToGet = 1024;
    if (rowsRead + rowsToGet >= PNGDec.Image?.Height)
        rowsToGet = (int)(PNGDec.Image?.Height - rowsRead);
    OutputFile.Write(PNGDec.ReadScanlines(rowsToGet));
    rowsRead += rowsToGet;
    Console.WriteLine("Read {0} rows, {1} bytes, {2} decompressed bytes.", rowsRead, File.Position, OutputFile.Position);
}