using libpngchunkdec;
// See https://aka.ms/new-console-template for more information
try
{
    int nRows = 480;
    if (args.Length > 2)
    {
        if (!int.TryParse(args[2], out nRows))
            Console.WriteLine("Argument #3 nRows ignored, not parsable as an integer.");
        if (nRows <= 0) nRows = 1;
    }
    Console.WriteLine("Test PNG: {0}, num rows per chunk: {1}", args[0], nRows);
    using FileStream InputFile = new(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
    using FileStream OutputFile = new (args[1], new FileStreamOptions { Mode = FileMode.OpenOrCreate, Access = FileAccess.Write, Share = FileShare.Read } );
    OutputFile.SetLength(0);
    using (Decoder PNGDec = new(InputFile, OutputFile))
    {
        Console.WriteLine("{0} x {1}, {3}, {2} bpc", PNGDec.Image.Width, PNGDec.Image.Height, PNGDec.Image.BitDepth, PNGDec.Image.ColorType.ToString());

        int rowsRead = 0;
        while (!PNGDec.EOF || !(rowsRead >= PNGDec.Image.Height))
        {
            int rowsToGet = nRows;
            if (rowsRead + rowsToGet >= PNGDec.Image.Height) rowsToGet = (int)(PNGDec.Image.Height - rowsRead);
            PNGDec.ReadScanlines(rowsToGet);
            rowsRead += rowsToGet;
            Console.WriteLine("Read {0} rows, {1} bytes in, {2} bytes out, mem WS: {3}", rowsRead, PNGTest.Helpers.HumanReadibleSize(InputFile.Position), PNGTest.Helpers.HumanReadibleSize(OutputFile.Position), PNGTest.Helpers.HumanReadibleSize(Environment.WorkingSet));
        }
    }
    OutputFile.Flush(true);
    OutputFile.Close();

}
catch(Exception ex)
{
    Console.WriteLine("\x1b[1;31m(x) FATAL ERROR");
    Console.WriteLine("\x1b[0;91m" + ex.Message + "\x1b[0m");
}
Console.WriteLine("end");
