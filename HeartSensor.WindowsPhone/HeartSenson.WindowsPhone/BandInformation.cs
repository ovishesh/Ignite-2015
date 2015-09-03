using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartSenson.WindowsPhone
{
    public class BandInformation
    {
        public string Name { get; private set; }
        public string Firmware { get; private set; }
        public string Hardware { get; private set; }
        public BandConnectionType ConnectionType { get; private set; }

        public async Task<string> RetrieveInfo(IBandInfo bandInfo, IBandClient client)
        {
            Name = bandInfo.Name;
            ConnectionType = bandInfo.ConnectionType;
            Firmware = await client.GetFirmwareVersionAsync();
            Hardware = await client.GetHardwareVersionAsync();
            return string.Format(" Connected to: {0}" +
                                 " \n Connection type : {1}" +
                                 " \n Firmware : {2} \n Hardware : {3}",
                    Name, ConnectionType, Firmware, Hardware);
        }
    }
}
