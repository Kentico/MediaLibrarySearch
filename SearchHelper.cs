using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.Search;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaLibrarySearchIndex
{
    internal class SearchHelper
    {
        private const string SEPARATOR = " ";
        public static readonly string INDEX_TYPE = typeof(SearchIndex).FullName.Replace(".","_");
        public static ISearchDocument GetSearchDocument(MediaFileInfo fileInfo, SearchIndexInfo indexInfo)
        {
            var contentBuilder = new StringBuilder();

            contentBuilder.Append(GetContent(fileInfo) + SEPARATOR);

            var doc = CMS.Search.SearchHelper.CreateDocument(new SearchDocumentParameters
            {
                Index = indexInfo,
                Type = CMS.Search.SearchHelper.CUSTOM_SEARCH_INDEX,
                Id = fileInfo.FileID.ToString(),
                Created = fileInfo.FileCreatedWhen
            });
            var keywords = fileInfo.GetValue("FileKeywords")?.ToString() ?? string.Empty;
            var mediaLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo(fileInfo.FileLibraryID);

            contentBuilder.Append(fileInfo.FileTitle + SEPARATOR);
            contentBuilder.Append(fileInfo.FileDescription + SEPARATOR);
            contentBuilder.Append(fileInfo.FileCustomData + SEPARATOR);
            contentBuilder.Append(fileInfo.FileName + SEPARATOR);
            contentBuilder.Append(keywords + SEPARATOR);

            CMS.Search.SearchHelper.AddGeneralField(doc, SearchFieldsConstants.CONTENT, HTMLHelper.HtmlToPlainText(contentBuilder.ToString()), false, true);
            CMS.Search.SearchHelper.AddGeneralField(doc, "Keywords", keywords, true, true);

            CMS.Search.SearchHelper.AddGeneralField(doc, SearchFieldsConstants.CUSTOM_TITLE, fileInfo.FileName, true, false, true);
            CMS.Search.SearchHelper.AddGeneralField(doc, SearchFieldsConstants.CUSTOM_DATE, fileInfo.FileCreatedWhen, true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, SearchFieldsConstants.CUSTOM_IMAGEURL, MediaLibraryHelper.GetMediaFileUrl(fileInfo.FileGUID, SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID).SiteName), true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileName), fileInfo.FileName, true, false, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileDescription), fileInfo.FileDescription, true, false, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileGUID), fileInfo.FileGUID, true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileLibraryID), fileInfo.FileLibraryID.ToString(), true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FilePath), fileInfo.FilePath, true, false, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileMimeType), fileInfo.FileMimeType, true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileImageWidth), fileInfo.FileImageWidth.ToString(), true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileImageHeight), fileInfo.FileImageHeight.ToString(), true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaFileInfo.FileSize), fileInfo.FileSize, true, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaLibraryInfo.LibraryDisplayName), mediaLibrary.LibraryDisplayName, true, false, false);
            CMS.Search.SearchHelper.AddGeneralField(doc, nameof(MediaLibraryInfo.LibraryFolder), mediaLibrary.LibraryFolder, true, false, false);

            return doc;
        }

        private static List<string> GetExtractors()
        {
            var extractors = SearchTextExtractorManager.GetSupportedContentExtractors().Replace(" ", "");
            return extractors.Split(',').ToList();
        }
        private static string GetContent(MediaFileInfo mfi)
        {
            if (mfi != null)
                return string.Empty;

            var content = new StringBuilder();
            content.Append(mfi.FileName);
            content.Append(" ");

            try
            {
                // see if we can use system extractor
                var extractors = GetExtractors();
                if (extractors.Contains(mfi.FileExtension.Trim('.').ToLower()))
                {
                    var cachedValueUsed = false;
                    var data = CMS.Search.SearchHelper.GetBinaryDataSearchContent(mfi, new ExtractionContext(), out cachedValueUsed);

                    if (data != null)
                        content.Append(ValidationHelper.GetString(data.GetValue(SearchFieldsConstants.CONTENT), string.Empty));
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaLibrarySearchIndex", "GetBinaryContentAsString", ex);
            }
            
            return content.ToString();
        }

        public static string GetCustomSearchIndexClassName(SearchIndexInfo searchIndex)
        {
            return searchIndex?.IndexSettings?.GetSearchIndexSettingsInfo(CMS.Search.SearchHelper.CUSTOM_INDEX_DATA)?.GetValue("ClassName")?.ToString() ?? string.Empty;
        }
    }
}
