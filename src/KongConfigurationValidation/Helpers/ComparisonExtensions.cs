using System.Collections.Generic;
using System.Linq;

namespace KongConfigurationValidation.Helpers
{
	public class Variance
	{
		public string Field { get; set; }
		public object Existing { get; set; }
		public object New { get; set; }
	}

	public static class ComparisonExtensions
	{
		public static List<Variance> DetailedCompare<T>(this T val1, T val2)
		{
			var fi = val1.GetType().GetProperties();

			return fi.Select(f => new Variance
				{
					Field = f.Name,
					Existing = f.GetValue(val1),
					New = f.GetValue(val2)
				})
				.Where(v => v.Existing?.Equals(v.New) == false)
				.ToList();
		}
	}
}
