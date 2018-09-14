namespace KongConfigurationValidation.DTOs
{
	public class Settings
	{
		public string KongHost { get; set; }
		public int KongAdminPort { get; set; } = 8001;
		public string TestsFolder { get; set; } = "tests";
		public int HttpLogPort { get; set; } = 65150;
	}
}
