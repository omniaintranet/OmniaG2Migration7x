﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class ImportMyLinksSettings : BaseMigrationActionSettings
    {
        public string? IconColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? CreatedByUser { get; set; }
    }
}
