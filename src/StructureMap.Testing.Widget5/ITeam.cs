using Lamar;

namespace StructureMap.Testing.Widget5
{
    public interface ITeam
    {
        
    }

    public class TeamRegistry : ServiceRegistry
    {
        public TeamRegistry()
        {
            Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ITeam>();
            });
        }
    }
    
    public class Chiefs : ITeam{}
    public class Chargers : ITeam{}
    public class Broncos : ITeam{}
    public class Raiders : ITeam{}
    
    
}