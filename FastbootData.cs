using System;
using System.Collections.Generic;

namespace FastbootEnhance
{
    class FastbootData
    {
        List<List<string>> raw_data;

        public Dictionary<string, long> partition_size;
        public Dictionary<string, bool?> partition_is_logical;
        public string product;
        public bool secure;
        public string current_slot;
        public bool fastbootd;
        public long max_download_size;
        public string snapshot_update_status;

        public FastbootData(string real_raw_data)
        {
            raw_data = new List<List<string>>();
            partition_size = new Dictionary<string, long>();
            partition_is_logical = new Dictionary<string, bool?>();
            product = null;
            secure = false;
            current_slot = null;
            fastbootd = false;
            max_download_size = -1;
            snapshot_update_status = null;

            foreach (string line in real_raw_data.Split(new char[] { '\n' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                List<string> tmp = new List<string>(line.Split(new char[] { ' ', ':', '\n', '\r', '\t' },
                    StringSplitOptions.RemoveEmptyEntries));

                if (tmp[0].Contains("bootloader"))
                    raw_data.Add(tmp);
            }

            foreach (List<string> line in raw_data)
            {
                if (line[1] == "partition-size")
                {
                    string raw_size = line[3];
                    raw_size = raw_size.Replace("0x", "");
                    try
                    {
                        partition_size.Add(line[2], Convert.ToInt64(raw_size, 16));
                    }
                    catch (Exception)
                    {
                        partition_size[line[2]] = -1;
                    }
                    continue;
                }

                if (line[1] == "is-logical")
                {
                    try
                    {
                        partition_is_logical.Add(line[2], line[3] == "yes");
                    }
                    catch (Exception)
                    {
                        partition_is_logical[line[2]] = null;
                    }
                    continue;
                }

                if (line[1] == "product")
                {
                    product = line[2];
                    continue;
                }

                if (line[1] == "secure")
                {
                    secure = line[2] == "yes";
                    continue;
                }

                if (line[1] == "current-slot")
                {
                    current_slot = line[2];
                    continue;
                }

                if (line[1] == "is-userspace")
                {
                    fastbootd = line[2] == "yes";
                    continue;
                }

                if (line[1] == "max-download-size")
                {
                    max_download_size = Convert.ToInt64(line[2], 16);
                    continue;
                }

                if (line[1] == "snapshot-update-status")
                {
                    snapshot_update_status = line[2];
                    continue;
                }
            }

        }
    }
}
