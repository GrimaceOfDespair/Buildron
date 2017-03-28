﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buildron.Infrastructure.BuildsProvider.Tfs.Models
{
    public enum TfsBuildStatus
    {
        inProgress,

        completed,

        cancelling,

        postponed,

        notStarted,

        all
    }
}