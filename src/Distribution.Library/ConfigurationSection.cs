using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distribution.Library
{
    public class ClusterNodesSection : ConfigurationSection
    {
        [ConfigurationProperty("endpoint", IsRequired = true)]
        public Boolean RemoteOnly
        {
            get
            {
                return false;
            }
            set
            {                
            }
        }
    }
}
