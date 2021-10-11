namespace Aarhusvandsportscenter.Api.Infastructure.Database.Entities
{
    public class ContentPageSectionEntity : BaseEntity
    {
        public ContentPageSectionEntity()
        {
        }

        public ContentPageSectionEntity(string key, string title, string content)
        {
            Key = key;
            Title = title;
            Content = content;
        }


        /// <summary>
        /// must be unique for the content page
        /// </summary>
        public string Key { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int ContentPageId { get; set; }
        public ContentPageEntity ContentPage { get; set; }

        public void MapFrom(ContentPageSectionEntity mapFrom)
        {
            Title = mapFrom.Title;
            Content = mapFrom.Content;
        }
    }
}