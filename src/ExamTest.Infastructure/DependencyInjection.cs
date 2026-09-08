using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;
using ExamTest.Infastructure.Cloudinary;
using ExamTest.Infastructure.Firebase;
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

            // One Firestore collection per entity type - add a line here for any new entity.
            services.AddScoped<IRepository<Series>>(sp =>
                new FirestoreRepository<Series>(sp.GetRequiredService<FirestoreDb>(), "series"));
            services.AddScoped<IRepository<Episode>>(sp =>
                new FirestoreRepository<Episode>(sp.GetRequiredService<FirestoreDb>(), "episodes"));
            services.AddScoped<IRepository<Genre>>(sp =>
                new FirestoreRepository<Genre>(sp.GetRequiredService<FirestoreDb>(), "genres"));
            services.AddScoped<IRepository<Film>>(sp =>
                new FirestoreRepository<Film>(sp.GetRequiredService<FirestoreDb>(), "films"));
            services.AddScoped<IRepository<Category>>(sp =>
                new FirestoreRepository<Category>(sp.GetRequiredService<FirestoreDb>(), "categories"));
            services.AddScoped<IRepository<Product>>(sp =>
                new FirestoreRepository<Product>(sp.GetRequiredService<FirestoreDb>(), "products"));

            return services;
        }

        public static IServiceCollection AddCloudinaryInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
            services.AddSingleton<CloudinaryImageService>();
            return services;
        }
    }
}
