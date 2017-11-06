
using System;
using CMS.Search;
using CMS.MediaLibrary;
using System.Collections.Generic;
using System.Linq;
using CMS.DataEngine;
using CMS.EventLog;
using MediaLibrarySearchIndex;
using CMS;

namespace MediaLibrarySearchIndex
{
    public class SearchIndex : ICustomSearchIndex
    {
        public void Rebuild(SearchIndexInfo srchInfo)
        {
            if (srchInfo == null || srchInfo.IndexSettings == null)
                return;

            var iw = srchInfo.Provider.GetWriter(true);
            if (iw == null)
                return;

            try
            {
                var provider = MediaFileInfoProvider.GetMediaFiles();
                if (srchInfo.IndexBatchSize > 0)
                    provider = provider.TopN(srchInfo.IndexBatchSize);

                var lastId = 0;
                List<MediaFileInfo> files;
                List<ISearchDocument> searchDocuments = new List<ISearchDocument>();
                while ((files = provider.WhereGreaterThan(nameof(MediaFileInfo.FileID), lastId).OrderBy(nameof(MediaFileInfo.FileID)).ToList()).Any())
                {
                    files.ForEach(file => searchDocuments.Add(SearchHelper.GetSearchDocument(file, srchInfo)));
                    lastId = files.Last().FileID;
                }

                searchDocuments.ForEach(doc => iw.AddDocument(doc));
                iw.Flush();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Smart search", "rebuild", ex);
                srchInfo.IndexStatus = IndexStatusEnum.ERROR;
            }
            finally
            {
                iw.Close();
            }
        }
    }
}
