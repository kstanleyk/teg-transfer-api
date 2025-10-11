namespace TegWallet.Application.Helpers.Exceptions;

public class BadRequestException(string message) : ApplicationException(message);