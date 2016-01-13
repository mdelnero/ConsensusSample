using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distribution.Library
{
    public class ClusterConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("endpoint", IsRequired = true)]
        public string Endpoint
        {
            get
            {
                return this["endpoint"].ToString();
            }
            set
            {
                this["endpoint"] = value;
            }
        }

        [ConfigurationProperty("Nodes")]
        public NodeConfigCollection Nodes
        {
            get { return base["Nodes"] as NodeConfigCollection; }
        }
    }

    public class NodeConfigurationElement : ConfigurationElement
    {
        public NodeConfigurationElement() 
        {
        }

        public NodeConfigurationElement(string address)
        {
            Address = address;
        }

        [ConfigurationProperty("address", IsRequired = true, IsKey = true)]
        public string Address
        {
            get { return (string)this["address"]; }
            set { this["address"] = value; }
        }       
    }

    [ConfigurationCollection(
        typeof(NodeConfigurationElement), 
        AddItemName = "Node", 
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class NodeConfigCollection : ConfigurationElementCollection
    {
        public NodeConfigurationElement this[int index]
        {
            get { return (NodeConfigurationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(NodeConfigurationElement nodeConfig)
        {
            BaseAdd(nodeConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NodeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NodeConfigurationElement)element).Address;
        }

        public void Remove(NodeConfigurationElement nodeConfig)
        {
            BaseRemove(nodeConfig.Address);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(String name)
        {
            BaseRemove(name);
        }
    }
}
