using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeaturesAction.Features
{
	public class ActivateData
	{
		public string id { get; set; }
		public string featureId { get; set; }
		public string version { get; set; }
		public string target { get; set; }
		public string error { get; set; }
		public int status { get; set; }
		public int step { get; set; }
		public DateTime created { get; set; }
		public DateTime modified { get; set; }
	}

	public class ActivateResult
	{
		public ActivateData data { get; set; }
		public bool isSuccess { get; set; }
		public Exception errorMessage { get; set; }
		public int status { get; set; }
	}
}
