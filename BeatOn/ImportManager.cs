using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using QuestomAssets.AssetOps;
using QuestomAssets.AssetsChanger;
using QuestomAssets.Models;
using QuestomAssets.Mods;

namespace BeatOn
{
    public class ImportManager
    {
        private Func<QuestomAssetsEngine> _getEngine;
        private Func<BeatSaberQuestomConfig> _getConfig;
        private QaeConfig _qaeConfig;
        private ShowToastDelegate _showToast;
        public ImportManager(QaeConfig qaeConfig, Func<BeatSaberQuestomConfig> getConfig, Func<QuestomAssetsEngine> getEngine, ShowToastDelegate showToast)
        {
            _qaeConfig = qaeConfig;
            _getConfig = getConfig;
            _getEngine = getEngine;
            _showToast = showToast;
        }

        public void ImportFromFileProvider(IFileProvider provider)
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
                        break;
                    case DownloadType.SongFile:
                        ImportSongFile(provider);
                        break;
                    case DownloadType.OldSongFile:
                    default:
                        throw new ImportException($"Unhandled file type {type} with {provider.SourceName}", $"File {provider.SourceName} isn't a known type that Beat On can use!");
                }
                _showToast("File Processed", $"The file {provider.SourceName} has been proccessed.", ClientModels.ToastType.Info, 2);
            }
            catch (ImportException iex)
            {
                _showToast("File Failed to Process!", $"The file {provider.SourceName} could not be processed: {iex.FriendlyMessage}", ClientModels.ToastType.Error, 5);
                throw;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Unhandled exception importing file from provider {provider.SourceName}", ex);
                _showToast("File Failed to Process!", $"The file {provider.SourceName} could not be processed!", ClientModels.ToastType.Error, 5);
                throw new ImportException($"Unhandled exception importing file from provider {provider.SourceName}", $"Unable to load file {provider.SourceName}!", ex);
            }
        }

        private void ImportSongFile(IFileProvider provider)
        {
            try
            {
                var songPath = ExtractSongGetPath(provider);
                var songID = songPath.GetFilenameFwdSlash();
                var playlist = GetOrCreateDefaultPlaylist();
                QueueAddSongToPlaylistOp(songID, songPath, playlist);
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
                var modPath = ExtractModGetPath(provider);
                QueueOrInstallMod(modPath);
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
                var targetSongID = provider.SourceName;
                if (targetSongID.Contains("."))
                {
                    targetSongID = targetSongID.Substring(0, targetSongID.LastIndexOf("."));
                }

                var targetOutputDir = _qaeConfig.SongsPath.CombineFwdSlash(targetSongID);

                if (!_qaeConfig.RootFileProvider.DirectoryExists(targetOutputDir))
                    _qaeConfig.RootFileProvider.MkDir(targetOutputDir);

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

        /// <summary>
        /// Extracts a mod from a provider and returns the path RELATIVE TO THE BEATONDATAROOT
        /// </summary>
        private string ExtractModGetPath(IFileProvider provider)
        {
            try
            {
                var def = ModDefinition.LoadDefinitionFromProvider(provider);
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
                return modOutputPath;
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception trying to load mod from provider {provider.SourceName}", ex);
                throw new ImportException($"Exception trying to load mod from provider {provider.SourceName}", $"Unable to load mod from {provider.SourceName}, it does not appear to be a valid mod file.", ex);
            }            
        }

        /// <summary>
        /// Queues an operation for adding a song to a playlist
        /// </summary>
        /// <param name="songID">The song ID to use for the imported song</param>
        /// <param name="songPath">The path, RELATIVE TO THE BEATONDATA ROOT, of where the custom song exists</param>
        /// <param name="playlist">The playlist to add the song to</param>
        private void QueueAddSongToPlaylistOp(string songID, string songPath, BeatSaberPlaylist playlist)
        {
            var qae = _getEngine();
            var bsSong = new BeatSaberSong()
            {
                SongID = songID, //ref: was Path.GetFileName(toInst.DownloadPath),
                CustomSongPath = songPath //ref: was toInst.DownloadPath
            };
            var addOp = new AddNewSongToPlaylistOp(bsSong, playlist.PlaylistID);

            //addOp.OpFinished += (s, op) =>
            //{
            //    //TODO: i'd like for this to come back out of the config rather than being added here
            //    //playlist.SongList.Add(bsSong);

            //    //if (op.Status == OpStatus.Complete)
            //    //    toInst.SetStatus(DownloadStatus.Installed);
            //    //else
            //    //    toInst.SetStatus(DownloadStatus.Failed);
            //};
            qae.OpManager.QueueOp(addOp);
        }

        /// <summary>
        /// Installs a mod, or queues its operations.
        /// </summary>
        /// <param name="modPath">The mod path RELATIVE TO THE BEATONDATAROOT</param>
        private void QueueOrInstallMod(string modPath)
        {
            try
            {
                ModDefinition.InstallModFromDirectory(modPath, _qaeConfig, _getEngine);
            }
            catch (Exception ex)
            {
                Log.LogErr($"Exception in import manager installing mod from {modPath}", ex);
                throw new ImportException($"Exception in import manager installing mod from {modPath}", $"Unable to install mod from {modPath}!", ex);
            }
        }

        private BeatSaberPlaylist GetOrCreateDefaultPlaylist()
        {
            var currentConfig = _getConfig();
            var qae = _getEngine();
            var pl = currentConfig.Playlists.FirstOrDefault(x => x.PlaylistID == "CustomSongs");
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
                currentConfig.Playlists.Add(pl);
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
                return DownloadType.Unknown;
        }
    }
}