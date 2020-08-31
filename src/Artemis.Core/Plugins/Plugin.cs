﻿using System;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     This is the base plugin type, use the other interfaces such as Module to create plugins
    /// </summary>
    public abstract class Plugin : IDisposable
    {
        /// <summary>
        ///     Gets the plugin info related to this plugin
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets whether the plugin is enabled
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        ///     Gets or sets a configuration dialog for this plugin that is accessible in the UI under Settings > Plugins
        /// </summary>
        public PluginConfigurationDialog ConfigurationDialog { get; protected set; }

        /// <inheritdoc />
        public void Dispose()
        {
            DisablePlugin();
        }

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        public abstract void EnablePlugin();

        /// <summary>
        ///     Called when the plugin is deactivated or when Artemis shuts down
        /// </summary>
        public abstract void DisablePlugin();

        internal void SetEnabled(bool enable, bool isAutoEnable = false)
        {
            if (enable && !Enabled)
            {
                try
                {
                    if (isAutoEnable && PluginInfo.GetLockFileCreated())
                    {
                        // Don't wrap existing lock exceptions, simply rethrow them
                        if (PluginInfo.LoadException is ArtemisPluginLockException)
                            throw PluginInfo.LoadException;

                        throw new ArtemisPluginLockException(PluginInfo.LoadException);
                    }

                    Enabled = true;
                    PluginInfo.Enabled = true;
                    PluginInfo.CreateLockFile();

                    // Allow up to 15 seconds for plugins to activate.
                    // This means plugins that need more time should do their long running tasks in a background thread, which is intentional
                    // Little meh: Running this from a different thread could cause deadlocks
                    var enableTask = Task.Run(InternalEnablePlugin);
                    if (!enableTask.Wait(TimeSpan.FromSeconds(15)))
                        throw new ArtemisPluginException(PluginInfo, "Plugin load timeout");

                    PluginInfo.LoadException = null;
                    OnPluginEnabled();
                }
                // If enable failed, put it back in a disabled state
                catch (Exception e)
                {
                    Enabled = false;
                    PluginInfo.Enabled = false;
                    PluginInfo.LoadException = e;
                    throw;
                }
                finally
                {
                    if (!(PluginInfo.LoadException is ArtemisPluginLockException))
                        PluginInfo.DeleteLockFile();
                }
            }
            else if (!enable && Enabled)
            {
                Enabled = false;
                PluginInfo.Enabled = false;

                // Even if disable failed, still leave it in a disabled state to avoid more issues
                InternalDisablePlugin();
                OnPluginDisabled();
            }
        }

        internal virtual void InternalEnablePlugin()
        {
            EnablePlugin();
        }

        internal virtual void InternalDisablePlugin()
        {
            DisablePlugin();
        }

        #region Events

        /// <summary>
        ///     Occurs when the plugin is enabled
        /// </summary>
        public event EventHandler PluginEnabled;

        /// <summary>
        ///     Occurs when the plugin is disabled
        /// </summary>
        public event EventHandler PluginDisabled;

        /// <summary>
        ///     Triggers the PluginEnabled event
        /// </summary>
        protected virtual void OnPluginEnabled()
        {
            PluginEnabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Triggers the PluginDisabled event
        /// </summary>
        protected virtual void OnPluginDisabled()
        {
            PluginDisabled?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}