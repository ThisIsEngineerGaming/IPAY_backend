using ExamTest.Domain.Entities.Auth;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;
using ExamTest.Domain.Interfaces.ForRepos.Shop;
using ExamTest.Infastructure.Cloudinary;
using ExamTest.Infastructure.Firebase;
using ExamTest.Infastructure.Firebase.Documents;
using ExamTest.Infastructure.Repositories.Shop;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExamTest.Infastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFirebaseInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<FirebaseOptions>>().Value;
                var credential = GoogleCredential.FromFile(options.ServiceAccountKeyPath);
                return new FirestoreDbBuilder
                {
                    ProjectId = options.ProjectId,
                    GoogleCredential = credential
                }.Build();
            });

            // Image hosting - Cloudinary (replaced Firebase Storage).
            services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
            services.AddSingleton<CloudinaryImageService>();

            // One Firestore collection per entity type - add a line here for any new entity
            // (plus a matching *Document class in Firebase/Documents).
            services.AddDocumentRepository<Series, SeriesDocument>(
                "series", SeriesDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Episode, EpisodeDocument>(
                "episodes", EpisodeDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Genre, GenreDocument>(
                "genres", GenreDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Film, FilmDocument>(
                "films", FilmDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Category, CategoryDocument>(
                "categories", CategoryDocument.FromEntity, document => document.ToEntity());
            services.AddDocumentRepository<Seller, SellerDocument>(
                "sellers", SellerDocument.FromEntity, document => document.ToEntity());

            services.AddScoped<IProductRepo>(sp =>
              new ProductRepo(
                  sp.GetRequiredService<FirestoreDb>(),
                  "products"));

            return services;
        }

        private static IServiceCollection AddDocumentRepository<TEntity, TDocument>(
            this IServiceCollection services,
            string collectionName,
            Func<TEntity, TDocument> toDocument,
            Func<TDocument, TEntity> toEntity)
            where TEntity : class
            where TDocument : class, new()
        {
            return services.AddScoped<IRepository<TEntity>>(sp =>
                new FirestoreRepository<TEntity, TDocument>(
                    sp.GetRequiredService<FirestoreDb>(),
                    collectionName,
                    toDocument,
                    toEntity));
        }
    }
}
