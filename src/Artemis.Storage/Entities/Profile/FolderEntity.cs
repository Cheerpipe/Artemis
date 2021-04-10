﻿using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;
using LiteDB;

namespace Artemis.Storage.Entities.Profile
{
    public class FolderEntity : RenderElementEntity
    {
        public FolderEntity()
        {
            PropertyEntities = new List<PropertyEntity>();
            LayerEffects = new List<LayerEffectEntity>();
            ExpandedPropertyGroups = new List<string>();
        }

        public int Order { get; set; }
        public string Name { get; set; }
        public bool Suspended { get; set; }

        [BsonRef("ProfileEntity")]
        public ProfileEntity Profile { get; set; }

        public Guid ProfileId { get; set; }
    }
}