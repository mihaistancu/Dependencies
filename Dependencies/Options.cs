using CommandLine;

namespace Dependencies
{
    public class Options {
        [Option(Default = ".")]
        public string Path { get; set; }
        [Option(Default = ".")]
        public string NuGetPath { get; set; }
        [Option(Default="(.*)(dll|exe)$")]
        public string Filter { get; set; }
        [Option(Default = false)]
        public bool Recursive { get;set; }
        [Option(Default = false)]
        public bool IncludeMicrosoftAssemblies { get;set; }
    }
}
