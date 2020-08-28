using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT8297Driver
{
    public class Models
    {
        struct IT8297Report
        {
            byte report_id;
            byte product;
            byte device_num;
            byte total_leds;
            UInt32 fw_ver;
            UInt16 curr_led_count;
            UInt16 reserved0;
            private char[] str_product;
            UInt32 byteorder0; // is little-endian 0x00RRGGBB ?
            UInt32 byteorder1;
            UInt32 byteorder2;
            UInt32 chip_id;
            UInt32 reserved1;
        };

    }
}
