﻿using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class SettingEntity
    {
        [Key]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}