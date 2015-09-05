using System.Linq;
using AutoMapper;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario
{
    public class AutoMapperConfig
    {
        public static void RegisterTypes()
        {
            //Mapper.CreateMap<List<string>, string>().ConvertUsing(list => list.JoinStrings(", "));
            //Mapper.CreateMap<Location, string>().ConstructUsing(location => location.StoreId + " " + location.DepartmentId);
            //Mapper.CreateMap<Location, string>().ConvertUsing(new LocationTypeConverter());
            //Mapper.CreateMap<string, List<string>>().ConvertUsing(list => list.Split(new[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList());
        }
    }

    public class LocationTypeConverter : ITypeConverter<Location, string>
    {
        readonly IDbConnectionFactory _dbFactory;

        public LocationTypeConverter()
        {
            _dbFactory = NinjectWebCommon.Resolve<IDbConnectionFactory>();
        }

        public string Convert(ResolutionContext context)
        {
            var location = (Location) context.SourceValue;
            if (location == null)
            {
                return "N/A";
            }
            using (var db = _dbFactory.OpenDbConnection())
            {
                var store = db.Single<Store>(s => s.Id == location.StoreId);
                var department = store.Departments.Single(s => s.Id == location.DepartmentId);
                return store.Name + " " + department.Name;
            }
        }
    }
}