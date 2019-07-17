using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BeatOn.ClientModels;
using BeatOn.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestomAssets;
using QuestomAssets.AssetOps;
using QuestomAssets.AssetsChanger;
using QuestomAssets.BeatSaber;
using QuestomAssets.Models;
using QuestomAssets.Mods;
using QuestomAssets.Utils;

namespace BeatOn
{
    public class ImportManager
    {
        private Func<QuestomAssetsEngine> _getEngine;
        private Func<BeatOnConfig> _getConfig;
        private QaeConfig _qaeConfig;
        private ShowToastDelegate _showToast;
        private Func<DownloadManager> _getDownloadManager;
        private Action _triggerConfigChanged;
        public ImportManager(QaeConfig qaeConfig, Func<BeatOnConfig> getConfig, Func<QuestomAssetsEngine> getEngine, ShowToastDelegate showToast, Func<DownloadManager> getDownloadManager, Action triggerConfigChanged)
        {
            _qaeConfig = qaeConfig;
            _getConfig = getConfig;
            _getEngine = getEngine;
            _showToast = showToast;
            _getDownloadManager = getDownloadManager;
            _triggerConfigChanged = triggerConfigChanged;
        }

        /// <summary>
        /// try to get a playlist ID from something... this may need to change in the future, which will be awkward
        /// </summary>
        private string GetPlaylistID(string filename, BPList bplist)
        {
            return Path.GetFileNameWithoutExtension(filename.GetFilenameFwdSlash());
        }


