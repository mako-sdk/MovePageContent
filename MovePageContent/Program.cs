//  <copyright file="Program.cs" company="Global Graphics Software Ltd">
//      Copyright (c) 2021 Global Graphics Software Ltd. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Global Graphics Software Ltd. does not warrant or make any representations
//  regarding the use or results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System;
using System.IO;
using JawsMako;

namespace MovePageContent
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: MovePageContent.exe <path to PDF test folder>");
                Console.WriteLine("Example: MovePageContent testfiles");
                Console.WriteLine("Output is written to a 'Results' folder the test folder.");
                return 1;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine($"Folder {args[0]} not found.");
                return 1;
            }

            string outputFolder = Path.Combine(args[0], "Results");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Create a Mako instance
            var jawsMako = IJawsMako.create();
            IJawsMako.enableAllFeatures(jawsMako);

            // Get the spec finder and content mover ready
            var getSpec = new GetSpec(jawsMako);
            var contentMover = new ContentMover(jawsMako);

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(args[0], "*.pdf");
            foreach (string filePath in fileEntries)
            {
                var pdfOutFile = Path.Combine(outputFolder, Path.GetFileName(filePath));

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Determining specification from {Path.GetFileName(filePath)}...");
                    var result = getSpec.GetSpecFromPage(docPath:filePath, pageIndex:0, out var moveSpec);
                    if (!result)
                        break;
                    
                    // Debug message
                    //Console.WriteLine($"Source: X:{moveSpec.Source.x / 96.0 * 72.0} Y:{moveSpec.Source.y / 96.0 * 72.0} dX:{moveSpec.Source.dX / 96.0 * 72.0} dY:{moveSpec.Source.dY / 96.0 * 72.0}");

                    Console.WriteLine($"Processing {Path.GetFileName(filePath)}...");
                    result = contentMover.MoveContent(docPath: filePath, outDocPath: pdfOutFile, moveSpec);
                    if (!result)
                        break;
                }
            }

            return 0;
        }
    }
}
