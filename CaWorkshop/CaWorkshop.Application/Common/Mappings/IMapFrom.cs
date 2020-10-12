using AutoMapper;

namespace CaWorkshop.Application.Common.Mappings
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile);
    }
}