using AutoMapper;
using TegWallet.Application.Features.Core.Wallets.Command;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Features.Core.Wallets.Model;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Ledgers;

public class LedgerProfile : Profile
{
    public LedgerProfile()
    {
        // Ledger to LedgerDto
        CreateMap<Domain.Entity.Core.Ledger, LedgerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.WalletId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason));
    }

}