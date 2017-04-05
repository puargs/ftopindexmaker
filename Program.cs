using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopIndexMaker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var argsList = args.ToList<string>();

            //index for any non-flag arguments
            int nonFlagArgCount = 0;

            bool errorEncountered = false;

            string baseDirectory = "";

            string title = TopIndexMaker.DefaultTitle;

            List<TopIndexMaker.Options> options = new List<TopIndexMaker.Options>();

            for (int c = 0; c < argsList.Count; c++) {
                string arg = argsList[c];

                //flag parameter, parse as such
                if (arg.StartsWith("-")) {
                    //parse flags here. If a flag expects a param, remember to advance c by 1 before exiting the if.
                    if (arg.Equals("-preserveunderscores", StringComparison.OrdinalIgnoreCase)) {
                        options.Add(TopIndexMaker.Options.preserveUnderscores);
                        Console.WriteLine("Preserving underscores in album titles.");
                    }

                    else if (arg.Equals("-preserveindex", StringComparison.OrdinalIgnoreCase)) {
                        options.Add(TopIndexMaker.Options.preserveIndexLink);
                        Console.WriteLine("Preserving index.html links.");
                    }

                    else if (arg.Equals("-title", StringComparison.OrdinalIgnoreCase)) {
                        c++;
                        bool titleError = false;

                        if (c < argsList.Count) {
                            title = argsList[c];
                            Console.WriteLine("Using title \"" + title + "\" for top level album.");
                        }

                        if (titleError) {
                            Console.WriteLine("Missing argument for title");
                        }
                    }
                }
                else {

                    //0: This should be the base folder
                    if (nonFlagArgCount == 0) {
                        try {
                            if (Directory.Exists(arg)) {
                                baseDirectory = arg;
                            }
                        }
                        catch (Exception ex) {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                    nonFlagArgCount++;
                }
            }

            if (!errorEncountered && !string.IsNullOrEmpty(baseDirectory)) {
                Console.WriteLine("Running with base folder: " + baseDirectory);
                var tim = new TopIndexMaker(baseDirectory, title, options.ToArray());
                tim.Start();
            }
        }
    }
}
