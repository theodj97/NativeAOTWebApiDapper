namespace WebApiDapperNativeAOT.Models.Exceptions;

public class BadRequestException(string message) : Exception(message) { }