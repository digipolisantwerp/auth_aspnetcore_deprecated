namespace Digipolis.Auth.Options
{
    public class AuthOptionsJsonFile
    {
        public AuthOptionsJsonFile() : this(AuthOptionsDefaults.OptionsFileName, AuthOptionsDefaults.OptionsFileAuthSection)
        { }

        public AuthOptionsJsonFile(string fileName, string section)
        {
            FileName = fileName;
            Section = section;
        }

        public string BasePath { get; set; }
        public string FileName { get; set; }
        public string Section { get; set; }
    }
}
