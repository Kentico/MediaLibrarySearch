using CMS.DataEngine;
using CMS.MediaLibrary;
using CMS.Scheduler;
using CMS.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaLibrarySearchIndex
{
    public class SearchTaskProcessor : ITask
    {
        public string Execute(TaskInfo task)
        {
            var customSearchIndexes = SearchIndexInfoProvider.GetSearchIndexes()
                .WhereEquals(nameof(SearchIndexInfo.IndexType), CMS.Search.SearchHelper.CUSTOM_SEARCH_INDEX);

            var searchIndex = customSearchIndexes.SingleOrDefault(i => SearchHelper.GetCustomSearchIndexClassName(i) == typeof(SearchIndex).FullName);

            if (searchIndex == null)
                return $"Search index of type {SearchHelper.INDEX_TYPE} was not found.";

            var provider = SearchTaskInfoProvider.GetSearchTasks()
                .WhereEquals(nameof(SearchTaskInfo.SearchTaskObjectType), SearchHelper.INDEX_TYPE)
                .WhereEquals(nameof(SearchTaskInfo.SearchTaskStatus), SearchTaskStatusEnum.Ready.ToString())
                .WhereEquals(nameof(SearchTaskInfo.SearchTaskType), SearchTaskTypeEnum.Update.ToString());

            if (searchIndex.IndexBatchSize > 0)
                provider = provider.TopN(searchIndex.IndexBatchSize);

            var lastId = 0;
            List<SearchTaskInfo> searchTasks;
            List<ISearchDocument> searchDocuments = new List<ISearchDocument>();
            while ((searchTasks = provider.WhereGreaterThan(nameof(SearchTaskInfo.SearchTaskID), lastId).OrderBy(nameof(SearchTaskInfo.SearchTaskID)).ToList()).Any())
            {
                var mediaFilesIds = searchTasks.Select(t => Convert.ToInt32(t.SearchTaskValue)).ToList();
                var mediaFiles = MediaFileInfoProvider.GetMediaFiles().WhereIn(nameof(MediaFileInfo.FileID), mediaFilesIds);
                foreach (var searchTask in searchTasks)
                {
                    searchDocuments.Add(SearchHelper.GetSearchDocument(mediaFiles.Single(f => f.FileID == Convert.ToInt32(searchTask.SearchTaskValue)), searchIndex));
                }
                
                lastId = searchTasks.Last().SearchTaskID;
                SearchTaskInfoProvider.ProviderObject.BulkDelete(new WhereCondition(SqlHelper.GetWhereInCondition(nameof(SearchTaskInfo.SearchTaskID), searchTasks.Select(s => s.SearchTaskID).ToList(), false, false)));
            }

            searchDocuments.ForEach(doc => CMS.Search.SearchHelper.Update(doc, searchIndex));
            return $"Processed {searchDocuments.Count} files.";
        }
    }
}
