﻿using System;

namespace Artemis.Storage.Entities.Profile
{
    public class LedEntity
    {
        public Guid Id { get; set; }

        public string LedName { get; set; }
        public int DeviceHash { get; set; }
    }
}