﻿using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using Scrubbler.Interfaces;
using Scrubbler.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Scrubbler.ViewModels.ScrobbleViewModels
{
  /// <summary>
  /// ViewModel for the <see cref="Views.ScrobbleViews.CacheScrobblerView"/>
  /// </summary>
  public class CacheScrobblerViewModel : ScrobbleViewModelBase, INeedCachingScrobbler
  {
    #region Properties

    /// <summary>
    /// Gets if the scrobble button is enabled.
    /// </summary>
    public override bool CanScrobble => Scrobbler.Auth.Authenticated && CachedScrobbles.Count > 0 && EnableControls;

    /// <summary>
    /// Gets if the preview button is enabled.
    /// </summary>
    public override bool CanPreview => CachedScrobbles.Count > 0 && EnableControls;

    /// <summary>
    /// Gets if certain controls that modify the
    /// scrobbling data are enabled.
    /// </summary>
    public override bool EnableControls
    {
      get { return _enableControls; }
      protected set
      {
        _enableControls = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange(() => CanScrobble);
        NotifyOfPropertyChange(() => CanPreview);
      }
    }

    /// <summary>
    /// List of cached scrobbles.
    /// </summary>
    public ObservableCollection<Scrobble> CachedScrobbles
    {
      get { return _cachedScrobbles; }
      private set
      {
        _cachedScrobbles = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange(() => CanScrobble);
        NotifyOfPropertyChange(() => CanPreview);
      }
    }
    private ObservableCollection<Scrobble> _cachedScrobbles;

    /// <summary>
    /// If true, tries to scrobble the cache at application startup.
    /// </summary>
    public bool AutoScrobble
    {
      get { return Settings.Default.AutoScrobbleCache; }
      set
      {
        Settings.Default.AutoScrobbleCache = value;
        NotifyOfPropertyChange();
      }
    }

    #endregion Properties

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="windowManager">WindowManager used to display dialogs.</param>
    public CacheScrobblerViewModel(IExtendedWindowManager windowManager)
      : base(windowManager, "Cache Scrobbler")
    {
      CachedScrobbles = new ObservableCollection<Scrobble>();
      StartupHandling();
    }

    /// <summary>
    /// Handles the auto scrobble.
    /// </summary>
    private async void StartupHandling()
    {
      if (base.CanScrobble)
      {
        await GetCachedScrobbles();
        if (AutoScrobble && CanScrobble)
          await Scrobble();
      }
    }

    /// <summary>
    /// Creates a list with scrobbles that will be scrobbles
    /// with the current configuration.
    /// </summary>
    /// <returns>List with scrobbles.</returns>
    protected override IEnumerable<Scrobble> CreateScrobbles()
    {
      return CachedScrobbles;
    }

    /// <summary>
    /// Scrobbles the selected tracks.
    /// </summary>
    /// <returns>Task.</returns>
    public override async Task Scrobble()
    {
      try
      {
        EnableControls = false;
        OnStatusUpdated("Trying to scrobble cached tracks...");
        var response = await Scrobbler.SendCachedScrobblesAsync();

        if (response.Success && response.Status == LastResponseStatus.Successful)
          OnStatusUpdated("Successfully scrobbled cached tracks");
        else
          OnStatusUpdated(string.Format("Error scrobbling cached tracks: {0}", response.Status));
      }
      catch(Exception ex)
      {
        OnStatusUpdated(string.Format("Fatal error while trying to scrobble cached tracks: {0}", ex.Message));
      }
      finally
      {
        await GetCachedScrobbles();
        EnableControls = true;
      }
    }

    /// <summary>
    /// Read the cached srobbles.
    /// </summary>
    public async Task GetCachedScrobbles()
    {
      try
      {
        EnableControls = false;
        CachedScrobbles = new ObservableCollection<Scrobble>(await Scrobbler.GetCachedAsync());
      }
      catch(Exception ex)
      {
        OnStatusUpdated(string.Format("Fatal error getting cached scrobbles: {0}", ex.Message));
      }
      finally
      {
        EnableControls = true;
      }
    }
  }
}