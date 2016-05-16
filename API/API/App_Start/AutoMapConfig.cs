using Api.Classes;
using Api.Factories;
using Api.Models.Api;
using Api.Models.EF;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

                c.CreateMap<People, PeopleViewModel>().ConvertUsing<PeopleViewTypeConverter>();
                c.CreateMap<PeopleViewModel, People>().ConvertUsing<ViewPeopleTypeConverter>();

                c.CreateMap<Location, LocationViewModel>().ConvertUsing<LocationViewTypeConverter>();
                c.CreateMap<LocationViewModel, Location>().ConvertUsing<ViewLocationTypeConverter>();
                c.CreateMap<IEnumerable<Location>, IEnumerable<LocationViewModel>>().ConvertUsing<IEnumLocationViewTypeConverter>();

                c.CreateMap<Coordinate, CoordinateViewModel>();
                c.CreateMap<CoordinateViewModel, Coordinate>();

                c.CreateMap<Accesspoint, AccesspointViewModel>().ConvertUsing<AccesspointTypeCoverter>();
                c.CreateMap<AccesspointViewModel, Accesspoint>().ConvertUsing<AccesspointViewTypeConverter>();

                c.CreateMap<PathPoint, PathPointViewModel>().ConvertUsing<PathPointTypeConverter>();
                c.CreateMap<PathPointViewModel, PathPoint>().ConvertUsing<PathPointViewTypeConverter>();

                c.CreateMap<PathNeighbour, NeighbourViewModel>().ConvertUsing<PathNeighbourTypeConverter>();
                c.CreateMap<NeighbourViewModel, PathNeighbour>().ConvertUsing<PathNeighbourViewTypeConverter>();

                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();
                c.CreateMap<PathPoint, IEnumerable<NeighbourViewModel>>().ConvertUsing<NeighbourTypeConverter>();
                c.CreateMap<PathPoint, PathPointNeighbourViewModel>().ConvertUsing<PathPointNeighbourTypeConverter>();
                c.CreateMap<IEnumerable<PathPoint>, IEnumerable<PathPointNeighbourViewModel>>().ConvertUsing<IEnumPathPointNeighbourTypeConverter>();
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

    public class ViewPeopleTypeConverter : ITypeConverter<PeopleViewModel, People>
    {
        public People Convert(ResolutionContext context)
        {
            PeopleViewModel source = (PeopleViewModel)context.SourceValue;

            People dest = new People
            {
                Id = source.Id.Value,
                Name = source.Name,
                Desc = source.Desc,
                Email = source.Email,
                Jobtitle = source.Jobtitle,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                LocationId = source.Location.Id
            };

            return dest;
        }
    }

    public class PeopleViewTypeConverter : ITypeConverter<People, PeopleViewModel>
    {
        public PeopleViewModel Convert(ResolutionContext context)
        {
            People source = (People)context.SourceValue;
            PeopleViewModel dest;

            using (var repo = new LocationRepository<Location>())
            dest = new PeopleViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Desc = source.Desc,
                Email = source.Email,
                Jobtitle = source.Jobtitle,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                Location = AutoMapConfig.getMapper().Map<Location, LocationViewModel>(repo.Read(source.LocationId.Value))
            };

            return dest;
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
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(location.CoordinateId)),
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
                if (source.Coordinate == null)
                    source.Coordinate = new CoordinateViewModel { Id = 0 };

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

                if (source.Id != null)
                {
                    Location toReturn;
                    using (var locRepo = new LocationRepository<Location>())
                        toReturn = locRepo.Read(source.Id.Value);

                    if (source.Name != null)
                        toReturn.Name = source.Name;

                    if (source.LocNr != null)
                        toReturn.LocNr = source.LocNr;

                    if (source.Hits != 0)
                        toReturn.Hits = source.Hits;

                    if (source.Desc != null)
                        toReturn.Desc = source.Desc;

                    if (source.Type != null)
                        toReturn.TypeId = source.Type.Id.Value;

                    if (source.NeighbourId != 0)
                        toReturn.NeighbourId = source.NeighbourId;

                    if (source.Distance != 0)
                        toReturn.Distance = source.Distance;

                    toReturn.CoordinateId = cor.Id;

                    return toReturn;
                }


                dest = new Location()
                  {
                        Id = source.Id.Value,
                        Name = source.Name,
                        Hits = source.Hits,
                        Desc = source.Desc,
                        LocNr = source.LocNr,
                        CoordinateId = cor.Id,
                        TypeId = source.Type.Id.Value,
                        NeighbourId = source.NeighbourId,
                        Distance = source.Distance
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
                    Type = AutoMapConfig.getMapper().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(source.TypeId)),
                    Distance = source.Distance
                };

            if (source.NeighbourId == null)
            {
                dest.NeighbourId = 0;
            }
            else
            {
                dest.NeighbourId = source.NeighbourId.Value;
            }
                

            return dest;
        }
    }

    public class IEnumLocationViewTypeConverter : ITypeConverter<IEnumerable<Location>, IEnumerable<LocationViewModel>>
    {
        public IEnumerable<LocationViewModel> Convert(ResolutionContext context)
        {
            IEnumerable<Location> source = (IEnumerable<Location>)context.SourceValue;
            List<LocationViewModel> dest = new List<LocationViewModel>();

            IEnumerable<Coordinate> coordinates;
            IEnumerable<Models.EF.Type> types;

            using (var repo = new LocationRepository<Coordinate>())
                coordinates = repo.List().ToList();

            using (var repo = new LocationRepository<Models.EF.Type>())
                types = repo.List().ToList();

            foreach(Location loc in source)
            {
                var temp = new LocationViewModel()
                {
                    Id = loc.Id,
                    Name = loc.Name,
                    Desc = loc.Desc,
                    Hits = loc.Hits,
                    LocNr = loc.LocNr,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(coordinates.Where(c => c.Id == loc.CoordinateId).FirstOrDefault()),
                    Type = AutoMapConfig.getMapper().Map<Models.EF.Type, TypeViewModel>(types.Where(t => t.Id == loc.TypeId).FirstOrDefault()),
                    Distance = loc.Distance
                };

                if (loc.NeighbourId == null)
                {
                    temp.NeighbourId = 0;
                }
                else
                {
                    temp.NeighbourId = loc.NeighbourId.Value;
                }

                dest.Add(temp);
            }

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

    public class PathPointNeighbourTypeConverter : ITypeConverter<PathPoint, PathPointNeighbourViewModel>
    {
        public PathPointNeighbourViewModel Convert(ResolutionContext context)
        {
            PathPoint source = (PathPoint)context.SourceValue;
            PathPointNeighbourViewModel dest;

            List<object> neighbours = new List<object>();

            List<PathNeighbour> temp;

            using (var pathRepo = new LocationRepository<PathNeighbour>())
                temp = pathRepo.List().Where(p => p.PathPointId1 == source.Id || p.PathPointId2 == source.Id).Include(p => p.PathPoint1.Coordinate).Include(p => p.PathPoint2.Coordinate).ToList();

            foreach (PathNeighbour path in temp)
            {
                if (path.PathPointId1 == source.Id)
                {
                    var point = path.PathPoint2;
                    neighbours.Add(new { ID = point.Id, Distance = path.Distance, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(point.Coordinate) });
                }
                else
                {
                    var point = path.PathPoint1;
                    neighbours.Add(new { ID = point.Id, Distance = path.Distance, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(point.Coordinate) });
                }
            }

            dest = new PathPointNeighbourViewModel()
            {
                Id = source.Id,
                Neighbours = neighbours,
                Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(source.Coordinate)
            };

            return dest;

        }
    }

    public class IEnumPathPointNeighbourTypeConverter : ITypeConverter<IEnumerable<PathPoint>, IEnumerable<PathPointNeighbourViewModel>>
    {
        public IEnumerable<PathPointNeighbourViewModel> Convert(ResolutionContext context)
        {
            IEnumerable<PathPoint> source = (IEnumerable<PathPoint>)context.SourceValue;
            List<PathPointNeighbourViewModel> dest = new List<PathPointNeighbourViewModel>();

            List<PathNeighbour> pathNeighbours;
            using (var repo = new LocationRepository<PathNeighbour>())
                pathNeighbours = repo.List().Include(p => p.PathPoint1.Coordinate).Include(p => p.PathPoint2.Coordinate).ToList();

            List<Location> locations;
            using (var repo = new LocationRepository<Location>())
                locations = repo.List().Where(l => l.NeighbourId != null).Include(l => l.Coordinate).ToList();

            foreach (PathPoint point in source)
            {
                List<object> neighbours = new List<object>();

                if (point.NeighbourCount != 0) {
                    List<PathNeighbour> temp = pathNeighbours.Where(p => p.PathPointId1 == point.Id || p.PathPointId2 == point.Id).ToList();

                    Location neighbour = locations.Where(l => l.NeighbourId.Value == point.Id).FirstOrDefault();
                    if (neighbour != null)
                        neighbours.Add(new { ID = neighbour.Id, Distance = neighbour.Distance, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(neighbour.Coordinate) });

                    foreach (PathNeighbour path in temp)
                    {
                        if (neighbours.Count == point.NeighbourCount)
                            break;

                        if (path.PathPointId1 == point.Id)
                        {
                            var pathPoint = path.PathPoint2;
                            neighbours.Add(new { ID = pathPoint.Id, Distance = path.Distance, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(pathPoint.Coordinate) });
                        }
                        else
                        {
                            var pathPoint = path.PathPoint1;
                            neighbours.Add(new { ID = pathPoint.Id, Distance = path.Distance, Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(pathPoint.Coordinate) });
                        }
                    }
                }
                dest.Add(new PathPointNeighbourViewModel()
                {
                    Id = point.Id,
                    Neighbours = neighbours,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(point.Coordinate)
                });
            }

            return dest;

        }
    }

    public class PathNeighbourTypeConverter : ITypeConverter<PathNeighbour, NeighbourViewModel>
    {
        public NeighbourViewModel Convert(ResolutionContext context)
        {
            PathNeighbour source = (PathNeighbour)context.SourceValue;
            NeighbourViewModel dest;

            using (var repo = new LocationRepository<PathPoint>())
            dest = new NeighbourViewModel
            {
                Id = source.Id,
                Distance = source.Distance,
                pathPoint1 = AutoMapConfig.getMapper().Map<PathPoint, PathPointViewModel>(repo.Read(source.PathPointId1.Value)),
                pathPoint2 = AutoMapConfig.getMapper().Map<PathPoint, PathPointViewModel>(repo.Read(source.PathPointId2.Value)),
            };

            return dest;
        }
    }

    public class PathNeighbourViewTypeConverter : ITypeConverter<NeighbourViewModel, PathNeighbour>
    {
        public PathNeighbour Convert(ResolutionContext context)
        {
            NeighbourViewModel source = (NeighbourViewModel)context.SourceValue;

            if (source.Id == null)
                source.Id = 0;

            PathNeighbour dest = new PathNeighbour
            {
                Id = source.Id.Value,
                Distance = source.Distance,
                PathPointId1 = source.pathPoint1.Id,
                PathPointId2 = source.pathPoint2.Id
            };

            return dest;
        }
    }

    public class NeighbourTypeConverter : ITypeConverter<PathPoint, IEnumerable<NeighbourViewModel>>
    {
        public IEnumerable<NeighbourViewModel> Convert(ResolutionContext context)
        {
            PathPoint source = (PathPoint)context.SourceValue;


            List<NeighbourViewModel> dest = new List<NeighbourViewModel>();

            List<PathPointViewModel> neighbours = new List<PathPointViewModel>();

            using (var pointRepo = new LocationRepository<PathPoint>())
            using (var pathRepo = new LocationRepository<PathNeighbour>())
            {
                var temp = pathRepo.List().Where(p => p.PathPointId1 == source.Id || p.PathPointId2 == source.Id).ToList();

                foreach (PathNeighbour path in temp)
                {
                    dest.Add(AutoMapConfig.getMapper().Map<PathNeighbour, NeighbourViewModel>(path));
                }
            }

            return dest;
        }
    }

    public class PathPointTypeConverter : ITypeConverter<PathPoint, PathPointViewModel>
    {
        public PathPointViewModel Convert(ResolutionContext context)
        {
            PathPoint source = (PathPoint)context.SourceValue;


            PathPointViewModel dest = new PathPointViewModel();

            using (var repo = new LocationRepository<Coordinate>())
                dest = new PathPointViewModel()
                {
                    Id = source.Id,
                    Coordinate = AutoMapConfig.getMapper().Map<Coordinate, CoordinateViewModel>(source.Coordinate),
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
            Coordinate cor;

            if (source.Id == null)
                source.Id = 0;

            if (source.Coordinate == null)
                source.Coordinate = new CoordinateViewModel { Id = 0 };

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

            dest = new PathPoint()
            {
                Id = source.Id.Value,
                Coordinate = cor,
                CoordinateId = cor.Id,
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