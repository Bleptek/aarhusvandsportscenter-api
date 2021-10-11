using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Collections.Generic;
using Aarhusvandsportscenter.Api.Infastructure.Database.Entities;
using Aarhusvandsportscenter.Api.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Aarhusvandsportscenter.Api.Infastructure.Database;
using Aarhusvandsportscenter.Api.Controllers.ContentPages;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;

namespace Aarhusvandsportscenter.Api.Tests.Controllers
{
    public class ContentPagesControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ContentPagesControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        #region ContentPage
        [Fact]
        public async Task GetContentPages_EndpointSuccessTest()
        {
            // Arrange
            var contentPages = new List<ContentPageEntity>
            {
                new ContentPageEntity(){
                    Title = "test1",
                    Key = "key-1",
                    Sections = new List<ContentPageSectionEntity>{
                        new ContentPageSectionEntity{
                            Key = "section-key-1",
                            Content = "some content",
                            Title = "section title"
                        }
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                await appDbContext.ContentPages.AddRangeAsync(contentPages);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient();
            var httpResponse = await httpClient.GetAsync($"/api/v1/contentPages");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<IEnumerable<ContentPageResponse>>();
            Assert.NotEmpty(responseObj);
        }

        [Fact]
        public async Task GetContentPage_EndpointSuccessTest()
        {
            // Arrange
            var contentPages = new List<ContentPageEntity>
            {
                new ContentPageEntity(){
                    Title = "test1",
                    Key = "key-1",
                    Sections = new List<ContentPageSectionEntity>{
                        new ContentPageSectionEntity{
                            Key = "section-key-1",
                            Content = "some content",
                            Title = "section title"
                        }
                    },
                    Images = new List<ContentPageImageEntity>{
                        new ContentPageImageEntity("some/path/lol.jpg")
                    }
                },
                new ContentPageEntity(){
                    Title = "test2",
                    Key = "key-2",
                    Sections = new List<ContentPageSectionEntity>{
                        new ContentPageSectionEntity{
                            Key = "section-key-1",
                            Content = "some content",
                            Title = "section title"
                        }
                    },
                    Images = new List<ContentPageImageEntity>{
                        new ContentPageImageEntity("some/path/lol.jpg")
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                await appDbContext.ContentPages.AddRangeAsync(contentPages);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient();
            var httpResponse = await httpClient.GetAsync($"/api/v1/contentPages/{contentPages[1].Key}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageResponse>();
            Assert.NotNull(responseObj);
            Assert.NotEmpty(responseObj.Sections);
            Assert.NotEmpty(responseObj.Images);
            Assert.Equal(contentPages[1].Key, responseObj.Key);
        }

        [Fact]
        public async Task CreateContentPage_EndpointSuccessTest()
        {
            // Arrange
            var request = new CreateContentPageRequest
            {
                Title = "new contentPage",
                Key = "key-1",
                Sections = new List<ContentPageSectionRequest>{
                    new ContentPageSectionRequest{
                        Key = "section-key-1",
                        Content = "some content",
                        Title = "section title"
                    }
                }
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PostAsync($"/api/v1/contentPages", request.ToHttpStringContent());

            // Assert
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageResponse>();

            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            Assert.Equal(request.Key, responseObj.Key);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var createdContentPage = await appDbContext.ContentPages
                    .Include(x => x.Sections)
                    .FirstAsync(x => x.Key == responseObj.Key);
                Assert.Equal(request.Title, createdContentPage.Title);
                Assert.Equal(request.Key, createdContentPage.Key);

                Assert.Single(createdContentPage.Sections);
                Assert.Equal(request.Sections.First().Key, createdContentPage.Sections.First().Key);
                Assert.Equal(request.Sections.First().Title, createdContentPage.Sections.First().Title);
                Assert.Equal(request.Sections.First().Content, createdContentPage.Sections.First().Content);
            }
        }

        [Fact]
        public async Task UpdateContentPage_EndpointSuccessTest()
        {
            // Arrange
            var key = "key-1";
            var contentPage = new ContentPageEntity()
            {
                Title = "test1",
                Key = key,
                Sections = new List<ContentPageSectionEntity>{
                    new ContentPageSectionEntity{
                        Key = "section-key-1",
                        Content = "some content",
                        Title = "section title"
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                await appDbContext.ContentPages.AddAsync(contentPage);
                await appDbContext.SaveChangesAsync();
            }

            var request = new UpdateContentPageRequest
            {
                Title = "new title",
                Sections = new List<ContentPageSectionRequest>{
                    new ContentPageSectionRequest{
                        Key = "section-key-1",
                        Content = "new content",
                        Title = "new title"
                    }
                }
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/contentPages/{key}", request.ToHttpStringContent());

            // Assert
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageResponse>();
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(key, responseObj.Key);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedContentPage = await appDbContext.ContentPages
                    .Include(x => x.Sections)
                    .FirstAsync(x => x.Key == responseObj.Key);
                Assert.Equal(request.Title, updatedContentPage.Title);
                Assert.Equal(key, updatedContentPage.Key); // unchanged

                Assert.Single(updatedContentPage.Sections);
                Assert.Equal(request.Sections.First().Key, updatedContentPage.Sections.First().Key);
                Assert.Equal(request.Sections.First().Title, updatedContentPage.Sections.First().Title);
                Assert.Equal(request.Sections.First().Content, updatedContentPage.Sections.First().Content);
            }
        }

        #endregion

        #region ContentPageImage
        [Fact]
        public async Task UploadContentPageImage_EndpointSuccessTest()
        {
            // Arrange
            var pageKey = "kitesurfing123";
            var contentPage = new ContentPageEntity()
            {
                Title = "test1",
                Key = pageKey
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                await appDbContext.ContentPages.AddAsync(contentPage);
                await appDbContext.SaveChangesAsync();
            }

            using var form = new MultipartFormDataContent();
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"TestFiles/boat-1.jpg");
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", filePath);

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PostAsync($"/api/v1/contentPages/{pageKey}/uploadImage", form);

            // Assert
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageImageResponse>();

            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
            Assert.True(!string.IsNullOrEmpty(responseObj.Path));

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var cpAfterUpdate = await appDbContext.ContentPages
                    .Include(x => x.Images)
                    .FirstAsync(x => x.Key == pageKey);

                Assert.Single(cpAfterUpdate.Images);
            }

            var uploadedFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"wwwroot/{responseObj.Path.Split("localhost/")[1]}");
            Assert.True(File.Exists(uploadedFilePath));
        }
        #endregion

        #region ContentPageSection
        [Fact]
        public async Task GetContentPageSection_EndpointSuccessTest()
        {
            // Arrange
            var sectionKey = "section-key-1";
            var contentPage = new ContentPageEntity()
            {
                Title = "test1",
                Key = "key-1",
                Sections = new List<ContentPageSectionEntity>{
                    new ContentPageSectionEntity{
                        Key = sectionKey,
                        Content = "some content",
                        Title = "section title"
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                appDbContext.ContentPages.Add(contentPage);
                await appDbContext.SaveChangesAsync();
            }

            // Act
            var httpClient = _factory.CreateNewHttpClient(false);
            var httpResponse = await httpClient.GetAsync($"/api/v1/contentPages/{contentPage.Key}/sections/{sectionKey}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageSectionResponse>();
            Assert.NotNull(responseObj);
            Assert.Equal(sectionKey, responseObj.Key);
            Assert.Equal(contentPage.Sections.ElementAt(0).Content, responseObj.Content);
            Assert.Equal(contentPage.Sections.ElementAt(0).Title, responseObj.Title);
        }

        [Fact]
        public async Task UpdateContentPageSection_EndpointSuccessTest()
        {
            // Arrange
            var pageKey = "key-1";
            var sectionKey = "section-key-1";
            var newTitle = "some new title";
            var newContent = "some new content";

            var contentPage = new ContentPageEntity()
            {
                Title = "test1",
                Key = pageKey,
                Sections = new List<ContentPageSectionEntity>{
                    new ContentPageSectionEntity{
                        Key = sectionKey,
                        Content = "some content",
                        Title = "section title"
                    }
                }
            };

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                await appDbContext.ContentPages.AddAsync(contentPage);
                await appDbContext.SaveChangesAsync();
            }

            var request = new ContentPageSectionBaseRequest
            {
                Title = newTitle,
                Content = newContent
            };

            // Act
            var httpClient = _factory.CreateNewHttpClient(true);
            var httpResponse = await httpClient.PutAsync($"/api/v1/contentPages/{pageKey}/sections/{sectionKey}", request.ToHttpStringContent());

            // Assert
            var responseObj = await httpResponse.DeserializeHttpResponse<ContentPageSectionResponse>();
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.Equal(sectionKey, responseObj.Key);

            using (var appDbContext = _factory.GetScopedServiceProvider().GetService<AppDbContext>())
            {
                var updatedSection = await appDbContext.ContentPageSections
                    .FirstAsync(x => x.ContentPage.Key == pageKey && x.Key == sectionKey);
                Assert.Equal(request.Title, updatedSection.Title);
                Assert.Equal(request.Content, updatedSection.Content);
                Assert.Equal(sectionKey, updatedSection.Key); // unchanged
            }
        }
        #endregion
    }
}