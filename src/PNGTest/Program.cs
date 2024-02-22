using libpngchunkdec;

// See https://aka.ms/new-console-template for more information
try
{
    Console.WriteLine("Test PNG: {0}", args[0]);
    using Stream InputFile = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
    using Stream OutputFile = new FileStream(args[1], FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
    OutputFile.SetLength(0);
    using (Decoder PNGDec = new(InputFile, OutputFile))
    {
        Console.WriteLine("{0} x {1}, {3}, {2} bpc", PNGDec.Image.Width, PNGDec.Image.Height, PNGDec.Image.BitDepth, PNGDec.Image.ColorType.ToString());

        int rowsRead = 0;
        while (!PNGDec.EOF || !(rowsRead >= PNGDec.Image.Height))
        {
            int rowsToGet = 1024;
            if (rowsRead + rowsToGet >= PNGDec.Image.Height) rowsToGet = (int)(PNGDec.Image.Height - rowsRead);
            PNGDec.ReadScanlines(rowsToGet);
            rowsRead += rowsToGet;
            Console.WriteLine("Read {0} rows, {1} bytes, {2} decompressed bytes.", rowsRead, InputFile.Position, OutputFile.Position);
        }
    }
    OutputFile.Flush();
    OutputFile.Close();
}
catch(Exception ex)
{
    Console.WriteLine("\x1b[1;31m(x) FATAL ERROR");
    Console.WriteLine("\x1b[0;91m" + ex.Message + "\x1b[0m");
}
Console.WriteLine("end");