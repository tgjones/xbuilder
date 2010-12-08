using System.Collections.Generic;

namespace XBuilder.Vsx
{
	public class XnaBuildProperties
	{
		public string Importer { get; set; }
		public string Processor { get; set; }
		public Dictionary<string, string> ProcessorParameters { get; private set; }
		public List<string> ProjectReferences { get; private set; }

		public XnaBuildProperties()
		{
			ProcessorParameters = new Dictionary<string, string>();
			ProjectReferences = new List<string>();
		}
	}
}