﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixCacheDesktop
{
    public class CacheDto
    {
        public string key { get; set; }
        public string value { get; set; }
        public string expires_in { get; set; }

    }
}
