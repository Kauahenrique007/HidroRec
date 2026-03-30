using AutoMapper;
using HidroRec.Backend.Application.DTOs.Admin;
using HidroRec.Backend.Application.DTOs.Alertas;
using HidroRec.Backend.Application.DTOs.Auth;
using HidroRec.Backend.Application.DTOs.Reportes;
using HidroRec.Backend.Application.DTOs.Usuarios;
using HidroRec.Backend.Domain.Entities;

namespace HidroRec.Backend.Configurations;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Perfil, opt => opt.MapFrom(src => src.Perfil.Nome));

        CreateMap<Usuario, UsuarioResumoDto>()
            .ForMember(dest => dest.Perfil, opt => opt.MapFrom(src => src.Perfil.Nome));

        CreateMap<Reporte, ReporteDto>()
            .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Bairro != null ? src.Bairro.Nome : src.BairroNome))
            .ForMember(dest => dest.Regiao, opt => opt.MapFrom(src => src.Regiao != null ? src.Regiao.Nome : src.RegiaoNome))
            .ForMember(dest => dest.ImagemUrl, opt => opt.MapFrom(src => src.CaminhoImagem))
            .ForMember(dest => dest.Historico, opt => opt.MapFrom(src => src.Historicos.OrderByDescending(h => h.DataAlteracao)));

        CreateMap<HistoricoReporte, HistoricoReporteDto>()
            .ForMember(dest => dest.AlteradoPor, opt => opt.MapFrom(src => src.AlteradoPorUsuario != null ? src.AlteradoPorUsuario.Nome : "Sistema"));

        CreateMap<Reporte, ReporteAdminItemDto>()
            .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Bairro != null ? src.Bairro.Nome : src.BairroNome))
            .ForMember(dest => dest.Regiao, opt => opt.MapFrom(src => src.Regiao != null ? src.Regiao.Nome : src.RegiaoNome));

        CreateMap<Alerta, AlertaDto>()
            .ForMember(dest => dest.Criticidade, opt => opt.MapFrom(src => src.Criticidade.ToString()))
            .ForMember(dest => dest.ReporteIds, opt => opt.MapFrom(src => src.AlertaReportes.Select(link => link.ReporteId)));

        CreateMap<Auditoria, AuditoriaDto>()
            .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nome : "Sistema"));

        CreateMap<LogSistema, LogSistemaDto>();
    }
}
