using Api.Classes;
using Api.Factories;
using Api.Models.Api;
using Api.Models.EF;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserDB;

namespace Api
{
    /// <summary>
    /// Configuration of automapper, used to convert between db models to viewmodels
    /// </summary>
    public class AutoMapConfig
    {
        public static IMapper getMapper()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<User, UserViewModel>().ConvertUsing<UserTypeConverter>();
                c.CreateMap<UserViewModel, User>();

                c.CreateMap<Models.EF.Type, TypeViewModel>();
                c.CreateMap<TypeViewModel, Models.EF.Type>();

                c.CreateMap<People, PeopleViewModel>();
                c.CreateMap<PeopleViewModel, People>();

                c.CreateMap<Location, LocationViewModel>().ConvertUsing<LocationViewTypeConverter>();
                c.CreateMap<LocationViewModel, Location>().ConvertUsing<ViewLocationTypeConverter>();

                c.CreateMap<Coordinate, CoordinateViewModel>();
                c.CreateMap<CoordinateViewModel, Coordinate>();

                c.CreateMap<Accesspoint, AccesspointViewModel>().ConvertUsing<AccesspointTypeCoverter>();
                c.CreateMap<AccesspointViewModel, Accesspoint>().ConvertUsing<AccesspointViewTypeConverter>();

                c.CreateMap<PathPoint, PathPointViewModel>().ConvertUsing<PathPointTypeConverter>();
                c.CreateMap<PathPointViewModel, PathPoint>().ConvertUsing<PathPointViewTypeConverter>();

                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();
                //c.CreateMap<PathPointViewModel, PathNeighbour>().ConvertUsing<PathNeighbourTypeConverter>();

