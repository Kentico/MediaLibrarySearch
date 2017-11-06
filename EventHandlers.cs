using CMS;
using CMS.DataEngine;
using CMS.MediaLibrary;
using CMS.Search;
using MediaLibrarySearchIndex;

[assembly: RegisterModule(typeof(EventHandlers))]

namespace MediaLibrarySearchIndex
{
    public class EventHandlers : Module
    {
        public EventHandlers() : base(nameof(MediaLibrarySearchIndex) + nameof(EventHandlers))
        {
        }
        protected override void OnInit()
        {
            base.OnInit();

            MediaFileInfo.TYPEINFO.Events.Delete.After += Delete_After;

            MediaFileInfo.TYPEINFO.Events.Insert.After += Update_After;
            MediaFileInfo.TYPEINFO.Events.Update.After += Update_After;
        }

        private void Delete_After(object sender, ObjectEventArgs e)
        {
            if (!SearchIndexInfoProvider.SearchEnabled || !SearchIndexInfoProvider.SearchTypeEnabled(CMS.Search.SearchHelper.CUSTOM_SEARCH_INDEX))
                return;

            var fileInfo = e.Object as MediaFileInfo;
            if (fileInfo == null)
                return;

            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, CMS.Search.SearchHelper.CUSTOM_SEARCH_INDEX, SearchFieldsConstants.ID, fileInfo.FileID.ToString(), 0);
        }

        private void Update_After(object sender, ObjectEventArgs e)
        {
            if (!SearchIndexInfoProvider.SearchEnabled || !SearchIndexInfoProvider.SearchTypeEnabled(CMS.Search.SearchHelper.CUSTOM_SEARCH_INDEX))
                return;

            var fileInfo = e.Object as MediaFileInfo;
            if (fileInfo == null)
                return;

            var taskInfo = new SearchTaskInfo
            {
                SearchTaskObjectType = SearchHelper.INDEX_TYPE,
                SearchTaskValue = fileInfo.FileID.ToString(),
                SearchTaskPriority = 0,
                SearchTaskType = SearchTaskTypeEnum.Update,
                SearchTaskStatus = SearchTaskStatusEnum.Ready
            };

            SearchTaskInfoProvider.SetSearchTaskInfo(taskInfo);
        }
    }
}
