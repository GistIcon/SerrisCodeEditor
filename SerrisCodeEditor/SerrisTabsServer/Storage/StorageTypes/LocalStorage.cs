﻿using Microsoft.Toolkit.Uwp.Helpers;
using SerrisModulesServer.Type.ProgrammingLanguage;
using SerrisTabsServer.Items;
using SerrisTabsServer.Manager;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SerrisTabsServer.Storage.StorageTypes
{
    public class LocalStorage : StorageType
    {
        public LocalStorage(InfosTab tab, int _ListTabsID) : base(tab, _ListTabsID)
        {
            Tab = tab; ListTabsID = _ListTabsID;
        }

        public async Task<bool> CreateFile()
        {
            bool result = false;

            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                var folderPicker = new FolderPicker();
                StorageFolder folder;
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    StorageFile file = await folder.CreateFileAsync(Tab.TabName, CreationCollisionOption.OpenIfExists);
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    Windows.Storage.FileProperties.BasicProperties date = await file.GetBasicPropertiesAsync();

                    Tab.TabDateModified = date.DateModified.ToString();
                    Tab.TabType = LanguagesHelper.GetLanguageType(file.FileType);
                    Tab.TabOriginalPathContent = file.Path;

                    await TabsWriteManager.PushUpdateTabAsync(Tab, ListTabsID, true);

                    result = true;
                }

            });

            return result;
        }

        public async void DeleteFile()
        {
            StorageFile file = AsyncHelpers.RunSync(() => StorageFile.GetFileFromPathAsync(Tab.TabOriginalPathContent).AsTask());
            await file.DeleteAsync();
            Tab.TabStorageMode = StorageListTypes.Nothing;
            Tab.TabOriginalPathContent = "";
            await TabsWriteManager.PushUpdateTabAsync(Tab, ListTabsID, false);
        }

        public async Task<bool> ReadFile(bool ReplaceEncoding)
        {
            StorageFile file = AsyncHelpers.RunSync(() => StorageFile.GetFileFromPathAsync(Tab.TabOriginalPathContent).AsTask());
            string encode_type = "";

            await Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(Tab.TabOriginalPathContent))
                {
                    var cdet = new Ude.CharsetDetector();
                    cdet.Feed(fs);
                    cdet.DataEnd();
                    if (cdet.Charset != null)
                    {
                        encode_type = cdet.Charset;
                    }
                }
            });

            if (encode_type == "")
                encode_type = "utf-8";

            using (var st = new StreamReader(await file.OpenStreamForReadAsync(), Encoding.GetEncoding(encode_type)))
            {
                await TabsWriteManager.PushTabContentViaIDAsync(new TabID { ID_Tab = Tab.ID, ID_TabsList = ListTabsID }, st.ReadToEnd(), true);

                if (ReplaceEncoding)
                {
                    Tab.TabEncoding = Encoding.GetEncoding(encode_type).CodePage;
                    await TabsWriteManager.PushUpdateTabAsync(Tab, ListTabsID, true);
                }

                st.Dispose();
            }

            return true;
        }

        public async Task<string> ReadFileAndGetContent()
        {
            StorageFile file = AsyncHelpers.RunSync(() => StorageFile.GetFileFromPathAsync(Tab.TabOriginalPathContent).AsTask());
            string encode_type = "", content = "";

            await Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(Tab.TabOriginalPathContent))
                {
                    var cdet = new Ude.CharsetDetector();
                    cdet.Feed(fs);
                    cdet.DataEnd();
                    if (cdet.Charset != null)
                    {
                        encode_type = cdet.Charset;
                    }
                }
            });

            using (var st = new StreamReader(await file.OpenStreamForReadAsync(), Encoding.GetEncoding(encode_type)))
            {
                content = st.ReadToEnd();
                st.Dispose();
                return content;
            }
        }

        public async Task WriteFile()
        {

            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(Tab.TabOriginalPathContent);

                    if (file != null)
                    {
                        await FileIO.WriteTextAsync(file, string.Empty);
                        using (var rd = new StreamWriter(await file.OpenStreamForWriteAsync(), Encoding.GetEncoding(Tab.TabEncoding)))
                        {
                            rd.Write(await TabsAccessManager.GetTabContentViaIDAsync(new TabID { ID_Tab = Tab.ID, ID_TabsList = ListTabsID }));
                            rd.Flush(); rd.Dispose();
                        }
                    }

                }
                catch
                {
                    await CreateFile().ContinueWith(async (e) => 
                    {
                        if(e.Result)
                        {
                            await WriteFile();
                        }
                    });
                }

            });

        }

    }
}
