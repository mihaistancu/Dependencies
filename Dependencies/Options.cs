using CommandLine;

namespace Dependencies
{
    public class Options {
        [Option(Default = ".")]
        public string Path { get; set; }
        [Option(Default="(.*)(dll|exe)$")]
        public string Filter { get; set; }
        [Option(Default = false)]
        public bool Recursive { get;set; }
    }
}