        public void ImportFile(string filename, string mimeType, byte[] fileData)
        {
            var ext = Path.GetExtension(filename).ToLower().ToLower().TrimStart('.');
            if (ext == "bplist" || ext == "json")
            {
                try
                {
                    //oh boy a playlist!
                    BPList bplist;
                    using (MemoryStream ms = new MemoryStream(fileData))
                    using (StreamReader sr = new StreamReader(ms))
                    using (JsonTextReader tr = new JsonTextReader(sr))
                        bplist = new JsonSerializer().Deserialize<BPList>(tr);

                    if (bplist.PlaylistTitle == null || bplist.Songs == null)
                    {
                        Log.LogErr($"Playlist title is null and it's songlist is null from file {filename}.");
                        _showToast("Playlist Failed", $"{filename} did not seem to have a playlist in it.", ToastType.Error);
                        return;
                    }

                    Log.LogMsg($"Creating playlist '{bplist.PlaylistTitle}'");
                    var bspl = new BeatSaberPlaylist()
                    {
                        PlaylistID = GetPlaylistID(filename, bplist),
                        CoverImageBytes = bplist.Image,
                        PlaylistName = bplist.PlaylistTitle
                    };
                    var plOp = new AddOrUpdatePlaylistOp(bspl);
                    _getEngine().OpManager.QueueOp(plOp);

                    plOp.FinishedEvent.WaitOne();
                    if (plOp.Status == OpStatus.Failed)
                    {
                        Log.LogErr("Exception in ImportFile handling a bplist playlist!", plOp.Exception);
                        _showToast("Playlist Failed", $"Unable to create playlist '{bplist.PlaylistTitle}'!", ToastType.Error);
                        return;
                    }
                    else if (plOp.Status == OpStatus.Complete)
                    {
                        //SUCCESS!  except what a fail on this poorly structured if/else block
                    }
                    else
                    {
                        throw new Exception("Playlist op finished with a completely unexpected status.");
                    }

                    var downloads = new List<Download>();
                    var ops = new ConcurrentBag<AssetOp>();
                    Debouncey<bool> configDebouncer = new Debouncey<bool>(5000, false);
                    configDebouncer.Debounced += (s, e) =>
                         {
                             _getConfig().Config = _getEngine().GetCurrentConfig();
                         };
                    foreach (var song in bplist.Songs)
                    {
                        string url = Constants.BEATSAVER_ROOT;
                        if (!string.IsNullOrWhiteSpace(song.Key))
                        {
                            url += string.Format(Constants.BEATSAVER_KEY_API, song.Key);
                        }
                        else if (!string.IsNullOrWhiteSpace(song.Hash))
                        {
                            url += string.Format(Constants.BEATSAVER_HASH_API, song.Hash);
                        }
                        else
                        {
                            Log.LogErr($"Song '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' has no hash or key.");
                            var dl = new Download("http://localhost", false);
                            dl.SetStatus(DownloadStatus.Failed, $"Song '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' has no hash or key.");
                            downloads.Add(dl);
                            continue;
                        }
                        try
                        {
                            var downloadUrl = BeatSaverUtils.GetDownloadUrl(url);
                            if (string.IsNullOrWhiteSpace(downloadUrl))
                            {
                                Log.LogErr($"Song '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' did not have a downloadURL in the response from beat saver.");
                                var dl = new Download("http://localhost", false);
                                dl.SetStatus(DownloadStatus.Failed, $"Song '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' did not have a downloadURL in the response from beat saver.");
                                downloads.Add(dl);
                                continue;
                            }
                            try
                            {
                                var dl = _getDownloadManager().DownloadFile(Constants.BEATSAVER_ROOT.CombineFwdSlash(downloadUrl), false);
                                dl.StatusChanged += (s, e) =>
                                {
                                    if (e.Status == DownloadStatus.Failed)
                                    {
                                        Log.LogErr($"Song '{song.SongName}' in playlist '{bplist.PlaylistTitle}' failed to download.");
                                    }
                                    else if (e.Status == DownloadStatus.Processed)
                                    {
                                        try
                                        {
                                            MemoryStream ms = new MemoryStream(dl.DownloadedData);
                                            try
                                            {
                                                var provider = new ZipFileProvider(ms, dl.DownloadedFilename, FileCacheMode.None, true, FileUtils.GetTempDirectory());
                                                try
                                                {
                                                    var op = ImportSongFile(provider, () =>
                                                    {
                                                        provider.Dispose();
                                                        ms.Dispose();
                                                    }, bspl, true);
                                                    op.OpFinished += (so, eo) =>
                                                      {
                                                          if (eo.Status == OpStatus.Complete)
                                                          {
                                                              configDebouncer.EventRaised(this, true);
                                                          }
                                                      };
                                                    ops.Add(op);
                                                }
                                                catch
                                                {
                                                    provider.Dispose();
                                                    throw;
                                                }
                                            }
                                            catch
                                            {
                                                ms.Dispose();
                                                throw;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.LogErr($"Song '{song.SongName}' in playlist '{bplist.PlaylistTitle}' threw an exception on adding to playlist", ex);
                                        }
                                    }
                                };
                                downloads.Add(dl);
                            }
                            catch (Exception ex)
                            {
                                Log.LogErr($"Exception making secondary call to get download readirect URL for '{downloadUrl}' on beat saver!", ex);
                                var dl = new Download("http://localhost", false);
                                dl.SetStatus(DownloadStatus.Failed, $"Failed to get download information for '{downloadUrl}' from beat saver!");
                                downloads.Add(dl);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Exception on song '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' getting info from beatsaver!", ex);
                            var dl = new Download("http://localhost", false);
                            dl.SetStatus(DownloadStatus.Failed, $"Failed to get information on '{song.SongName ?? "(null)"}' in playlist '{bplist.PlaylistTitle}' from beatsaver!");
                            downloads.Add(dl);
                        }
                    }

                    if (downloads.Count < 1)
                    {
                        Log.LogErr($"Every single song in playlist '{bplist.PlaylistTitle}' failed to start downloading!");
                        _showToast("Playlist Failed", $"Every song in the playlist '{bplist.PlaylistTitle}' failed to start downloading.", ToastType.Error);
                        return;
                    }
                    downloads.WaitForFinish();
                    ops.WaitForFinish();

                    var badDl = downloads.Count(x => x.Status == DownloadStatus.Failed);
                    var goodCount = ops.Count(x => x.Status == OpStatus.Complete);
                    var existCount = ops.Count(x => x.Status == OpStatus.Failed && x.Exception as AddSongException != null && ((AddSongException)x.Exception).FailType == AddSongFailType.SongExists);
                    var badSong = ops.Count(x => x.Status == OpStatus.Failed) - existCount;
                    _getConfig().Config = _getEngine().GetCurrentConfig();
                    _showToast("Playlist Import Complete", $"Playlist '{bplist.PlaylistTitle}' has completed.  {goodCount} of {bplist.Songs.Count} songs succeeded.", ToastType.Success, 8);
                    // successfully downloaded, {existCount} already existed in other playlists, {badDl} failed to download, {badSong} failed to import."

                    //save playlist file for later use on QAE commit
                    var plFileSave = _qaeConfig.PlaylistsPath.CombineFwdSlash(Path.GetFileNameWithoutExtension(filename) + ".json");
                    QueuedFileOp qfo = new QueuedFileOp()
                    {
                        Type = QueuedFileOperationType.WriteFile,
                        TargetPath = plFileSave,
                        SourceData = fileData
                    };
                    _getEngine().OpManager.QueueOp(qfo);
                }
                catch (Exception ex)
                {
                    Log.LogErr("Exception in ImportFile handling a bplist playlist!", ex);
                    _showToast("Playlist Failed", $"Failed to read playlist from {filename}!", ToastType.Error);
                }
            }
            else
            {
                Log.LogErr($"Unsupported file type uploaded as {filename}");
                _showToast("Unsupported File", $"The file {filename} is not a supported type.", ToastType.Error);
            }
        }

        /// <summary>
        /// Imports a song/mod/etc from a file provider
        /// </summary>
        /// <param name="provider">The file provider containing the data at its root</param>
        /// <param name="completionCallback">A completion callback for when the operations have all completed.  Will not be called if there's an exception during initial processing, but will be called if a background operation fails</param>
        /// <param name="targetPlaylistID">Optionally the playlist to add the song to, if the download is a song</param>
        public void ImportFromFileProvider(IFileProvider provider, Action completionCallback, string targetPlaylistID = null, bool suppressToast = false)
        {
            try
            {
                if (string.IsNullOrEmpty(provider.SourceName))
                {
                    throw new ImportException("Unable to determine the filename from the provider!", "The filename could not be determined for this download!");
                }
                var type = IdentifyFileType(provider);
                switch (type)
                {
                    case DownloadType.ModFile:
                        ImportModFile(provider);
                        completionCallback?.Invoke();
                        break;
                    case DownloadType.SongFile:
                        BeatSaberPlaylist playlist = null;

                        if (targetPlaylistID != null)
                            playlist = _getConfig().Config.Playlists.FirstOrDefault(x => x.PlaylistID == targetPlaylistID);

                        ImportSongFile(provider, completionCallback, playlist, suppressToast);
                        break;
                    case DownloadType.Playlist:
                        ImportPlaylistFilesFromProvider(provider);
                        break;
                    case DownloadType.OldSongFile:
                    default:
                        throw new ImportException($"Unhandled file type {type} with {provider.SourceName}", $"File {provider.SourceName} isn't a known type that Beat On can use!");
                }
               // _showToast("File Processed", $"The file {provider.SourceName} has been proccessed.", ClientModels.ToastType.Info, 2);
            }
            catch (ImportException iex)
            {
                if (!suppressToast)
                    _showToast("File Failed to Process!", $"The file {provider.SourceName} could not be processed: {iex.FriendlyMessage}", ClientModels.ToastType.Error, 5);
                throw;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Unhandled exception importing file from provider {provider.SourceName}", ex);
                if (!suppressToast)
                    _showToast("File Failed to Process!", $"The file {provider.SourceName} could not be processed!", ClientModels.ToastType.Error, 5);
                throw new ImportException($"Unhandled exception importing file from provider {provider.SourceName}", $"Unable to load file {provider.SourceName}!", ex);
            }
        }

        private void ImportPlaylistFilesFromProvider(IFileProvider provider)
        {
            int success = 0;
            int fail = 0;
            foreach (var file in provider.FindFiles("*.json"))
            {
                try
                {
                    Log.LogMsg($"Importing playlist file '{file}' from zip '{provider.SourceName}'");
                    ImportFile(file.GetFilenameFwdSlash(), "application/json", provider.Read(file));
                    success++;
                }
                catch (Exception ex)
                {
                    Log.LogErr($"Exception trying to add playlist file '{file}' from zip '{provider.SourceName}'", ex);
                    fail++;
                    continue;
                }
            }
            //todo: show a toast here?
        }

        private AssetOp ImportSongFile(IFileProvider provider, Action completionCallback, BeatSaberPlaylist addToPlaylist = null, bool suppressToast = false)
        {
            try
            {
                var songPath = ExtractSongGetPath(provider);
                var songID = songPath.GetFilenameFwdSlash();
                var playlist = addToPlaylist??GetOrCreateDefaultPlaylist();
                return QueueAddSongToPlaylistOp(songID, songPath, playlist, completionCallback, suppressToast);
            }
            catch (ImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Unhandled exception importing song file from provider {provider.SourceName}", ex);
                throw new ImportException($"Unhandled exception importing song file from provider {provider.SourceName}", $"Unable to load song from file {provider.SourceName}!", ex);
            }
        }

        private void ImportModFile(IFileProvider provider)
        {
            try
            {
                ExtractAndInstallMod(provider);
            }
            catch (ImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Unhandled exception importing mod file from provider {provider.SourceName}", ex);
                throw new ImportException($"Unhandled exception importing mod file from provider {provider.SourceName}", $"Unable to load mod from file {provider.SourceName}!", ex);
            }
        }

        /// <summary>
        /// Extracts a song from a provider and returns the path RELATIVE TO THE BEATONDATAROOT
        /// </summary>
        private string ExtractSongGetPath(IFileProvider provider)
        {
            try
            {
                //use the incoming file or folder name (without extension) as the song ID.  Since the filesystem has to have unique names, this should help keep songIDs unique also
                var targetSongID = Path.GetFileNameWithoutExtension(provider.SourceName);
                if (targetSongID.Contains("."))
                {
                    targetSongID = targetSongID.Substring(0, targetSongID.LastIndexOf("."));
                }

                //checking this first to maintain compability with song IDs that are just <songid> and are not <songid>_<name> - <author>
                var targetOutputDir = _qaeConfig.SongsPath.CombineFwdSlash(targetSongID);
                if (_qaeConfig.RootFileProvider.FileExists(targetOutputDir.CombineFwdSlash("info.dat")))
                {
                    Log.LogMsg($"ImportManager skipping extract because {targetOutputDir} already exists and has an info.dat.");
                    return targetOutputDir;
                }

                var allFiles = provider.FindFiles("*");
                var firstInfoDat = allFiles.FirstOrDefault(x => x.ToLower() == "info.dat");
                if (firstInfoDat == null)
                {
                    throw new ImportException($"Unable to find info.dat in provider {provider.SourceName}", $"Zip file {provider.SourceName} doesn't seem to be a song (no info.dat).");
                }

                //deserialize the info to get song name and stuff
                try
                {
                    var infoStr = provider.ReadToString(firstInfoDat);
                    var bml = JsonConvert.DeserializeObject<BeatmapLevelDataObject>(infoStr);
                    if (bml == null)
                        throw new Exception("Info.dat returned null from deserializer.");
                    if (bml.SongName == null)
                        throw new Exception("Info.dat deserialized with a null SongName.");
                    var songName = $"{bml.SongName} - {bml.SongAuthorName}";
                    songName = Regex.Replace(songName, "[^a-zA-Z0-9 -]", "");
                    targetSongID = $"{targetSongID}_{songName}";
                }
                catch (Exception ex)
                {
                    Log.LogErr($"Deserializing song info for file {provider.SourceName} failed", ex);
                    throw new ImportException($"Deserializing song info for file {provider.SourceName} failed", $"Info.dat file in zip {provider.SourceName} doesn't appear to be a valid song.", ex);
                }

                targetOutputDir = _qaeConfig.SongsPath.CombineFwdSlash(targetSongID);

                if (_qaeConfig.RootFileProvider.FileExists(targetOutputDir.CombineFwdSlash("info.dat")))
                {
                    Log.LogMsg($"ImportManager skipping extract because {targetOutputDir} already exists and has an info.dat.");
                    return targetOutputDir;
                }
                if (!_qaeConfig.RootFileProvider.DirectoryExists(targetOutputDir))
                    _qaeConfig.RootFileProvider.MkDir(targetOutputDir);

                //get the path of where the info.dat is and assume that the rest of the files will be in the same directory as it.  
                //  This is to handle zips that have duplicate info or nested folders
                var infoDatPath = firstInfoDat.GetDirectoryFwdSlash();
                foreach (var file in allFiles)
                {
                    try
                    {
                        if (Path.GetExtension(file).ToLower() == "zip")
                        {
                            Log.LogMsg($"Skipped {file} because it looks like a nested zip file.");
                            continue;
                        }
                        //if the file isn't in the same path as the located info.dat, skip it
                        if (file.GetDirectoryFwdSlash() != infoDatPath)
                        {
                            Log.LogMsg($"Skipped zip file {file} because it wasn't in the path with info.dat at {infoDatPath}");
                            continue;
                        }

                        var targetFile = targetOutputDir.CombineFwdSlash(file.GetFilenameFwdSlash());

                        using (Stream fs = _qaeConfig.RootFileProvider.GetWriteStream(targetFile))
                        {
                            using (Stream rs = provider.GetReadStream(file, true))
                            {
                                rs.CopyTo(fs);
                            }
                            //TODO: this won't work with zip file provider on the root because the stream has to stay open until Save()
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr($"Exception extracting song file {file} from provider {provider.SourceName}", ex);
                        throw new ImportException($"Exception extracting song file {file} from provider {provider.SourceName}", $"Unable to import song from {provider.SourceName}!  Extracting files failed!", ex);
                    }
                }
                return targetOutputDir;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception importing song from provider {provider.SourceName}", ex);
                throw new ImportException($"Exception importing song from provider {provider.SourceName}", $"Error importing song from {provider.SourceName}", ex);
            }
        }
        private object _modInstallLock = new object();
        /// <summary>
        /// Extracts a mod from a provider and returns the path RELATIVE TO THE BEATONDATAROOT
        /// </summary>
        private void ExtractAndInstallMod(IFileProvider provider)
        {
            lock (_modInstallLock)
            {
                try
                {
                    var def = _getEngine().ModManager.LoadDefinitionFromProvider(provider);

                    if (def.Platform != "Quest")
                    {
                        Log.LogErr($"Attempted to load a mod for a different platform, '{def.Platform ?? "(null)"}', only 'Quest' is supported");
                        _showToast("Incompatible Mod", "This mod is not supported on the Quest.", ToastType.Error, 5);
                        return;
                    }
                    var modOutputPath = _qaeConfig.ModsSourcePath.CombineFwdSlash(def.ID);

                    if (_qaeConfig.RootFileProvider.DirectoryExists(modOutputPath))
                    {
                        Log.LogMsg($"Installing mod ID {def.ID} but it seems to exist.  Deleting the existing mod folder.");
                        _qaeConfig.RootFileProvider.RmRfDir(modOutputPath);
                    }

                    _qaeConfig.RootFileProvider.MkDir(modOutputPath);

                    var allFiles = provider.FindFiles("*");
                    foreach (var file in allFiles)
                    {
                        try
                        {
                            var targetFile = modOutputPath.CombineFwdSlash(file);
                            var dir = targetFile.GetDirectoryFwdSlash();
                            if (!_qaeConfig.RootFileProvider.DirectoryExists(dir))
                                _qaeConfig.RootFileProvider.MkDir(dir);

                            using (Stream fs = _qaeConfig.RootFileProvider.GetWriteStream(targetFile))
                            {
                                using (Stream rs = provider.GetReadStream(file, true))
                                {
                                    rs.CopyTo(fs);
                                }
                                //TODO: this won't work with zip file provider on the root because the stream has to stay open until Save()
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogErr($"Exception extracting {file} from provider {provider.SourceName} on mod ID {def.ID}", ex);
                            throw new ImportException($"Exception extracting {file} from provider {provider.SourceName}", $"Unable to extract files from mod file {provider.SourceName}!", ex);
                        }
                    }
                    var eng = _getEngine();
                    eng.ModManager.ModAdded(def);
                    _getEngine().ModManager.ResetCache();
                    QueueModInstall(def);

                }
                catch (Exception ex)
                {
                    Log.LogErr($"Exception trying to load mod from provider {provider.SourceName}", ex);
                    throw new ImportException($"Exception trying to load mod from provider {provider.SourceName}", $"Unable to load mod from {provider.SourceName}, it does not appear to be a valid mod file.", ex);
                }
            }
        }

        /// <summary>
        /// Queues an operation for adding a song to a playlist
        /// </summary>
        /// <param name="songID">The song ID to use for the imported song</param>
        /// <param name="songPath">The path, RELATIVE TO THE BEATONDATA ROOT, of where the custom song exists</param>
        /// <param name="playlist">The playlist to add the song to</param>
        private AssetOp QueueAddSongToPlaylistOp(string songID, string songPath, BeatSaberPlaylist playlist, Action completionCallback = null, bool suppressToast = false)
        {
            var qae = _getEngine();
            var bsSong = new BeatSaberSong()
            {
                SongID = songID, //ref: was Path.GetFileName(toInst.DownloadPath),
                CustomSongPath = songPath //ref: was toInst.DownloadPath
            };
            var addOp = new AddNewSongToPlaylistOp(bsSong, playlist.PlaylistID);

            addOp.OpFinished += (s, op) =>
            {
                //TODO: i'd like for this to come back out of the config rather than being added here
                if (!playlist.SongList.Any(x=> x.SongID == bsSong.SongID))                
                    playlist.SongList.Add(bsSong);
                if (op.Status == OpStatus.Complete)
                {
                    if (!suppressToast)
                        _showToast($"Song Added", $"{songID} was downloaded and added successfully", ClientModels.ToastType.Success);
                }
                else if (op.Status == OpStatus.Failed)
                {
                    if (op.Exception as AddSongException != null)
                    {
                        var ex = op.Exception as AddSongException;
                        if (ex.FailType == AddSongFailType.SongExists)
                        {
                            //don't show toast for a song already existing
                        }
                        else if (ex.FailType == AddSongFailType.InvalidFormat)
                        {
                            if (!suppressToast)
                                _showToast($"Song Invalid", $"{songID} failed to import, it wasn't in a valid format.", ClientModels.ToastType.Error);
                        }
                        else
                        {
                            if (!suppressToast)
                                _showToast($"Song Failed", $"{songID} failed to import!", ClientModels.ToastType.Error);
                        }
                    }
                    else
                    {
                        if (!suppressToast)
                            _showToast($"Song Failed", $"{songID} failed to import!", ClientModels.ToastType.Error);
                    }
                }
                completionCallback?.Invoke();
            };
            qae.OpManager.QueueOp(addOp);
            return addOp;
        }

        /// <summary>
        /// Installs a mod, or queues its operations.
        /// </summary>
        /// <param name="modPath">The mod path RELATIVE TO THE BEATONDATAROOT</param>
        private void QueueModInstall(ModDefinition modDefinition)
        {
            try
            {
                var ops = _getEngine().ModManager.GetInstallModOps(modDefinition);
                if (ops.Count > 0)
                {
                    ops.Last().OpFinished += (s,e)=>
                    {
                        if (e.Status == OpStatus.Complete)
                        {
                            _showToast($"Mod Installed", $"{modDefinition.Name} was installed and activated", ClientModels.ToastType.Success);
                        }
                    };
                }
                ops.ForEach(x => _getEngine().OpManager.QueueOp(x));
                ops.WaitForFinish();
                var cfg = _getEngine().GetCurrentConfig();
                _getConfig().Config = cfg;
                _triggerConfigChanged();

            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception in import manager installing mod ID {modDefinition.ID}", ex);
                throw new ImportException($"Exception in import manager installing mod ID {modDefinition.ID}", $"Unable to install mod ID {modDefinition.ID}!", ex);
            }
        }

        private BeatSaberPlaylist GetOrCreateDefaultPlaylist()
        {
            var currentConfig = _getConfig();
            var qae = _getEngine();
            var pl = currentConfig.Config.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");
            if (pl == null)
            {
                pl = new BeatSaberPlaylist()
                {
                    PlaylistID = "CustomSongs",
                    PlaylistName = "Custom Songs"
                };
                var addPlOp = new AddOrUpdatePlaylistOp(pl);
                qae.OpManager.QueueOp(addPlOp);
                addPlOp.FinishedEvent.WaitOne();
                if (addPlOp.Status != OpStatus.Complete)
                {
                    Log.LogErr($"Unable to create CustomSongs playlist!", addPlOp.Exception);
                    throw new ImportException("Could not create CustomSongs playlist, the Op failed.", "Unable to create a Custom Songs playlist, cannot import song!", addPlOp.Exception);
                }
                currentConfig.Config.Playlists.Add(pl);
                return pl;
            }
            return pl;
        }

        private DownloadType IdentifyFileType(IFileProvider provider)
        {
            if (provider.FileExists("beatonmod.json"))
                return DownloadType.ModFile;
            else if (provider.FileExists("info.dat"))
                return DownloadType.SongFile;
            else if (provider.FileExists("info.json"))
                return DownloadType.OldSongFile;
            else
            {
                var files = provider.FindFiles("*");
                //check if all of the files are .json files, and guess that maybe it's a playlist or two
                if (!files.Any(x=>!x.ToLower().EndsWith(".json")))
                {
                    //going to guess maybe this is a playlist, becuase there aren't any other options right now
                    return DownloadType.Playlist;
                }
            }

            return DownloadType.Unknown;
        }
    }
}