/********************
*   @Hassan Ali
*   21 Jan 2015
*   SoundManager
********************/

using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    #region Member Variables
    private AudioSource _soundSource;
    private AudioSource _musicSource;
    private string _currentMusicPath = "";
    private bool _isMuted = false;

    private Dictionary<string, AudioClip> _soundClips = new Dictionary<string, AudioClip>();
    private AudioClip _currentMusicClip = null;

    private string _resourcePrefix = "Sounds/";
    private float _volume = 1.0f;
    #endregion

    /// <summary>
    /// Set prefix for sound files in resources folder.
    /// </summary>
    /// <param name="prefix"></param>
    public void SetResourcePrefix(string prefix)
    {
        _resourcePrefix = prefix;
    }

    /// <summary>
    /// Master volume from 0 to 1
    /// </summary>
    public float volume
    {
        set
        {
            _volume = value;

            if (_isMuted)
            {
                AudioListener.volume = 0.0f;
            }
            else
            {
                AudioListener.volume = _volume;
            }
        }
        get
        {
            return AudioListener.volume;
        }
    }

    /// <summary>
    /// Mute volume, saved in PlayerPrefs.
    /// </summary>
    public bool isMuted
    {
        set
        {
            _isMuted = value;
            AudioListener.pause = value;
            PlayerPrefs.SetInt("SoundManager_IsAudioMuted", value ? 1 : 0);

            if (_isMuted)
            {
                AudioListener.volume = 0.0f;
            }
            else
            {
                AudioListener.volume = _volume;
            }
        }
        get
        {
            return AudioListener.pause;
        }
    }

    #region Sounds
    /// <summary>
    /// Preload sounds in memory.
    /// </summary>
    /// <param name="resourceName"></param>
    public void PreloadSound(string resourceName)
    {
        string fullPath = _resourcePrefix + resourceName;

        if (_soundClips.ContainsKey(fullPath))
        {
            return;
        }
        else
        {
            AudioClip soundClip = Resources.Load(fullPath) as AudioClip;

            if (soundClip == null)
            {
                Debug.Log("Couldn't find sound at: " + fullPath);
            }
            else
            {
                _soundClips[fullPath] = soundClip;
            }
        }
    }

    /// <summary>
    /// Play Sound.
    /// </summary>
    /// <param name="resourceName"></param>
    public void PlaySound(string resourceName)
    {
        PlaySound(resourceName, 1.0f);
    }

    /// <summary>
    /// Play sound.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="volume"></param>
	public void PlaySound(string resourceName, float volume)
    {
        if (_isMuted) return;

        string fullPath = _resourcePrefix + resourceName;

        AudioClip soundClip;

        if (_soundClips.ContainsKey(fullPath))
        {
            soundClip = _soundClips[fullPath];
        }
        else
        {
            soundClip = Resources.Load(fullPath) as AudioClip;

            if (soundClip == null)
            {
                Debug.Log("Couldn't find sound at: " + fullPath);
                return;
            }
            else
            {
                _soundClips[fullPath] = soundClip;
            }
        }

        _soundSource.PlayOneShot(soundClip, volume);
    }

    /// <summary>
    /// Unload sound from memory.
    /// </summary>
    /// <param name="resourceName"></param>
    public void UnloadSound(string resourceName)
    {
        string fullPath = _resourcePrefix + resourceName;

        if (_soundClips.ContainsKey(fullPath))
        {
            AudioClip clip = _soundClips[fullPath];
            Resources.UnloadAsset(clip);
            _soundClips.Remove(fullPath);
        }
    }

    /// <summary>
    /// Unload all sounds from memory.
    /// </summary>
    public void UnloadAllSounds()
    {
        foreach (AudioClip audioClip in _soundClips.Values)
        {
            Resources.UnloadAsset(audioClip);
        }

        _soundClips.Clear();
    }
    #endregion

    #region Music
    /// <summary>
    /// Play Music.
    /// </summary>
    /// <param name="resourceName"></param>
    public void PlayMusic(string resourceName)
    {
        PlayMusic(resourceName, 1.0f);
    }

    /// <summary>
    /// Play Music.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="volume"></param>
    public void PlayMusic(string resourceName, float volume)
    {
        PlayMusic(resourceName, volume, true);
    }

    /// <summary>
    /// Play Music.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="volume"></param>
    /// <param name="shouldRestartIfSameSongIsAlreadyPlaying"></param>
	public void PlayMusic(string resourceName, float volume, bool shouldRestartIfSameSongIsAlreadyPlaying)
    {
        if (_isMuted) return;

        string fullPath = _resourcePrefix + resourceName;

        if (_currentMusicClip != null)
        {
            if (_currentMusicPath == fullPath)
            {
                if (shouldRestartIfSameSongIsAlreadyPlaying)
                {
                    _musicSource.Stop();
                    _musicSource.volume = volume;
                    _musicSource.loop = true;
                    _musicSource.Play();
                }
                return;
            }
            else
            {
                _musicSource.Stop();
                Resources.UnloadAsset(_currentMusicClip);
                _currentMusicClip = null;
                _currentMusicPath = "";
            }
        }

        _currentMusicClip = Resources.Load(fullPath) as AudioClip;

        if (_currentMusicClip == null)
        {
            Debug.Log("Error! Couldn't find music clip " + fullPath);
        }
        else
        {
            _currentMusicPath = fullPath;
            _musicSource.clip = _currentMusicClip;
            _musicSource.volume = volume;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    /// <summary>
    /// Stop Music.
    /// </summary>
	public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    /// <summary>
    /// Unload Music
    /// </summary>
    public void UnloadMusic()
    {
        if (_currentMusicClip != null)
        {
            Resources.UnloadAsset(_currentMusicClip);
            _currentMusicClip = null;
            _currentMusicPath = "";
        }
    }
    #endregion

    #region Singleton
    private static SoundManager _instance;

    private static object _lock = new object();

    public static SoundManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(SoundManager) +
                                 "' already destroyed on application quit." +
                                 " Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (SoundManager)FindObjectOfType(typeof(SoundManager));

                    if (FindObjectsOfType(typeof(SoundManager)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                                       " - there should never be more than 1 singleton!" +
                                       " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<SoundManager>();
                        singleton.name = "(singleton) " + typeof(SoundManager).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(SoundManager) +
                                  " is needed in the scene, so '" + singleton +
                                  "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " +
                                  _instance.gameObject.name);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Called once on creation.
    /// </summary>
    protected SoundManager()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _soundSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioListener>();

        if (PlayerPrefs.HasKey("SoundManager_IsAudioMuted"))
        {
            isMuted = (PlayerPrefs.GetInt("SoundManager_IsAudioMuted") == 1);
        }
    }

    private static bool applicationIsQuitting = false;
    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy()
    {
        applicationIsQuitting = true;
    }
    #endregion
}