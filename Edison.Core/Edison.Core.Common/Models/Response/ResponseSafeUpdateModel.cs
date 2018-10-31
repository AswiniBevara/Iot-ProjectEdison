﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Edison.Core.Common.Models
{
    public class ResponseSafeUpdateModel
    {
        public string UserId { get; set; }
        public bool IsSafe { get; set; }
    }
}
