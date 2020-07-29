﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M4ProfileSegmentsMigration : IStorageMigration
    {
        public int UserVersion => 4;

        public void Apply(LiteRepository repository)
        {
            var profiles = repository.Query<ProfileEntity>().ToList();
            foreach (var profileEntity in profiles)
            {
                foreach (var folder in profileEntity.Folders.Where(f => f.MainSegmentLength == TimeSpan.Zero))
                {
                    if (folder.PropertyEntities.Any(p => p.KeyframeEntities.Any()))
                        folder.MainSegmentLength = folder.PropertyEntities.Where(p => p.KeyframeEntities.Any()).Max(p => p.KeyframeEntities.Max(k => k.Position));
                    if (folder.MainSegmentLength == TimeSpan.Zero)
                        folder.MainSegmentLength = TimeSpan.FromSeconds(5);
                }

                foreach (var layer in profileEntity.Layers.Where(l => l.MainSegmentLength == TimeSpan.Zero))
                {
                    if (layer.PropertyEntities.Any(p => p.KeyframeEntities.Any()))
                        layer.MainSegmentLength = layer.PropertyEntities.Where(p => p.KeyframeEntities.Any()).Max(p => p.KeyframeEntities.Max(k => k.Position));
                    if (layer.MainSegmentLength == TimeSpan.Zero)
                        layer.MainSegmentLength = TimeSpan.FromSeconds(5);
                }

                repository.Update(profileEntity);
            }
        }
    }
}