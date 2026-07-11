namespace X7.ProjectIndexer.CLI;

public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException("Missing input folder.");

        var input = args[0];

        var output = ".x7index";

        var verbose = false;

        var force = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":

                    output = args[++i];

                    break;

                case "--verbose":

                    verbose = true;

                    break;

                case "--force":

                    force = true;

                    break;
            }
        }

        return new CommandLineOptions
        {
            Input = input,
            Output = output,
            Verbose = verbose,
            Force = force
        };
    }
}