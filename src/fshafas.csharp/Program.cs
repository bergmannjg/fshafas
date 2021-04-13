using System;
using System.Threading.Tasks;
using FsHafas.Client;
using FsHafas.Api;
using FsHafas.Profiles;

namespace fshafas.csharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HafasAsyncClient.initSerializer();

            using (var client = new HafasAsyncClient(ProfileId.Db))
            {
                var locations = await HafasAsyncClient.toTask(client.AsyncLocations("Bielefeld", Default.LocationsOptions));

                Console.WriteLine("Locations found: " + locations.Length);

                Console.WriteLine(FsHafas.Printf.Short.Locations(locations));

                /*
                foreach (var l in locations)
                {
                    switch (l)
                    {
                        case U3<Station, Stop, Location>.Case1 e:
                            Console.WriteLine($"Station {e.Item.name}");
                            break;
                        case U3<Station, Stop, Location>.Case2 e:
                            Console.WriteLine($"Stop {e.Item.name}");
                            break;
                        case U3<Station, Stop, Location>.Case3 e:
                            Console.WriteLine($"Location {e.Item.name}");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                */
            }
        }
    }
}
