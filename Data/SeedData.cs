using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_eschwalbach.Models;
using Fall2024_Assignment3_eschwalbach.Data;

namespace Fall2024_Assignment3_eschwalbach.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
        {
            // Check if DB has been seeded
            if (context.Movies.Any())
            {
                return;
            }

            // Seed the database with movies
            context.Movies.AddRange(
                // 1
                new Movie
                {
                    Title = "2001: A Space Odyssey",
                    Imdb = "https://www.imdb.com/title/tt0062622/",
                    Genre = "Sci-fi/Adventure",
                    Year = 1968,
                    Poster = "https://m.media-amazon.com/images/M/MV5BNjU0NDFkMTQtZWY5OS00MmZhLTg3Y2QtZmJhMzMzMWYyYjc2XkEyXkFqcGc@._V1_QL75_UX190_CR0,0,190,281_.jpg"
                },
                // 2
                new Movie
                {
                    Title = "Inception",
                    Imdb = "https://www.imdb.com/title/tt1375666/",
                    Genre = "Sci-fi/Action",
                    Year = 2010,
                    Poster = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_.jpg"
                },
                // 3
                new Movie
                {
                    Title = "Fight Club",
                    Imdb = "https://www.imdb.com/title/tt0137523/",
                    Genre = "Action/Thriller",
                    Year = 1999,
                    Poster = "https://m.media-amazon.com/images/M/MV5BOTgyOGQ1NDItNGU3Ny00MjU3LTg2YWEtNmEyYjBiMjI1Y2M5XkEyXkFqcGc@._V1_.jpg"
                },
                // 4
                new Movie
                {
                    Title = "The Shawshank Redemption",
                    Imdb = "https://www.imdb.com/title/tt0111161/",
                    Genre = "Thriller/Crime",
                    Year = 1994,
                    Poster = "https://m.media-amazon.com/images/M/MV5BMDAyY2FhYjctNDc5OS00MDNlLThiMGUtY2UxYWVkNGY2ZjljXkEyXkFqcGc@._V1_.jpg"
                }
            );
            context.SaveChanges();

            // Seed the database with actors
            context.Actors.AddRange(
                // 1
                new Actor
                {
                    Name = "Morgan Freeman",
                    Gender = "Male",
                    Age = 87,
                    Imdb = "https://www.imdb.com/name/nm0000151/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMTc0MDMyMzI2OF5BMl5BanBnXkFtZTcwMzM2OTk1MQ@@._V1_FMjpg_UX1000_.jpg"
                },
                // 2
                new Actor
                {
                    Name = "Christian Bale",
                    Gender = "Male",
                    Age = 50,
                    Imdb = "https://www.imdb.com/name/nm0000288/?ref_=fn_al_nm_1",
                    Photo = "https://upload.wikimedia.org/wikipedia/commons/0/0a/Christian_Bale-7837.jpg"
                },
                // 3
                new Actor
                {
                    Name = "Brad Pitt",
                    Gender = "Male",
                    Age = 60,
                    Imdb = "https://www.imdb.com/name/nm0000093/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMjA1MjE2MTQ2MV5BMl5BanBnXkFtZTcwMjE5MDY0Nw@@._V1_FMjpg_UX1000_.jpg"
                },
                // 4
                new Actor
                {
                    Name = "Tim Robbins",
                    Gender = "Male",
                    Age = 66,
                    Imdb = "https://www.imdb.com/name/nm0000209/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMTI1OTYxNzAxOF5BMl5BanBnXkFtZTYwNTE5ODI4._V1_FMjpg_UX1000_.jpg"
                },
                // 5
                new Actor
                {
                    Name = "Leonardo DiCaprio",
                    Gender = "Male",
                    Age = 49,
                    Imdb = "https://www.imdb.com/name/nm0000138/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMjI0MTg3MzI0M15BMl5BanBnXkFtZTcwMzQyODU2Mw@@._V1_FMjpg_UX1000_.jpg"
                },
                // 6
                new Actor
                {
                    Name = "Edward Norton",
                    Gender = "Male",
                    Age = 55,
                    Imdb = "https://www.imdb.com/name/nm0001570/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMTYwNjQ5MTI1NF5BMl5BanBnXkFtZTcwMzU5MTI2Mw@@._V1_FMjpg_UX1000_.jpg"
                },
                // 7
                new Actor
                {
                    Name = "Keir Dullea",
                    Gender = "Male",
                    Age = 88,
                    Imdb = "https://www.imdb.com/name/nm0001158/",
                    Photo = "https://m.media-amazon.com/images/M/MV5BMTczODgxNDU1NV5BMl5BanBnXkFtZTYwMzQxOTI2._V1_.jpg"
                },
                // 8
                new Actor
                {
                    Name = "Cillian Murphy",
                    Gender = "Male",
                    Age = 48,
                    Imdb = "https://www.imdb.com/name/nm0614165/",
                    Photo = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTyUB0NhvWx6tnPahAEZZSiiB53AJimydLueA&s"
                }
       
            );
            context.SaveChanges();


            context.MovieActors.AddRange(

                new MovieActor { MovieId = 1, ActorId = 7 },

                new MovieActor { MovieId = 2, ActorId = 5 },  
                new MovieActor { MovieId = 2, ActorId = 8 },  

                new MovieActor { MovieId = 3, ActorId = 3 },  
                new MovieActor { MovieId = 3, ActorId = 6 },  


                new MovieActor { MovieId = 4, ActorId = 1 },
                new MovieActor { MovieId = 4, ActorId = 4 } 

            );
            context.SaveChanges();
        }
    }
}
