namespace Transfer.Application.Helpers.Exceptions;

public class BadRequestException(string message) : ApplicationException(message);