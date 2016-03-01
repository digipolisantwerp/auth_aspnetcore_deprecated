namespace Toolbox.Auth.Options
{
    public class AuthOptionsJsonFile
    {
        public AuthOptionsJsonFile() : this(AuthOptionsDefaults.OptionsFileName)
        { }

        public AuthOptionsJsonFile(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; set; }
        public string Section { get; set; }
    }
}
