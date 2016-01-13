using Distribution.Library;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Distribution.NetworkNode
{
    internal class ServiceHostHelper
    {
        /// <summary>
        /// Service Host.</summary>
        private ServiceHost wcfServiceHost;

        /// <summary>
        /// Start WCF server using net.pipe transport.
        /// </summary>
        /// <param name="singletonInstance"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool StartPipeHost(object singletonInstance, string address)
        {
            bool openSucceeded = false;

            try
            {
                //Uri baseAddress = new Uri("net.pipe://localhost/" + address);

                Uri baseAddress = new Uri("net.tcp://" + address + "/ServiceNode");

               

                wcfServiceHost = new ServiceHost(singletonInstance/*, baseAddress**/);

                NetTcpBinding binding = new NetTcpBinding();

                binding.ReceiveTimeout = TimeSpan.MaxValue;
                binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
                binding.ReliableSession.Enabled = true;



                wcfServiceHost.AddServiceEndpoint( typeof(IClusterNode), binding, baseAddress);



                wcfServiceHost.Open();

                openSucceeded = true;

                foreach (ServiceEndpoint se in wcfServiceHost.Description.Endpoints)
                {
                    Console.WriteLine(String.Format("{0} at {1} using {2}", se.Contract.Name, se.Address, se.Binding.Name));
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(String.Format("An exception occurred: {0}", ce.Message));
                wcfServiceHost.Abort();
                wcfServiceHost = null;
            }
            finally
            {
                if (!openSucceeded && wcfServiceHost != null)
                {
                    wcfServiceHost.Abort();
                    wcfServiceHost = null;
                }
            }

            return openSucceeded;
        }

        /// <summary>
        /// Stops hosting a WCF Service.
        /// </summary>
        public void StopHost()
        {
            if (wcfServiceHost != null)
            {
                wcfServiceHost.Close();
                wcfServiceHost = null;
            }
        }
    }
}
