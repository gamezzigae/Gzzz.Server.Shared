namespace Gzzz;

public interface ITextLogger
{
	void Write(string message);
	void Write(object obj);
}

/*
 {
    "requestTime": "2025-08-18T16:00:49.4819233+00:00",
    "path": "/echo",
    "requestBody": "aaa",
    "ecode": 0,
    "emsg": "request body deserialize error",
    "statusCode": 400,
    "elapsed": 12
}
 */
