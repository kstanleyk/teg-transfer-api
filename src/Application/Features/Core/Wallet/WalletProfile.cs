using AutoMapper;
using TegWallet.Application.Features.Core.Wallet.Command;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.Wallet;

public class WalletProfile : Profile
{
    public WalletProfile()
    {
        // Deposit mappings
        CreateMap<DepositRequestDto, DepositFundsCommand>();

        // Withdrawal mappings
        CreateMap<WithdrawalRequestDto, WithdrawFundsCommand>();

        CreateMap<ApproveDepositDto, ApproveDepositCommand>();

        // Ledger to TransactionDto
        CreateMap<Ledger, TransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.WalletId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.FailureReason, opt => opt.MapFrom(src => src.FailureReason));

        // Money to MoneyDto
        CreateMap<Money, MoneyDto>()
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency.Symbol));
    }
}