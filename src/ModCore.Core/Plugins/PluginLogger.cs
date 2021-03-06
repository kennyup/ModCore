﻿using ModCore.Abstraction.DataAccess;
using ModCore.Abstraction.Plugins;
using ModCore.Abstraction.Site;
using ModCore.Models.Core;
using ModCore.Core.Site;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModCore.Core.Plugins
{
    public class PluginLogger : SiteLogger, IPluginLog, ILog
    {

        public PluginLogger(IDataRepository<Log> logDb)
            :base(logDb)
        {
            
        }

        public void SetPlugin(IPlugin plugin)
        {
            _pluginName = plugin.Name;
        }

    }
}