                c.CreateMap<SearchViewModel, IEnumerable<object>>().ConvertUsing<SearchTypeConverter>();
            });

            return config.CreateMapper();
        }         
    }

    public class UserTypeConverter : ITypeConverter<User, UserViewModel>
    {
        public UserViewModel Convert(ResolutionContext context)
        {
            User user = (User)context.SourceValue;

            IQueryable<UserLocation> uLocs;
            using (var userLocRepo = new LocationRepository<UserLocation>())
            using (var locRepo = new LocationRepository<Location>())
            {
                uLocs = userLocRepo.List().Where(u => u.UserId == user.Id);

                List<LocationViewModel> locs = new List<LocationViewModel>();


                foreach (UserLocation uLoc in uLocs)
                {
                    Location temp = locRepo.Read(uLoc.LocationId);
                    temp.Hits = uLoc.Hits;
                    locs.Add(AutoMapConfig.getMapper().Map<Location, LocationViewModel>(temp));
                }

                return new UserViewModel()
                {
                    Id = user.Id,
                    Username = user.Username,
                    Locations = locs.AsQueryable()
                };
            }
        }
    }

    public class UserLocationTypeConverter : ITypeConverter<UserLocation, LocationViewModel>
    {
        public LocationViewModel Convert(ResolutionContext context)
        {
            UserLocation userLocation = (UserLocation)context.SourceValue;

            Location location;
            using (var repo = new LocationRepository<Location>())
                location = repo.Read(userLocation.Id);

            using (var typeRepo = new LocationRepository<Models.EF.Type>())
            using (var coorRepo = new LocationRepository<Coordinate>())
                return new LocationViewModel()
                {
                    Id = location.Id,
                    Name = location.Name,
                    Desc = location.Desc,
                    Hits = userLocation.Hits,
                    LocNr = location.LocNr,
                    Type = AutoMapConfig.getMapper().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(location.TypeId)),
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(location.CoordinateId))
                };
        }
    }

    public class ViewLocationTypeConverter : ITypeConverter<LocationViewModel, Location>
    {
        public Location Convert(ResolutionContext context)
        {

            LocationViewModel source = (LocationViewModel)context.SourceValue;

            Coordinate cor;
            Location dest;
            using (var repo = new LocationRepository<Coordinate>())
            {
                if (source.Coordinate.Id == null)
                    source.Coordinate.Id = 0;

                if (source.Coordinate.Id.Value == 0)
                {
                    cor = repo.Create(new Coordinate() { Lat = source.Coordinate.Lat, Lng = source.Coordinate.Lng, Alt = source.Coordinate.Alt });
                }
                else
                {
                    cor = new Coordinate { Id = source.Coordinate.Id.Value };    
                }

                if(source.Id.Value == 0)
                source.Id = cor.Id;

                dest = new Location()
                  {
                        Id = source.Id.Value,
                        Name = source.Name,
                        Hits = source.Hits,
                        Desc = source.Desc,
                        LocNr = source.LocNr,
                        CoordinateId = cor.Id,
                        TypeId = source.Type.Id.Value,
                  };
            }
                

            return dest;
        }
    }

    public class LocationViewTypeConverter : ITypeConverter<Location, LocationViewModel>
    {
        public LocationViewModel Convert(ResolutionContext context)
        {

            Location source = (Location)context.SourceValue;
            LocationViewModel dest;
            using (var coorRepo = new LocationRepository<Coordinate>())
            using (var typeRepo = new LocationRepository<Models.EF.Type>())
                dest = new LocationViewModel()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Desc = source.Desc,
                    Hits = source.Hits,
                    LocNr = source.LocNr,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(source.CoordinateId)),
                    Type = AutoMapConfig.getMapper().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(source.TypeId))
                };

            return dest;
        }
    }

    public class AccesspointTypeCoverter : ITypeConverter<Accesspoint, AccesspointViewModel>
    {
        public AccesspointViewModel Convert(ResolutionContext context)
        {
            Accesspoint source = (Accesspoint)context.SourceValue;

            AccesspointViewModel dest = new AccesspointViewModel();
            using (var repo = new LocationRepository<Coordinate>())
                dest = new AccesspointViewModel()
                {
                    Id = source.Id,
                    Desc = source.Desc,
                    MacAddress = source.MacAddress,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(repo.Read(source.CoordinateId))

                };

            return dest;
        }
    }

    public class AccesspointViewTypeConverter : ITypeConverter<AccesspointViewModel, Accesspoint>
    {
        public Accesspoint Convert(ResolutionContext context)
        {
            AccesspointViewModel source = (AccesspointViewModel)context.SourceValue;
            Coordinate cor;

            if (source.Coordinate.Id == null)
                source.Coordinate.Id = 0;

            if (source.Coordinate.Id.Value == 0)
            {
                using (var repo = new LocationRepository<Coordinate>())
                    cor = repo.Create(new Coordinate() { Lat = source.Coordinate.Lat, Lng = source.Coordinate.Lng, Alt = source.Coordinate.Alt });
            }
            else
            {
                cor = new Coordinate { Id = source.Id.Value };
            }

            if (source.Id.Value == 0)
                source.Id = cor.Id;

            Accesspoint dest = new Accesspoint()
            {
                Id = source.Id.Value,
                Desc = source.Desc,
                MacAddress = source.MacAddress,
                CoordinateId = cor.Id
            };

            return dest;
        }
    }

    public class PathPointTypeConverter : ITypeConverter<PathPoint, PathPointViewModel>
    {
        public PathPointViewModel Convert(ResolutionContext context)
        {
            PathPoint source = (PathPoint)context.SourceValue;

            PathPointViewModel dest = new PathPointViewModel();

            List<PathPointViewModel> neighbours = new List<PathPointViewModel>();

            using(var pointRepo = new LocationRepository<PathPoint>())
            using(var pathRepo = new LocationRepository<PathNeighbour>())
            {
                var temp = pathRepo.List().Where(p => p.PathPointId1 == source.Id || p.PathPointId2 == source.Id).ToList();

                foreach(PathNeighbour path in temp)
                {
                    if(path.PathPointId1 == source.Id)
                    {
                        var point = pointRepo.Read(path.PathPointId2.Value);
                        neighbours.Add(new PathPointViewModel { Id = point.Id, Distance = path.length, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(point.Coordinate) });
                    }
                    else
                    {
                        var point = pointRepo.Read(path.PathPointId1.Value);
                        neighbours.Add(new PathPointViewModel { Id = point.Id, Distance = path.length, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(point.Coordinate) });
                    }
                }
            }


            using (var repo = new LocationRepository<Coordinate>())
                dest = new PathPointViewModel()
                {
                    Id = source.Id,
                    Distance = 0,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(repo.Read(source.CoordinateId)),
                    Neighbours = neighbours

                };

            return dest;
        }
    }

    public class PathPointViewTypeConverter : ITypeConverter<PathPointViewModel, PathPoint>
    {
        public PathPoint Convert(ResolutionContext context)
        {
            PathPointViewModel source = (PathPointViewModel)context.SourceValue;
            PathPoint dest;

            dest = new PathPoint()
            {
                Id = source.Id.Value,
                CoordinateId = source.Coordinate.Id.Value,
            };

            return dest;
        }
    }

    public class SearchTypeConverter : ITypeConverter<SearchViewModel, IEnumerable<object>>
    {
        public IEnumerable<object> Convert(ResolutionContext context)
        {
            SearchViewModel searchViewModel = (SearchViewModel)context.SourceValue;
            return new List<object>() { searchViewModel.LocationViewModel, searchViewModel.PeopleViewModel }.AsQueryable();
        }
    }
}